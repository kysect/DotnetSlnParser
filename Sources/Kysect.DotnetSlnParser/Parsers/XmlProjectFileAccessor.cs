using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Kysect.DotnetSlnParser.Parsers;

public class XmlProjectFileAccessor(XmlDocumentSyntax document, ILogger logger)
{
    public static XmlProjectFileAccessor Create(string path, IFileSystem fileSystem, ILogger logger)
    {
        path.ThrowIfNull();
        fileSystem.ThrowIfNull();
        logger.ThrowIfNull();

        string csprojContent =
            fileSystem.File.Exists(path)
                ? fileSystem.File.ReadAllText(path)
                : string.Empty;

        XmlDocumentSyntax root = Parser.ParseText(csprojContent);
        return new XmlProjectFileAccessor(root, logger);
    }

    public void UpdateDocument(Func<XmlDocumentSyntax, XmlDocumentSyntax> morphism)
    {
        morphism.ThrowIfNull();

        document = morphism(document);
    }

    public void UpdateDocument<TSyntax>(IXmlProjectFileModifyStrategy<TSyntax> modifyStrategy)
        where TSyntax : SyntaxNode
    {
        modifyStrategy.ThrowIfNull();

        IReadOnlyCollection<TSyntax> nodes = modifyStrategy.Select(document);

        document = document.ReplaceNodes(nodes, (_, n) => modifyStrategy.ApplyChanges(n));
    }

    public IXmlElementSyntax Single(string name)
    {
        return SingleOrDefault(name) ?? throw DotnetSlnParseException.PropertyNotFound(name);
    }

    public IXmlElementSyntax? SingleOrDefault(string name)
    {
        IReadOnlyCollection<IXmlElementSyntax> elements = GetNodesByName(name);

        if (elements.Count > 1)
            throw new DotnetSlnParseException($"Unexpected count of {name} nodes: {elements.Count}");

        return elements.SingleOrDefault();
    }

    public IReadOnlyCollection<IXmlElementSyntax> GetNodesByName(string name)
    {
        return document.GetNodesByName(name);
    }

    public string GetPropertyValue(string propertyName)
    {
        string? value = FindPropertyValue(propertyName);

        if (value is null)
            throw DotnetSlnParseException.PropertyNotFound(propertyName);

        return value;
    }

    public string? FindPropertyValue(string propertyName)
    {
        IReadOnlyCollection<IXmlElementSyntax> elements = GetNodesByName(propertyName);

        if (elements.Count > 1)
            logger.LogWarning("Xml file contains more that one node with name {Name}", propertyName);

        return elements.FirstOrDefault()?.Content.ToFullString();
    }

    public bool GetBoolPropertyValue(string propertyName)
    {
        bool? value = FindBoolPropertyValue(propertyName);
        if (value is null)
            throw DotnetSlnParseException.PropertyNotFound(propertyName);

        return value.Value;
    }

    public bool? FindBoolPropertyValue(string propertyName)
    {
        string? value = FindPropertyValue(propertyName);

        if (value is null)
            return null;

        if (!bool.TryParse(value, out bool result))
            throw new DotnetSlnParseException($"Property {propertyName} cannot be parsed");

        return result;
    }

    public string ToFullString()
    {
        return document.ToFullString();
    }
}