using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetSlnParser.Parsers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Kysect.DotnetSlnParser.Modifiers;

public class DotnetSolutionModifier
{
    private readonly IFileSystem _fileSystem;
    private readonly SolutionFileParser _solutionFileParser;
    private readonly ILogger _logger;

    private readonly string _solutionPath;
    private readonly DotnetPropsModifier _directoryBuildPropsModifier;
    private readonly DotnetPropsModifier _directoryPackagePropsModifier;
    private readonly Lazy<IReadOnlyCollection<DotnetProjectModifier>> _projects;

    public IReadOnlyCollection<DotnetProjectModifier> Projects => _projects.Value;

    public DotnetSolutionModifier(string solutionPath, IFileSystem fileSystem, ILogger logger, SolutionFileParser solutionFileParser)
    {
        _solutionFileParser = solutionFileParser.ThrowIfNull();
        _solutionPath = solutionPath.ThrowIfNull();
        _fileSystem = fileSystem.ThrowIfNull();
        _logger = logger.ThrowIfNull();

        IFileInfo fileInfo = fileSystem.FileInfo.New(solutionPath);
        fileInfo.Directory.ThrowIfNull();
        _directoryBuildPropsModifier = new DotnetPropsModifier(_fileSystem.Path.Combine(fileInfo.Directory.FullName, "Directory.Build.props"), _fileSystem);
        _directoryPackagePropsModifier = new DotnetPropsModifier(_fileSystem.Path.Combine(fileInfo.Directory.FullName, "Directory.Package.props"), _fileSystem);
        _projects = new Lazy<IReadOnlyCollection<DotnetProjectModifier>>(ParseProjects);
    }

    public void Save()
    {
        _directoryBuildPropsModifier.Save();
        _directoryPackagePropsModifier.Save();

        if (_projects.IsValueCreated)
        {
            foreach (DotnetProjectModifier projectModifier in _projects.Value)
                projectModifier.Save();
        }
    }

    private IReadOnlyCollection<DotnetProjectModifier> ParseProjects()
    {
        string solutionFileContent = _fileSystem.File.ReadAllText(_solutionPath);
        var projects = _solutionFileParser
            .ParseSolutionFileContent(solutionFileContent)
            .Select(p => new DotnetProjectModifier(p.ProjectPath, _fileSystem, _logger))
            .Where(p =>
            {
                bool supportModification = p.SupportModification();
                if (!supportModification)
                    _logger.LogWarning("Project {Path} use legacy csproj format and will be skipped.", p.Path);
                return supportModification;
            })
            .ToList();

        return projects;
    }
}