using System.Collections.Generic;

namespace Kysect.DotnetSlnParser.Models;

public record DotnetSolutionDescriptor(string FilePath, Dictionary<string, DotnetProjectFileContent> Projects);