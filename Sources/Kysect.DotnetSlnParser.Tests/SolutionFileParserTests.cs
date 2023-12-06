using FluentAssertions;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetSlnGenerator;
using Kysect.DotnetSlnParser.Models;
using Kysect.DotnetSlnParser.Parsers;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetSlnParser.Tests;

public class SolutionFileParserTests
{
    private readonly SolutionFileParser _parser;

    public SolutionFileParserTests()
    {
        ILogger logger = DefaultLoggerConfiguration.CreateConsoleLogger();

        _parser = new SolutionFileParser(logger);
    }

    [Test]
    public void ParseSolutionFileContent_ThisSolution_ReturnExpectedResult()
    {
        var solutionBuilder = new DotnetSolutionBuilder("Solution")
            .AddProject(new DotnetProjectBuilder("Kysect.DotnetSlnParser", string.Empty))
            .AddProject(new DotnetProjectBuilder("Kysect.DotnetSlnParser.Tests", string.Empty));

        var fileSystem = new MockFileSystem();
        string solutionFile = solutionBuilder.CreateSolutionFile(fileSystem);

        List<DotnetProjectFileDescriptor> expected = new List<DotnetProjectFileDescriptor>
        {
            new DotnetProjectFileDescriptor(
                Guid.Empty,
                "Kysect.DotnetSlnParser",
                "Kysect.DotnetSlnParser\\Kysect.DotnetSlnParser.csproj",
                Guid.Empty),

            new DotnetProjectFileDescriptor(
                Guid.Empty,
                "Kysect.DotnetSlnParser.Tests",
                "Kysect.DotnetSlnParser.Tests\\Kysect.DotnetSlnParser.Tests.csproj",
                Guid.Empty),
        };

        List<DotnetProjectFileDescriptor> projects = _parser.ParseSolutionFileContent(solutionFile).ToList();

        projects.Should().BeEquivalentTo(expected);
    }
}