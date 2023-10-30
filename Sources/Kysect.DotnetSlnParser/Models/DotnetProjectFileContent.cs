using System.Collections.Generic;

namespace Kysect.DotnetSlnParser.Models;

public record struct DotnetProjectFileContent(
    string? TargetFramework,
    bool EnableDefaultItems,
    IReadOnlyCollection<string> IncludedFiles,
    IReadOnlyCollection<string> References);