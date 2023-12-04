using Microsoft.Extensions.Logging;
using Microsoft.Language.Xml;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.DotnetSlnParser.Parsers;

public class XmlProjectFileAccessor(XmlDocumentSyntax document, ILogger logger)
{
    public IXmlElementSyntax Single(string name)
    {
        return SingleOrDefault(name) ?? throw DotnetSlnParseException.PropertyNotFound(name);
    }

    public IXmlElementSyntax? SingleOrDefault(string name)
    {
        IReadOnlyCollection<IXmlElementSyntax> elements = GetNodes(name);

        if (elements.Count > 1)
            throw new DotnetSlnParseException($"Unexpected count of {name} nodes: {elements.Count}");

        return elements.SingleOrDefault();
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
        IReadOnlyCollection<IXmlElementSyntax> elements = GetNodes(propertyName);

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

    public IReadOnlyCollection<IXmlElementSyntax> GetNodes(string name)
    {
        return document
            .Descendants()
            .Where(n => n.Name == name)
            .ToList();
    }

    public string ToFullString()
    {
        return document.ToFullString();
    }
}