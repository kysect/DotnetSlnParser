using Kysect.DotnetSlnParser.Parsers;
using Microsoft.Extensions.Logging;
using Microsoft.Language.Xml;
using System;
using System.IO.Abstractions;

namespace Kysect.DotnetSlnParser.Modifiers;

public class DotnetProjectModifier
{
    public string Path => _path;

    private readonly string _path;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly Lazy<XmlProjectFileAccessor> _fileAccessor;

    public XmlProjectFileAccessor Accessor => _fileAccessor.Value;

    public DotnetProjectModifier(string path, IFileSystem fileSystem, ILogger logger)
    {
        _path = path;
        _fileSystem = fileSystem;
        _logger = logger;

        _fileAccessor = new Lazy<XmlProjectFileAccessor>(CreateFileAccessor);
    }

    public bool SupportModification()
    {
        XmlProjectFileAccessor xmlProjectFileAccessor = _fileAccessor.Value;
        IXmlElementSyntax projectNode = xmlProjectFileAccessor.Single("Project");
        XmlAttributeSyntax? toolsVersionAttribute = projectNode.GetAttribute("ToolsVersion");
        return toolsVersionAttribute is null;
    }

    public void Save()
    {
        if (!_fileAccessor.IsValueCreated)
            return;

        _fileSystem.File.WriteAllText(_path, _fileAccessor.Value.ToFullString());
    }

    private XmlProjectFileAccessor CreateFileAccessor()
    {
        string csprojContent = _fileSystem.File.ReadAllText(_path);
        XmlDocumentSyntax root = Parser.ParseText(csprojContent);
        return new XmlProjectFileAccessor(root, _logger);
    }
}