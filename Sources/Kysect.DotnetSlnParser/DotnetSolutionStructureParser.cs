using System.Text.RegularExpressions;
using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Logging;
using Kysect.DotnetSlnParser.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Kysect.DotnetSlnParser;

public class DotnetSolutionStructureParser
{
    private static readonly Regex ProjectPattern = new Regex(
        @"Project\(\""(?<typeGuid>.*?)\""\)\s+=\s+\""(?<name>.*?)\"",\s+\""(?<path>.*?)\"",\s+\""(?<guid>.*?)\""(?<content>.*?)\bEndProject\b",
        RegexOptions.ExplicitCapture | RegexOptions.Singleline);

    private readonly IFileSystem _fileSystem;
    private readonly DotnetProjectStructureParser _projectStructureParser;
    private readonly ILogger _logger;

    public DotnetSolutionStructureParser(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem;
        _logger = logger.WithPrefix("DotnetSolutionStructureParser");

        _projectStructureParser = new DotnetProjectStructureParser(fileSystem, _logger);
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

        List<DotnetProjectFileDescriptor> projectFileDescriptors = ParseSolutionFileContent(slnFileContent).ToList();
        var projects = new Dictionary<string, DotnetProjectFileContent>();
        foreach (DotnetProjectFileDescriptor? projectFileDescriptor in projectFileDescriptors)
        {
            string projectFullPath = _fileSystem.Path.Combine(solutionDirectory.FullName, projectFileDescriptor.ProjectPath);

            _logger.LogTrace("Parsing project {path}", projectFullPath);
            DotnetProjectFileContent? projectDescriptor = _projectStructureParser.ReadAndParse(projectFullPath);
            if (projectDescriptor is not null)
                projects[projectFullPath] = projectDescriptor.Value;
        }

        _logger.LogInformation("Solution parsed and contains {projectCount}", projectFileDescriptors.Count);
        return new DotnetSolutionDescriptor(solutionFileFullPath, projects);
    }

    public IEnumerable<DotnetProjectFileDescriptor> ParseSolutionFileContent(string solutionContents)
    {
        Match match = ProjectPattern.Match(solutionContents);

        while (match.Success)
        {
            string projectName = match.Groups["name"].Value;
            string projectPath = match.Groups["path"].Value;
            string projectIdString = match.Groups["guid"].Value;
            string projectTypeIdString = match.Groups["typeGuid"].Value;

            bool isPathToProject = projectPath.EndsWith("proj");

            if (isPathToProject)
            {
                if (projectPath.EndsWith("vdproj"))
                {
                    _logger.LogTrace("vdproj is not supported. Skip project {path}", projectPath);
                }
                else
                {
                    _logger.LogDebug("Parsing project row with name {ProjectName}", projectName);

                    if (!Guid.TryParse(projectIdString, out Guid projectId))
                        throw new DotnetSlnParseException($"Project id {projectTypeIdString} is not valid id");

                    if (!Guid.TryParse(projectTypeIdString, out Guid projectTypeId))
                        throw new DotnetSlnParseException($"Project type id {projectTypeIdString} is not valid id");

                    var project = new DotnetProjectFileDescriptor(projectTypeId, projectName, projectPath, projectId);
                    yield return project;
                }
            }

            match = match.NextMatch();
        }
    }
}