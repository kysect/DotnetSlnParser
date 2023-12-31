﻿using Kysect.DotnetSlnParser.Models;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using Microsoft.Language.Xml;

namespace Kysect.DotnetSlnParser.Parsers;

public class ProjectFileParser(IFileSystem fileSystem, ILogger logger)
{
    public DotnetProjectFileContent? ReadAndParse(string path)
    {
        logger.LogInformation("Parsing project structure for {path}", path);
        string csprojContent = fileSystem.File.ReadAllText(path);
        return ParseContent(csprojContent);
    }
    public DotnetProjectFileContent? ParseContent(string csprojContent)
    {
        XmlDocumentSyntax root = Parser.ParseText(csprojContent);
        XmlProjectFileAccessor xmlProjectFileAccessor = new XmlProjectFileAccessor(root, logger);

        IXmlElementSyntax projectNode = xmlProjectFileAccessor.Single("Project");
        XmlAttributeSyntax? toolsVersionAttribute = projectNode.GetAttribute("ToolsVersion");
        if (toolsVersionAttribute is not null)
        {
            logger.LogWarning("Legacy xml format is not supported");
            return null;
        }

        bool enableDefaultItems = xmlProjectFileAccessor.FindBoolPropertyValue("EnableDefaultItems") ?? true;
        string? targetFramework = xmlProjectFileAccessor.FindPropertyValue("TargetFramework");

        List<string> sources = xmlProjectFileAccessor
            .GetNodesByName("Compile")
            .Select(n => n.GetAttributeValue("Include"))
            .Where(n => n is not null)
            .ToList();

        List<string> references = xmlProjectFileAccessor
            .GetNodesByName("ProjectReference")
            .Select(n => n.GetAttributeValue("Include"))
            .Where(n => n is not null)
            .ToList();

        logger.LogInformation("Project structure parsed. Source files: {fileCount}, references: {referenceCount}", sources.Count, references.Count);

        return new DotnetProjectFileContent(targetFramework, enableDefaultItems, sources, references);
    }
}