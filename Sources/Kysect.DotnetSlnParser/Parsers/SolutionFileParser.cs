using System.Text.RegularExpressions;
using Kysect.DotnetSlnParser.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Kysect.DotnetSlnParser.Parsers;

public class SolutionFileParser(ILogger logger)
{
    private static readonly Regex ProjectPattern = new Regex(
        @"Project\(\""(?<typeGuid>.*?)\""\)\s+=\s+\""(?<name>.*?)\"",\s+\""(?<path>.*?)\"",\s+\""(?<guid>.*?)\""(?<content>.*?)\bEndProject\b",
        RegexOptions.ExplicitCapture | RegexOptions.Singleline);

    public IEnumerable<DotnetProjectFileDescriptor> ParseSolutionFileContent(string solutionContents)
    {
        Match match = ProjectPattern.Match(solutionContents);

        while (match.Success)
        {
            string projectName = match.Groups["name"].Value;
            string projectPath = match.Groups["path"].Value;
            string projectIdString = match.Groups["guid"].Value;
            string projectTypeIdString = match.Groups["typeGuid"].Value;

            bool isPathToProject = projectPath.EndsWith("proj");

            if (isPathToProject)
            {
                if (projectPath.EndsWith("vdproj"))
                    logger.LogTrace("vdproj is not supported. Skip project {path}", projectPath);
                else
                {
                    logger.LogDebug("Parsing project row with name {ProjectName}", projectName);

                    if (!Guid.TryParse(projectIdString, out Guid projectId))
                        throw new DotnetSlnParseException($"Project id {projectTypeIdString} is not valid id");

                    if (!Guid.TryParse(projectTypeIdString, out Guid projectTypeId))
                        throw new DotnetSlnParseException($"Project type id {projectTypeIdString} is not valid id");

                    var project = new DotnetProjectFileDescriptor(projectTypeId, projectName, projectPath, projectId);
                    yield return project;
                }
            }

            match = match.NextMatch();
        }
    }
}