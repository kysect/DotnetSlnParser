﻿using FluentAssertions;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.DotnetSlnParser.Models;
using Kysect.DotnetSlnParser.Parsers;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.DotnetSlnParser.Tests;

public class ProjectFileParserTests
{
    private readonly ProjectFileParser _parser;

    public ProjectFileParserTests()
    {
        ILogger logger = DefaultLoggerConfiguration.CreateConsoleLogger();

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

        _parser = new ProjectFileParser(fileSystem, logger);
    }

    [Test]
    public void ParseProjectFileContent_WithAutoIncludeCsproj_ReturnExpectedResult()
    {
        var csprojContent = """
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
                            
                              <ItemGroup>
                                <ProjectReference Include="..\Kysect.DotnetSlnParser\Kysect.DotnetSlnParser.csproj" />
                              </ItemGroup>
                            </Project>
                            """;

        var expected = new DotnetProjectFileContent(
            TargetFramework: "net8.0",
            EnableDefaultItems: true,
            IncludedFiles: Array.Empty<string>(),
            References: new[] { "..\\Kysect.DotnetSlnParser\\Kysect.DotnetSlnParser.csproj" });

        DotnetProjectFileContent? result = _parser.ParseContent(csprojContent);

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ParseProjectFileContent_WithoutAutoIncludeCsproj_ReturnExpectedResult()
    {
        var csprojContent = """
                            <Project Sdk="Microsoft.NET.Sdk">
                              <PropertyGroup>
                                <TargetFramework>net8.0</TargetFramework>
                                <ImplicitUsings>enable</ImplicitUsings>
                                <Nullable>enable</Nullable>
                                <EnableDefaultItems>false</EnableDefaultItems>
                              </PropertyGroup>
                            
                              <ItemGroup>
                                <PackageReference Include="FluentAssertions" />
                                <PackageReference Include="Microsoft.NET.Test.Sdk" />
                              </ItemGroup>
                            
                              <ItemGroup>
                                <Compile Include="Program.cs" />
                              </ItemGroup>
                            </Project>
                            """;

        var expected = new DotnetProjectFileContent(
            TargetFramework: "net8.0",
            EnableDefaultItems: false,
            IncludedFiles: new[] { "Program.cs" },
            References: Array.Empty<string>());

        DotnetProjectFileContent? result = _parser.ParseContent(csprojContent);

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ParseProjectFileContent_LegacyCsproj_ReturnExpectedResult()
    {
        var csprojContent = """
                            <?xml version="1.0" encoding="utf-8"?>
                            <Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
                              <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
                              <ItemGroup>
                                <Reference Include="System" />
                              </ItemGroup>
                              <ItemGroup>
                                <Compile Include="Program.cs" />
                                <Compile Include="Properties\AssemblyInfo.cs" />
                              </ItemGroup>
                              <ItemGroup>
                                <None Include="App.config" />
                              </ItemGroup>
                              <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
                            </Project>
                            """;

        DotnetProjectFileContent? result = _parser.ParseContent(csprojContent);
        result.Should().BeNull();
    }
}