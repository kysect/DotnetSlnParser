using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Kysect.DotnetSlnParser.Tests;

public static class TestLogger
{
    public static ILogger Create()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .CreateLogger();

        using ILoggerFactory loggerFactory = LoggerFactory.Create(b => b.AddSerilog());

        return loggerFactory.CreateLogger("TestLogger");
    }
}