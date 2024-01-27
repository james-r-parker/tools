namespace DotnetHelp.DevTools.Shared;

public record NewHttpMock(
    string Bucket,
    string Slug,
    string Method,
    IReadOnlyCollection<KeyValuePair<string,string>> Headers,
    string Body);

public record HttpMock(
    string Bucket,
    string Slug,
    string Method,
    IReadOnlyCollection<KeyValuePair<string,string>> Headers,
    string Body,
    DateTimeOffset Ttl,
    DateTimeOffset Created);