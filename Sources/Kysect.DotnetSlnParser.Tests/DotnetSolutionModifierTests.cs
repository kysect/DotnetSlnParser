using FluentAssertions;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetSlnGenerator;
using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Parsers;
using Kysect.DotnetSlnParser.Tests.Tools;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetSlnParser.Tests;

public class DotnetSolutionModifierTests
{
    private MockFileSystem _fileSystem;
    private ILogger _logger;

    [SetUp]
    public void Setup()
    {
        _logger = DefaultLoggerConfiguration.CreateConsoleLogger();
        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
    }

    [Test]
    public void CreateModifier_ReturnFullPathToProjects()
    {
        string projectContent = """
                                <Project Sdk="Microsoft.NET.Sdk">
                                  <PropertyGroup>
                                    <TargetFramework>net8.0</TargetFramework>
                                  </PropertyGroup>
                                </Project>
                                """;

        string projectName = "SampleProject";
        string solutionSln = "Solution.sln";
        string currentPath = _fileSystem.Path.GetFullPath("SolutionDirectory");
        string solutionPath = _fileSystem.Path.Combine(currentPath, solutionSln);

        _fileSystem.Directory.CreateDirectory(currentPath);

        var solutionBuilder = new DotnetSolutionBuilder("Solution")
            .AddProject(new DotnetProjectBuilder(projectName, projectContent));
        solutionBuilder.Save(_fileSystem, currentPath);

        var solutionModifier = DotnetSolutionModifier.Create(solutionPath, _fileSystem, _logger, new SolutionFileParser(_logger));

        solutionModifier.Projects.Single().Path.Should().Be(_fileSystem.Path.Combine(currentPath, projectName, $"{projectName}.csproj"));
    }

    [Test]
    public void Save_WithoutChanges_FinishWithoutErrors()
    {
        string projectContent = """
                                <Project Sdk="Microsoft.NET.Sdk">
                                  <PropertyGroup>
                                    <TargetFramework>net8.0</TargetFramework>
                                    <ImplicitUsings>enable</ImplicitUsings>
                                    <Nullable>enable</Nullable>
                                  </PropertyGroup>

                                  <ItemGroup>
                                    <PackageReference Include="FluentAssertions" />
                                    <PackageReference Include="Microsoft.NET.Test.Sdk" />
                                  </ItemGroup>
                                </Project>
                                """;

        string currentPath = _fileSystem.Path.GetFullPath(".");

        var solutionBuilder = new DotnetSolutionBuilder("Solution")
            .AddProject(
                new DotnetProjectBuilder("SampleProject", projectContent));
        solutionBuilder.Save(_fileSystem, currentPath);

        var solutionModifier = DotnetSolutionModifier.Create("Solution.sln", _fileSystem, _logger, new SolutionFileParser(_logger));
        solutionModifier.Save();
    }

    [Test]
    public void Save_AfterChangingTargetFramework_ChangeFileContentToExpected()
    {
        string projectContent = """
                                <Project Sdk="Microsoft.NET.Sdk">
                                  <PropertyGroup>
                                    <TargetFramework>net8.0</TargetFramework>
                                  </PropertyGroup>
                                </Project>
                                """;

        var expectedProjectContent = """
                                     <Project Sdk="Microsoft.NET.Sdk">
                                       <PropertyGroup>
                                         <TargetFramework>net9.0</TargetFramework>
                                       </PropertyGroup>
                                     </Project>
                                     """;

        string currentPath = _fileSystem.Path.GetFullPath(".");
        var solutionBuilder = new DotnetSolutionBuilder("Solution")
            .AddProject(
                new DotnetProjectBuilder("SampleProject", projectContent));
        solutionBuilder.Save(_fileSystem, currentPath);

        var solutionModifier = DotnetSolutionModifier.Create("Solution.sln", _fileSystem, _logger, new SolutionFileParser(_logger));

        foreach (DotnetProjectModifier solutionModifierProject in solutionModifier.Projects)
            solutionModifierProject.Accessor.UpdateDocument(new SetTargetFrameworkModifyStrategy("net9.0"));

        solutionModifier.Save();

        string fullPathToProjectFile = Path.Combine(@"SampleProject", "SampleProject.csproj");
        _fileSystem.File.ReadAllText(fullPathToProjectFile).Should().Be(expectedProjectContent);
    }
}