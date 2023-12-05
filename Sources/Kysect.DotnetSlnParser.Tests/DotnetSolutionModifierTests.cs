using FluentAssertions;
using Kysect.CommonLib.DependencyInjection.Logging;
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
    public void Save_WithoutChanges_FinishWithoutErrors()
    {
        string solutionContent = SolutionItemFactory.CreateSolutionFile(("SampleProject", @"SampleProject\SampleProject.csproj"));

        var projectContent = """
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

        string projectDirectoryPath = @"C:\SampleProject";
        string fullPathToProjectFile = Path.Combine(projectDirectoryPath, "SampleProject.csproj");

        _fileSystem.AddFile(@"C:\Solution.sln", new MockFileData(solutionContent));
        _fileSystem.AddDirectory(projectDirectoryPath);
        _fileSystem.AddFile(fullPathToProjectFile, new MockFileData(projectContent));

        var solutionModifier = DotnetSolutionModifier.Create("Solution.sln", _fileSystem, _logger, new SolutionFileParser(_logger));

        solutionModifier.Save();
    }

    [Test]
    public void Save_AfterChangingTargetFramework_ChangeFileContentToExpected()
    {
        string solutionContent = SolutionItemFactory.CreateSolutionFile(("SampleProject", @"SampleProject\SampleProject.csproj"));

        var projectContent = """
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

        string projectDirectoryPath = @"C:\SampleProject";
        string fullPathToProjectFile = Path.Combine(projectDirectoryPath, "SampleProject.csproj");

        _fileSystem.AddFile(@"C:\Solution.sln", new MockFileData(solutionContent));
        _fileSystem.AddDirectory(projectDirectoryPath);
        _fileSystem.AddFile(fullPathToProjectFile, new MockFileData(projectContent));

        var solutionModifier = DotnetSolutionModifier.Create("Solution.sln", _fileSystem, _logger, new SolutionFileParser(_logger));

        foreach (DotnetProjectModifier solutionModifierProject in solutionModifier.Projects)
            solutionModifierProject.Accessor.UpdateDocument(new SetTargetFrameworkModifyStrategy("net9.0"));

        solutionModifier.Save();

        _fileSystem.File.ReadAllText(fullPathToProjectFile).Should().Be(expectedProjectContent);
    }
}