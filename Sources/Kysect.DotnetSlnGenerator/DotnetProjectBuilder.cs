using Kysect.CommonLib.BaseTypes.Extensions;
using System.IO.Abstractions;

namespace Kysect.DotnetSlnGenerator;

public class DotnetProjectBuilder
{
    private readonly string _projectFileContent;
    private readonly List<string[]> _files;

    public string ProjectName { get; }

    public DotnetProjectBuilder(string projectName, string projectFileContent)
    {
        ProjectName = projectName;
        _projectFileContent = projectFileContent;

        _files = new List<string[]>();
    }

    public DotnetProjectBuilder AddEmptyFile(params string[] path)
    {
        _files.Add(path);
        return this;
    }

    public void Save(IFileSystem fileSystem, string rootPath)
    {
        fileSystem.ThrowIfNull();

        fileSystem.EnsureDirectoryExists(fileSystem.Path.Combine(rootPath, ProjectName));
        string csprojPath = fileSystem.Path.Combine(rootPath, ProjectName, $"{ProjectName}.csproj");
        fileSystem.File.WriteAllText(csprojPath, _projectFileContent);

        foreach (string[] path in _files)
        {
            string[] fileFullPathParts = [rootPath, ProjectName, .. path];
            string fileFullPath = fileSystem.Path.Combine(fileFullPathParts);
            IFileInfo fileInfo = fileSystem.FileInfo.New(fileFullPath);
            fileSystem.EnsureContainingDirectoryExists(fileInfo);

            fileSystem.File.WriteAllText(fileFullPath, string.Empty);
        }
    }
}