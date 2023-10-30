using FluentAssertions;
using Kysect.DotnetSlnParser.Models;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetSlnParser.Tests;

public class DotnetSolutionSourceFileFinderTests
{
    private DotnetSolutionStructureParser _solutionStructureParser;
    private DotnetSolutionSourceFileFinder _sourceFileFinder;
    private MockFileSystem _fileSystem;

    [SetUp]
    public void Setup()
    {
        ILogger logger = TestLogger.Create();

        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        _solutionStructureParser = new DotnetSolutionStructureParser(_fileSystem, logger);
        _sourceFileFinder = new DotnetSolutionSourceFileFinder(_fileSystem, logger);
    }

    [Test]
    public void FindSourceFiles_ProjectWithDefaultItems_ReturnExpectedResult()
    {
        var solutionContent = """
                              Microsoft Visual Studio Solution File, Format Version 12.00
                              # Visual Studio Version 17
                              VisualStudioVersion = 17.0.31903.59
                              MinimumVisualStudioVersion = 10.0.40219.1
                              Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "SampleProject", "SampleProject\SampleProject.csproj", "{20453538-0E86-4A56-9369-E7FF1AA75CC9}"
                              EndProject
                              Global
                              	GlobalSection(SolutionConfigurationPlatforms) = preSolution
                              		Debug|Any CPU = Debug|Any CPU
                              		Release|Any CPU = Release|Any CPU
                              	EndGlobalSection
                              	GlobalSection(ProjectConfigurationPlatforms) = postSolution
                              		{20453538-0E86-4A56-9369-E7FF1AA75CC9}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                              		{20453538-0E86-4A56-9369-E7FF1AA75CC9}.Debug|Any CPU.Build.0 = Debug|Any CPU
                              		{20453538-0E86-4A56-9369-E7FF1AA75CC9}.Release|Any CPU.ActiveCfg = Release|Any CPU
                              		{20453538-0E86-4A56-9369-E7FF1AA75CC9}.Release|Any CPU.Build.0 = Release|Any CPU
                              	EndGlobalSection
                              	GlobalSection(SolutionProperties) = preSolution
                              		HideSolutionNode = FALSE
                              	EndGlobalSection
                              EndGlobal
                              
                              """;

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
}