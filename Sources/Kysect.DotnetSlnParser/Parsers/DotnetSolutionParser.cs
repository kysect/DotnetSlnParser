using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetSlnParser.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Kysect.DotnetSlnParser.Parsers;

public class DotnetSolutionParser
{
    private readonly IFileSystem _fileSystem;
    private readonly SolutionFileParser _solutionFileParser;
    private readonly ProjectFileParser _projectFileParser;
    private readonly ILogger _logger;

    public DotnetSolutionParser(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem;
        _solutionFileParser = new SolutionFileParser(logger);
        _projectFileParser = new ProjectFileParser(fileSystem, logger);
        _logger = logger;
    }

    public DotnetSolutionDescriptor Parse(string filePath)
    {
        filePath.ThrowIfNull(nameof(filePath));

        _logger.LogInformation("Parsing solution {solutionPath}", filePath);
        var solutionFileFullPath = _fileSystem.FileInfo.New(filePath).FullName;

        if (!_fileSystem.File.Exists(solutionFileFullPath))
            throw new DotnetSlnParseException($"Cannot parse solution {solutionFileFullPath}. File not found.");

        IDirectoryInfo? solutionDirectory = _fileSystem.Directory.GetParent(solutionFileFullPath);
        if (solutionDirectory is null)
            throw new DotnetSlnParseException("Cannot get solution parent directory");

        string slnFileContent = _fileSystem.File.ReadAllText(solutionFileFullPath);

        var projectFileDescriptors = _solutionFileParser.ParseSolutionFileContent(slnFileContent).ToList();
        var projects = new Dictionary<string, DotnetProjectFileContent>();
        foreach (DotnetProjectFileDescriptor? projectFileDescriptor in projectFileDescriptors)
        {
            string projectFullPath = _fileSystem.Path.Combine(solutionDirectory.FullName, projectFileDescriptor.ProjectPath);

            _logger.LogTrace("Parsing project {path}", projectFullPath);
            DotnetProjectFileContent? projectDescriptor = _projectFileParser.ReadAndParse(projectFullPath);
            if (projectDescriptor is not null)
                projects[projectFullPath] = projectDescriptor.Value;
        }

        _logger.LogInformation("Solution parsed and contains {projectCount}", projectFileDescriptors.Count);
        return new DotnetSolutionDescriptor(solutionFileFullPath, projects);
    }
}