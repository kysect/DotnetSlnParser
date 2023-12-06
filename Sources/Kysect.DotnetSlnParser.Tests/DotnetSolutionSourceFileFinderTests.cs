using FluentAssertions;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetSlnGenerator;
using Kysect.DotnetSlnParser.Models;
using Kysect.DotnetSlnParser.Parsers;
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
        string fullPathToFirstFile = _fileSystem.Path.Combine(currentPath, "SampleProject", "File1.cs");
        string pathToSecondFile = _fileSystem.Path.Combine(currentPath, "SampleProject", "InnerDirectory", "File2.cs");

        var expectedProjectPaths = new DotnetProjectPaths(
            _fileSystem.Path.Combine(currentPath, "SampleProject", "SampleProject.csproj"),
            new[] { fullPathToFirstFile, pathToSecondFile });

        var expected = new DotnetSolutionPaths(
            _fileSystem.Path.Combine(currentPath, "Solution.sln"),
            new[] { expectedProjectPaths });

        var solutionBuilder = new DotnetSolutionBuilder("Solution")
            .AddProject(
                new DotnetProjectBuilder("SampleProject", projectContent)
                    .AddEmptyFile("File1.cs")
                    .AddEmptyFile("InnerDirectory", "File2.cs"));

        solutionBuilder.Save(_fileSystem, currentPath);
        DotnetSolutionDescriptor dotnetSolutionDescriptor = _solutionStructureParser.Parse("Solution.sln");
        DotnetSolutionPaths dotnetSolutionPaths = _sourceFileFinder.FindSourceFiles(dotnetSolutionDescriptor);

        dotnetSolutionPaths.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void FindSourceFiles_ProjectWithDefaultItemsAndBinObjDirectories_ReturnExpectedResult()
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
        string fullPathToFirstFile = _fileSystem.Path.Combine(currentPath, "SampleProject", "File1.cs");
        string pathToSecondFile = _fileSystem.Path.Combine(currentPath, "SampleProject", "InnerDirectory", "File2.cs");

        var expectedProjectPaths = new DotnetProjectPaths(
            _fileSystem.Path.Combine(currentPath, "SampleProject", "SampleProject.csproj"),
            new[] { fullPathToFirstFile, pathToSecondFile });

        var expected = new DotnetSolutionPaths(
            _fileSystem.Path.Combine(currentPath, "Solution.sln"),
            new[] { expectedProjectPaths });

        var solutionBuilder = new DotnetSolutionBuilder("Solution")
            .AddProject(
                new DotnetProjectBuilder("SampleProject", projectContent)
                    .AddEmptyFile("File1.cs")
                    .AddEmptyFile("InnerDirectory", "File2.cs")
                    .AddEmptyFile("bin", "Bin.cs")
                    .AddEmptyFile("obj", "Obj.cs"));

        solutionBuilder.Save(_fileSystem, currentPath);
        DotnetSolutionDescriptor dotnetSolutionDescriptor = _solutionStructureParser.Parse("Solution.sln");
        DotnetSolutionPaths dotnetSolutionPaths = _sourceFileFinder.FindSourceFiles(dotnetSolutionDescriptor);

        dotnetSolutionPaths.Should().BeEquivalentTo(expected);
    }
}