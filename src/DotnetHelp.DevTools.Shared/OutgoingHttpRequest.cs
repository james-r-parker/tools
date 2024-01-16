namespace DotnetHelp.DevTools.Shared;

public record OutgoingHttpRequest(
    string Uri,
    string Method,
    IReadOnlyDictionary<string, string>? Headers,
    string? Body);

public record OutgoingHttpResponse(
    IReadOnlyDictionary<string, string> Headers,
    int StatusCode,
    string Body);