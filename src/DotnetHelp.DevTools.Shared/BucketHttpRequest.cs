namespace DotnetHelp.DevTools.Shared;

public record BucketHttpRequest(
    string Bucket,
    DateTimeOffset Created,
    DateTimeOffset Ttl,
    string? IpAddress,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyDictionary<string, string> Query,
    string Body);