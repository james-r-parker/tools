namespace DotnetHelp.DevTools.Shared;

public record NewHttpMock(
    string Bucket,
    string Slug,
    string Method,
    IReadOnlyCollection<KeyValuePair<string, string>> Headers,
    string Body);

public record HttpMock(
    string Bucket,
    string Slug,
    string Method,
    IReadOnlyCollection<KeyValuePair<string, string>> Headers,
    string Body,
    int Executions,
    DateTimeOffset Ttl,
    DateTimeOffset Created,
    DateTimeOffset Updated,
    DateTimeOffset? LastExecuted);

public record HttpMockOverview(
    string Name,
    string Method,
    int Executions,
    DateTimeOffset Created,
    DateTimeOffset? LastExecuted);