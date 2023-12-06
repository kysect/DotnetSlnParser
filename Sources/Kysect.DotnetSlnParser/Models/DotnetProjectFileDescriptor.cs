namespace Kysect.DotnetSlnParser.Models;

public record DotnetProjectFileDescriptor(Guid ProjectTypeGuid, string ProjectName, string ProjectPath, Guid ProjectGuid);