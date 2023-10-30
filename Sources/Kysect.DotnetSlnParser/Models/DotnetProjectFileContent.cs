using System.Collections.Generic;

namespace Kysect.DotnetSlnParser.Models;

public record DotnetProjectFileContent(
    string? TargetFramework,
    bool EnableDefaultItems,
    IReadOnlyCollection<string> IncludedFiles,
    IReadOnlyCollection<string> References);