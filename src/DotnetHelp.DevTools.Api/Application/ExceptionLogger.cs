using Microsoft.AspNetCore.Diagnostics;

namespace DotnetHelp.DevTools.Api.Application;

public class ExceptionLogger(ILogger<ExceptionLogger> logger) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        ExceptionLogging.UnhandledException(logger, exception);
        httpContext.Response.StatusCode = 500;
        return new ValueTask<bool>(true);
    }
}

internal static partial class ExceptionLogging
{
    [LoggerMessage(LogLevel.Error, "An unhandled exception has occurred: {Exception}")]
    public static partial void UnhandledException(ILogger<ExceptionLogger> logger, Exception exception);
}