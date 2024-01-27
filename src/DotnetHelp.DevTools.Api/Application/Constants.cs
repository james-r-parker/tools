using System.Collections.Immutable;

namespace DotnetHelp.DevTools.Api.Application;

public static class Constants
{
    public const int TTLDays = 7;

    public static string BinTableName =>
        Environment.GetEnvironmentVariable("DB_TABLE_NAME")
        ?? throw new Exception("DB_TABLE_NAME environment variable not set");

    public static ProductInfoHeaderValue UserAgent =>
        new("DotnetHelp.DevTools.Api", "1.0.0");

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