using FluentAssertions;
using Kysect.CommonLib.DependencyInjection;
using Kysect.DotnetSlnParser.Models;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetSlnParser.Tests;

public class DotnetSolutionStructureParserTests
{
    private readonly DotnetSolutionStructureParser _parser;

    public DotnetSolutionStructureParserTests()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        ILogger logger = PredefinedLogger.CreateConsoleLogger();

        _parser = new DotnetSolutionStructureParser(fileSystem, logger);
    }

    [Test]
    public void ParseSolutionFileContent_ThisSolution_ReturnExpectedResult()
    {
        var solutionContent = """
                              Microsoft Visual Studio Solution File, Format Version 12.00
                              # Visual Studio Version 17
                              VisualStudioVersion = 17.0.31903.59
                              MinimumVisualStudioVersion = 10.0.40219.1
                              Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Kysect.DotnetSlnParser", "Kysect.DotnetSlnParser\Kysect.DotnetSlnParser.csproj", "{20453538-0E86-4A56-9369-E7FF1AA75CC9}"
                              EndProject
                              Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Kysect.DotnetSlnParser.Tests", "Kysect.DotnetSlnParser.Tests\Kysect.DotnetSlnParser.Tests.csproj", "{16F41CB6-D59A-4FDD-9AB0-7D0FB5687BC5}"
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
                              		{16F41CB6-D59A-4FDD-9AB0-7D0FB5687BC5}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                              		{16F41CB6-D59A-4FDD-9AB0-7D0FB5687BC5}.Debug|Any CPU.Build.0 = Debug|Any CPU
                              		{16F41CB6-D59A-4FDD-9AB0-7D0FB5687BC5}.Release|Any CPU.ActiveCfg = Release|Any CPU
                              		{16F41CB6-D59A-4FDD-9AB0-7D0FB5687BC5}.Release|Any CPU.Build.0 = Release|Any CPU
                              	EndGlobalSection
                              	GlobalSection(SolutionProperties) = preSolution
                              		HideSolutionNode = FALSE
                              	EndGlobalSection
                              EndGlobal
                              
                              """;

        List<DotnetProjectFileDescriptor> expected = new List<DotnetProjectFileDescriptor>
        {
            new DotnetProjectFileDescriptor(
                Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"),
                "Kysect.DotnetSlnParser",
                "Kysect.DotnetSlnParser\\Kysect.DotnetSlnParser.csproj",
                Guid.Parse("20453538-0E86-4A56-9369-E7FF1AA75CC9")),

            new DotnetProjectFileDescriptor(
                Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"),
                "Kysect.DotnetSlnParser.Tests",
                "Kysect.DotnetSlnParser.Tests\\Kysect.DotnetSlnParser.Tests.csproj",
                Guid.Parse("16F41CB6-D59A-4FDD-9AB0-7D0FB5687BC5")),
        };

        List<DotnetProjectFileDescriptor> projects = _parser.ParseSolutionFileContent(solutionContent).ToList();

        projects.Should().BeEquivalentTo(expected);
    }
}