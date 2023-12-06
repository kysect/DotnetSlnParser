namespace Kysect.DotnetSlnParser;

public class DotnetSlnParseException : Exception
{
    public DotnetSlnParseException()
    {
    }

    public DotnetSlnParseException(string message) : base(message)
    {
    }

    public DotnetSlnParseException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public static DotnetSlnParseException PropertyNotFound(string property)
    {
        return new DotnetSlnParseException($"Node with name {property} was not found");
    }
}