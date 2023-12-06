using Kysect.DotnetSlnParser.Parsers;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.DotnetSlnParser.Modifiers;

public class DotnetPropsModifier
{
    private readonly string _path;
    private readonly IFileSystem _fileSystem;
    private readonly Lazy<XmlProjectFileAccessor> _fileAccessor;
    public XmlProjectFileAccessor Accessor => _fileAccessor.Value;

    public DotnetPropsModifier(string path, IFileSystem fileSystem, ILogger logger)
    {
        _path = path;
        _fileSystem = fileSystem;
        _fileAccessor = new Lazy<XmlProjectFileAccessor>(() => XmlProjectFileAccessor.Create(_path, _fileSystem, logger));
    }

    public void Save()
    {
        if (!_fileAccessor.IsValueCreated)
            return;

        _fileSystem.File.WriteAllText(_path, _fileAccessor.Value.ToFullString());
    }
}