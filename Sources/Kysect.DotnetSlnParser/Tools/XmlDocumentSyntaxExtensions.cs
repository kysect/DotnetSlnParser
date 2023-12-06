using Microsoft.Language.Xml;

namespace Kysect.DotnetSlnParser.Tools;

public static class XmlDocumentSyntaxExtensions
{
    public static IReadOnlyCollection<IXmlElementSyntax> GetNodesByName(this XmlDocumentSyntax document, string name)
    {
        return document
            .Descendants()
            .Where(n => n.Name == name)
            .ToList();
    }
}