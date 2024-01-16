namespace DotnetHelp.DevTools.Api.Handlers.Http;

internal class OutgoingHttpClient(HttpClient client, ILogger<OutgoingHttpClient> logger)
{
    public async Task<OutgoingHttpResponse> Send(
        OutgoingHttpRequest request,
        CancellationToken cancellationToken)
    {
        logger.RequestSent(request.Uri, request.Method, request.Body);

        using var httpRequest = new HttpRequestMessage();
        httpRequest.Method = HttpMethod.Parse(request.Method);
        httpRequest.RequestUri = new Uri(request.Uri);

        if (request.Headers is not null)
        {
            foreach (var header in request.Headers)
            {
                httpRequest.Headers.Add(header.Key, header.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Body))
        {
            httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
        }

        using var httpResponse = await client.SendAsync(httpRequest, cancellationToken);
        var headers = new Dictionary<string, string>();
        foreach (var header in httpResponse.Headers)
        {
            headers.TryAdd(header.Key, string.Join(",", header.Value));
        }

        var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        return new OutgoingHttpResponse(headers, (int)httpResponse.StatusCode, body);
    }
}

static partial class OutgoingHttpClientLogger
{
    [LoggerMessage(LogLevel.Debug, "Request sent to {Uri} with method {Method} and body {Body}")]
    public static partial void RequestSent(
        this ILogger<OutgoingHttpClient> logger,
        string uri,
        string method,
        string? body);
}