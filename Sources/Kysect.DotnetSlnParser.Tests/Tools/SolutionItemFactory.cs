﻿using System.Text;

namespace Kysect.DotnetSlnParser.Tests.Tools;

public static class SolutionItemFactory
{
    public static string CreateSolutionFile(params (string ProjectName, string ProjectPath)[] projectDefinitions)
    {
        string[] projects = projectDefinitions
            .Select(p =>
                $$"""
                  Project("{{{Guid.Empty}}}") = "{{p.ProjectName}}", "{{p.ProjectPath}}", "{{{Guid.Empty}}}"
                  EndProject
                  """)
            .ToArray();

        return CreateSolutionFile(projects);
    }

    public static string CreateSolutionFile(params string[] projectDefinitions)
    {
        var header = """
                     Microsoft Visual Studio Solution File, Format Version 12.00
                     # Visual Studio Version 17
                     VisualStudioVersion = 17.0.31903.59
                     MinimumVisualStudioVersion = 10.0.40219.1
                     """;

        var footer = """
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

        StringBuilder builder = new StringBuilder()
            .AppendLine(header)
            .AppendJoin(Environment.NewLine, projectDefinitions)
            .AppendLine()
            .AppendLine(footer);

        return builder.ToString();
    }
}