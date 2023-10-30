using System.Xml;
using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetSlnParser.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Kysect.DotnetSlnParser;

public class DotnetProjectStructureParser
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public DotnetProjectStructureParser(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem.ThrowIfNull();
        _logger = logger.ThrowIfNull();
    }

    public DotnetProjectFileContent ReadAndParse(string path)
    {
        _logger.LogInformation("Parsing project structure for {path}", path);
        string csprojContent = _fileSystem.File.ReadAllText(path);
        return ParseContent(csprojContent);
    }

    public DotnetProjectFileContent ParseContent(string csprojContent)
    {
        var sources = new List<string>();
        var references = new List<string>();

        var document = new XmlDocument();
        document.LoadXml(csprojContent);

        XmlNode? projectNode = document.DocumentElement;
        if (projectNode is null)
            throw new ArgumentException($"Cannot load xml, document element collection is null.");

        if (IsLegacyXmlFormat(projectNode))
            throw new DotnetSlnParseException("Legacy xml format is not supported");

        bool enableDefaultItems = IsEnableDefaultItems(projectNode);
        string? targetFramework = FindTargetFramework(projectNode);

        IReadOnlyCollection<XmlNode> itemGroupNodes = GerChild(projectNode, "ItemGroup");

        foreach (XmlNode node in itemGroupNodes)
        {
            foreach (XmlNode s in node.ChildNodes.Cast<XmlNode>())
            {
                if (s.Name == "Compile")
                {
                    string filePath = GetAttributeValue(s, "Include");
                    sources.Add(filePath);
                }

                if (s.Name == "ProjectReference")
                {
                    string referencePath = GetAttributeValue(s, "Include");
                    references.Add(referencePath);
                }
            }
        }

        _logger.LogInformation("Project structure parsed. Source files: {fileCount}, references: {referenceCount}", sources.Count, references.Count);

        return new DotnetProjectFileContent(targetFramework, enableDefaultItems, sources, references);
    }

    private bool IsLegacyXmlFormat(XmlNode rootNode)
    {
        XmlNode projectNode = rootNode;
        string? toolsVersion = FindAttributeValue(projectNode, "ToolsVersion");

        return toolsVersion is not null;
    }

    private bool IsEnableDefaultItems(XmlNode rootNode)
    {
        IReadOnlyCollection<XmlNode> itemGroupNodes = GerChild(rootNode, "PropertyGroup");
        foreach (XmlNode itemGroupNode in itemGroupNodes)
        {
            IReadOnlyCollection<XmlNode> enableDefaultItemNodes = GerChild(itemGroupNode, "EnableDefaultItems");
            foreach (XmlNode enableDefaultItemNode in enableDefaultItemNodes)
            {
                if (bool.TryParse(enableDefaultItemNode.InnerText, out var enableDefaultItem))
                    return enableDefaultItem;
            }
        }

        return true;
    }

    private string? FindTargetFramework(XmlNode rootNode)
    {
        IReadOnlyCollection<XmlNode> itemGroupNodes = GerChild(rootNode, "PropertyGroup");
        foreach (XmlNode itemGroupNode in itemGroupNodes)
        {
            IReadOnlyCollection<XmlNode> enableDefaultItemNodes = GerChild(itemGroupNode, "TargetFramework");
            foreach (XmlNode enableDefaultItemNode in enableDefaultItemNodes)
            {
                return enableDefaultItemNode.InnerText;
            }
        }

        return null;
    }

    private static IReadOnlyCollection<XmlNode> GerChild(XmlNode parent, string name)
    {
        return parent
            .ChildNodes
            .Cast<XmlNode>()
            .Where(n => n.Name == name)
            .ToList();
    }

    private static string GetAttributeValue(XmlNode node, string attrName)
    {
        return FindAttributeValue(node, attrName).ThrowIfNull(attrName);
    }

    private static string? FindAttributeValue(XmlNode node, string name)
    {
        if (node.Attributes is null)
            return null;

        XmlAttribute? attribute = node
            .Attributes
            .Cast<XmlAttribute>()
            .FirstOrDefault(n => string.Equals(n.Name, name, StringComparison.InvariantCultureIgnoreCase));

        return attribute?.Value;
    }
}