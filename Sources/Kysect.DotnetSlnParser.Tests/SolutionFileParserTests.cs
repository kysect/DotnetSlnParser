using FluentAssertions;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetSlnParser.Models;
using Kysect.DotnetSlnParser.Parsers;
using Kysect.DotnetSlnParser.Tests.Tools;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

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

        string solutionContent = SolutionItemFactory.CreateSolutionFile(
            ("Kysect.DotnetSlnParser", @"Kysect.DotnetSlnParser\Kysect.DotnetSlnParser.csproj"),
            ("Kysect.DotnetSlnParser.Tests", @"Kysect.DotnetSlnParser.Tests\Kysect.DotnetSlnParser.Tests.csproj")
        );

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

        List<DotnetProjectFileDescriptor> projects = _parser.ParseSolutionFileContent(solutionContent).ToList();

        projects.Should().BeEquivalentTo(expected);
    }
}