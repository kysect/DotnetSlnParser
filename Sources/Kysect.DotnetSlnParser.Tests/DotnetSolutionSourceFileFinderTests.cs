using FluentAssertions;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetSlnParser.Models;
using Kysect.DotnetSlnParser.Parsers;
using Kysect.DotnetSlnParser.Tests.Tools;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetSlnParser.Tests;

public class DotnetSolutionSourceFileFinderTests
{
    private DotnetSolutionParser _solutionStructureParser;
    private DotnetSolutionSourceFileFinder _sourceFileFinder;
    private MockFileSystem _fileSystem;

    [SetUp]
    public void Setup()
    {
        ILogger logger = DefaultLoggerConfiguration.CreateConsoleLogger();

        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        _solutionStructureParser = new DotnetSolutionParser(_fileSystem, logger);
        _sourceFileFinder = new DotnetSolutionSourceFileFinder(_fileSystem, logger);
    }

    [Test]
    public void FindSourceFiles_ProjectWithDefaultItems_ReturnExpectedResult()
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
        string fullPathToFirstFile = Path.Combine(projectDirectoryPath, "File1.cs");
        string pathToInnerDirectory = Path.Combine(projectDirectoryPath, "InnerDirectory");
        string pathToSecondFile = Path.Combine(projectDirectoryPath, "InnerDirectory", "File2.cs");

        _fileSystem.AddFile(@"C:\Solution.sln", new MockFileData(solutionContent));
        _fileSystem.AddDirectory(projectDirectoryPath);
        _fileSystem.AddFile(fullPathToProjectFile, new MockFileData(projectContent));
        _fileSystem.AddEmptyFile(fullPathToFirstFile);
        _fileSystem.AddDirectory(pathToInnerDirectory);
        _fileSystem.AddEmptyFile(pathToSecondFile);

        var expectedProjectPaths = new DotnetProjectPaths(
            @"C:\SampleProject\SampleProject.csproj",
            new[] { fullPathToFirstFile, pathToSecondFile });

        var expected = new DotnetSolutionPaths(
            @"C:\Solution.sln",
            new[] { expectedProjectPaths });


        DotnetSolutionDescriptor dotnetSolutionDescriptor = _solutionStructureParser.Parse("Solution.sln");
        DotnetSolutionPaths dotnetSolutionPaths = _sourceFileFinder.FindSourceFiles(dotnetSolutionDescriptor);

        dotnetSolutionPaths.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void FindSourceFiles_ProjectWithDefaultItemsAndBinObjDirectories_ReturnExpectedResult()
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
        string fullPathToFirstFile = Path.Combine(projectDirectoryPath, "File1.cs");
        string pathToInnerDirectory = Path.Combine(projectDirectoryPath, "InnerDirectory");
        string pathToSecondFile = Path.Combine(projectDirectoryPath, "InnerDirectory", "File2.cs");

        string pathToFileInBin = Path.Combine(projectDirectoryPath, "bin", "Bin.cs");
        string pathToFileInObj = Path.Combine(projectDirectoryPath, "obj", "Obj.cs");

        _fileSystem.AddFile(@"C:\Solution.sln", new MockFileData(solutionContent));
        _fileSystem.AddDirectory(projectDirectoryPath);
        _fileSystem.AddFile(fullPathToProjectFile, new MockFileData(projectContent));
        _fileSystem.AddEmptyFile(fullPathToFirstFile);
        _fileSystem.AddEmptyFile(pathToFileInBin);
        _fileSystem.AddEmptyFile(pathToFileInObj);
        _fileSystem.AddDirectory(pathToInnerDirectory);
        _fileSystem.AddEmptyFile(pathToSecondFile);

        var expectedProjectPaths = new DotnetProjectPaths(
            @"C:\SampleProject\SampleProject.csproj",
            new[] { fullPathToFirstFile, pathToSecondFile });

        var expected = new DotnetSolutionPaths(
            @"C:\Solution.sln",
            new[] { expectedProjectPaths });


        DotnetSolutionDescriptor dotnetSolutionDescriptor = _solutionStructureParser.Parse("Solution.sln");
        DotnetSolutionPaths dotnetSolutionPaths = _sourceFileFinder.FindSourceFiles(dotnetSolutionDescriptor);

        dotnetSolutionPaths.Should().BeEquivalentTo(expected);
    }
}