using System.Collections.Immutable;

namespace DotnetHelp.DevTools.Api.Application;

public static class Constants
{
    public static string ConnectionTableName =>
        Environment.GetEnvironmentVariable("CONNECTION_TABLE_NAME")
        ?? throw new Exception("CONNECTION_TABLE_NAME environment variable not set");

    public static string HttpRequestTableName =>
        Environment.GetEnvironmentVariable("HTTP_REQUEST_TABLE_NAME")
        ?? throw new Exception("CONNECTION_TABLE_NAME environment variable not set");

    public static string CacheTableName =>
        Environment.GetEnvironmentVariable("CACHE_TABLE_NAME")
        ?? throw new Exception("CACHE_TABLE_NAME environment variable not set");

    public static string WebsocketUrl =>
        Environment.GetEnvironmentVariable("WEBSOCKET_URL")
        ?? throw new Exception("WEBSOCKET_URL environment variable not set");

    public static ProductInfoHeaderValue UserAgent =>
        new ("DotnetHelp.DevTools.Api", "1.0.0");
    
    public static readonly ImmutableHashSet<string> IgnoreHeaders =
        ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
            new[]
            {
                "x-amzn-tls-version",
                "x-forwarded-proto",
                "x-forwarded-port",
                "x-amzn-tls-cipher-suite",
                "x-amzn-trace-id",
                "x-amz-cf-id",
                "via",
                "host"
            });
}