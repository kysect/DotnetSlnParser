using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetSlnParser.Parsers;
using Microsoft.Extensions.Logging;
using Microsoft.Language.Xml;
using System.IO.Abstractions;

namespace Kysect.DotnetSlnParser.Modifiers;

public class DotnetProjectModifier
{
    public string Path { get; }
    public XmlProjectFileAccessor Accessor { get; }

    private readonly IFileSystem _fileSystem;

    public DotnetProjectModifier(string path, IFileSystem fileSystem, ILogger logger)
    {
        Path = path.ThrowIfNull();
        _fileSystem = fileSystem.ThrowIfNull();

        if (!fileSystem.File.Exists(path))
            throw new ArgumentException($"Project file with path {path} was not found");

        Accessor = XmlProjectFileAccessor.Create(Path, _fileSystem, logger);
    }

    public bool SupportModification()
    {
        IXmlElementSyntax projectNode = Accessor.Single("Project");
        XmlAttributeSyntax? toolsVersionAttribute = projectNode.GetAttribute("ToolsVersion");
        return toolsVersionAttribute is null;
    }

    public void Save()
    {
        _fileSystem.File.WriteAllText(Path, Accessor.ToFullString());
    }
}