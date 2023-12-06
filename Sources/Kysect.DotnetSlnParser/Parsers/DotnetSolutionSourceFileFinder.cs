using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Logging;
using Kysect.DotnetSlnParser.Models;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.DotnetSlnParser.Parsers;

public record DotnetSolutionPaths(string SolutionFileFullPath, IReadOnlyCollection<DotnetProjectPaths> ProjectPaths);
public record DotnetProjectPaths(string ProjectFileFullPath, IReadOnlyCollection<string> SourceFileFullPaths);

public class DotnetSolutionSourceFileFinder
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public DotnetSolutionSourceFileFinder(IFileSystem fileSystem, ILogger logger)
    {
        _logger = logger;
        _fileSystem = fileSystem;
    }

    public DotnetSolutionPaths FindSourceFiles(DotnetSolutionDescriptor solutionDescriptor)
    {
        solutionDescriptor.ThrowIfNull();

        _logger.LogInformation("Extract source file paths for solution {path}", solutionDescriptor.FilePath);
        _fileSystem.File.Exists(solutionDescriptor.FilePath);

        var projectPaths = new List<DotnetProjectPaths>();

        foreach (KeyValuePair<string, DotnetProjectFileContent> descriptorProject in solutionDescriptor.Projects)
        {
            IFileInfo projectFileInfo = _fileSystem.FileInfo.New(descriptorProject.Key);
            if (projectFileInfo.Directory == null)
                throw new DotnetSlnParseException($"Cannot get project directory for {descriptorProject.Key}");

            _logger.LogInformation("Adding files from csproj");
            var projectFileFullPaths = descriptorProject.Value
                .IncludedFiles
                .Select(p => _fileSystem.Path.Combine(projectFileInfo.Directory.FullName, p))
                .ToList();

            _logger.LogDebug("Added files: ");
            foreach (string projectFileFullPath in projectFileFullPaths)
                _logger.LogTabDebug(1, projectFileFullPath);

            if (descriptorProject.Value.EnableDefaultItems)
            {
                string binDirectoryPath = Path.Combine(projectFileInfo.Directory.FullName, "bin");
                string objDirectoryPath = Path.Combine(projectFileInfo.Directory.FullName, "obj");

                _logger.LogInformation("Default items enabled. Trying to add files in directory");
                var defaultItems = _fileSystem.Directory
                    .EnumerateFiles(projectFileInfo.Directory.FullName, "*", SearchOption.AllDirectories)
                    .Where(p => p != projectFileInfo.FullName)
                    .Where(p => !p.StartsWith(binDirectoryPath))
                    .Where(p => !p.StartsWith(objDirectoryPath))
                    .ToList();

                _logger.LogDebug("Added files: ");
                foreach (string defaultItem in defaultItems)
                    _logger.LogTabDebug(1, defaultItem);

                projectFileFullPaths.AddRange(defaultItems);
            }

            projectPaths.Add(new DotnetProjectPaths(descriptorProject.Key, projectFileFullPaths));
        }

        return new DotnetSolutionPaths(solutionDescriptor.FilePath, projectPaths);
    }
}