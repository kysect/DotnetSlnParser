using Microsoft.Language.Xml;
using System;
using System.IO.Abstractions;

namespace Kysect.DotnetSlnParser.Modifiers;

public class DotnetPropsModifier
{
    private readonly string _path;
    private readonly IFileSystem _fileSystem;

    private readonly Lazy<XmlDocumentSyntax> _document;

    public DotnetPropsModifier(string path, IFileSystem fileSystem)
    {
        _path = path;
        _fileSystem = fileSystem;
        _document = new Lazy<XmlDocumentSyntax>(LoadDocument);
    }

    public void Save()
    {
        if (!_document.IsValueCreated)
            return;

        _fileSystem.File.WriteAllText(_path, _document.Value.ToFullString());
    }

    private XmlDocumentSyntax LoadDocument()
    {
        string fileContent = _fileSystem.File.ReadAllText(_path);
        return Parser.ParseText(fileContent);
    }
}