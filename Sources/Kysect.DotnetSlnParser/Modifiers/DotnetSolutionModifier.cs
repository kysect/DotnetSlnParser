﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetSlnParser.Models;
using Kysect.DotnetSlnParser.Parsers;
using Kysect.DotnetSlnParser.Tools;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.DotnetSlnParser.Modifiers;

public class DotnetSolutionModifier
{
    public IReadOnlyCollection<DotnetProjectModifier> Projects { get; }
    public DotnetPropsModifier DirectoryBuildPropsModifier { get; }
    public DotnetPropsModifier DirectoryPackagePropsModifier { get; }

    public static DotnetSolutionModifier Create(string solutionPath, IFileSystem fileSystem, ILogger logger, SolutionFileParser solutionFileParser)
    {
        solutionPath.ThrowIfNull();
        fileSystem.ThrowIfNull();
        logger.ThrowIfNull();
        solutionFileParser.ThrowIfNull();

        IFileInfo fileInfo = fileSystem.FileInfo.New(solutionPath);
        fileInfo.Directory.ThrowIfNull();

        var directoryBuildPropsModifier = new DotnetPropsModifier(fileSystem.Path.Combine(fileInfo.Directory.FullName, SolutionItemNameConstants.DirectoryBuildProps), fileSystem, logger);
        var directoryPackagePropsModifier = new DotnetPropsModifier(fileSystem.Path.Combine(fileInfo.Directory.FullName, SolutionItemNameConstants.DirectoryPackagesProps), fileSystem, logger);

        string solutionFileContent = fileSystem.File.ReadAllText(solutionPath);
        IReadOnlyCollection<DotnetProjectFileDescriptor> projectFileDescriptors = solutionFileParser.ParseSolutionFileContent(solutionFileContent);
        var projects = new List<DotnetProjectModifier>();

        foreach (DotnetProjectFileDescriptor projectFileDescriptor in projectFileDescriptors)
        {
            string projectFullPath = fileSystem.Path.Combine(fileInfo.Directory.FullName, projectFileDescriptor.ProjectPath);
            var projectModifier = new DotnetProjectModifier(projectFullPath, fileSystem, logger);
            bool supportModification = projectModifier.SupportModification();
            if (!supportModification)
                logger.LogWarning("Project {Path} use legacy csproj format and will be skipped.", projectModifier.Path);
            else
                projects.Add(projectModifier);
        }

        return new DotnetSolutionModifier(projects, directoryBuildPropsModifier, directoryPackagePropsModifier);
    }

    public DotnetSolutionModifier(IReadOnlyCollection<DotnetProjectModifier> projects, DotnetPropsModifier directoryBuildPropsModifier, DotnetPropsModifier directoryPackagePropsModifier)
    {
        Projects = projects;
        DirectoryBuildPropsModifier = directoryBuildPropsModifier;
        DirectoryPackagePropsModifier = directoryPackagePropsModifier;
    }

    public void Save()
    {
        DirectoryBuildPropsModifier.Save();
        DirectoryPackagePropsModifier.Save();

        foreach (DotnetProjectModifier projectModifier in Projects)
            projectModifier.Save();
    }
}