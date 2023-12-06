using Kysect.CommonLib.BaseTypes.Extensions;
using System.IO.Abstractions;

namespace Kysect.DotnetSlnGenerator;

public class DotnetSolutionBuilder
{
    private readonly string _solutionName;
    private readonly List<DotnetProjectBuilder> _projects;

    public DotnetSolutionBuilder(string solutionName)
    {
        _solutionName = solutionName;
        _projects = new List<DotnetProjectBuilder>();
    }

    public DotnetSolutionBuilder AddProject(DotnetProjectBuilder project)
    {
        _projects.Add(project);
        return this;
    }

    public void Save(IFileSystem fileSystem, string rootPath)
    {
        fileSystem.ThrowIfNull();

        string solutionFileContent = CreateSolutionFile(fileSystem);
        fileSystem.File.WriteAllText(fileSystem.Path.Combine(rootPath, $"{_solutionName}.sln"), solutionFileContent);

        foreach (DotnetProjectBuilder projectBuilder in _projects)
            projectBuilder.Save(fileSystem, rootPath);
    }

    public string CreateSolutionFile(IFileSystem fileSystem)
    {
        fileSystem.ThrowIfNull();

        var solutionFileStringBuilder = new SolutionFileStringBuilder();

        foreach (DotnetProjectBuilder projectBuilder in _projects)
            solutionFileStringBuilder.AddProject(projectBuilder.ProjectName, fileSystem.Path.Combine(projectBuilder.ProjectName, $"{projectBuilder.ProjectName}.csproj"));

        return solutionFileStringBuilder.Build();
    }
}