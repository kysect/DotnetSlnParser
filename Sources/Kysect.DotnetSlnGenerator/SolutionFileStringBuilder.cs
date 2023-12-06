using System.Text;

namespace Kysect.DotnetSlnGenerator;

public class SolutionFileStringBuilder
{
    private readonly StringBuilder _builder;

    public SolutionFileStringBuilder()
    {
        var header = """
                     Microsoft Visual Studio Solution File, Format Version 12.00
                     # Visual Studio Version 17
                     VisualStudioVersion = 17.0.31903.59
                     MinimumVisualStudioVersion = 10.0.40219.1
                     """;

        _builder = new StringBuilder();
        _builder.AppendLine(header);
    }

    public SolutionFileStringBuilder AddProject(string projectName, string projectPath)
    {
        string projectDefinition = $$"""
                                     Project("{{{Guid.Empty}}}") = "{{projectName}}", "{{projectPath}}", "{{{Guid.Empty}}}"
                                     EndProject
                                     """;

        _builder.AppendLine(projectDefinition);
        return this;
    }

    public string Build()
    {
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

        _builder.AppendLine(footer);
        return _builder.ToString();
    }
}