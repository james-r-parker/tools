using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace DotnetHelp.DevTools.Api.Handlers.Http;

internal class OutgoingHttpClient(HttpClient client, IDistributedCache cache, ILogger<OutgoingHttpClient> logger)
{
    private static readonly IReadOnlySet<string> UntrusedHosts =
        ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, new[]
        {
            "localhost",
        });

    public async Task<OutgoingHttpResponse> Send(
        OutgoingHttpRequest request,
        CancellationToken cancellationToken)
    {
        string cacheKey = $"http:{request.Method}:{request.Uri}:{request.Body}";

        OutgoingHttpResponse? cached =
            await cache.GetAsync(
                cacheKey,
                ApiJsonContext.Default.OutgoingHttpResponse,
                cancellationToken);

        if (cached is not null)
        {
            return cached;
        }

        OutgoingHttpResponse response = await SendInternal(request, cancellationToken);

        await cache.SetAsync(cacheKey,
            response,
            ApiJsonContext.Default.OutgoingHttpResponse,
            TimeSpan.FromMinutes(5),
            cancellationToken);

        return response;
    }

    private async Task<OutgoingHttpResponse> SendInternal(
        OutgoingHttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseUri(request.Uri, out Uri? uri))
        {
            logger.InvalidUrl(request.Uri);
            throw new ArgumentException("Uri is not valid", nameof(request.Uri));
        }

        logger.RequestSent(request.Uri, request.Method, request.Body);

        using var httpRequest = new HttpRequestMessage();
        httpRequest.Method = HttpMethod.Parse(request.Method);
        httpRequest.RequestUri = uri;

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

    private static bool TryParseUri(string input, [NotNullWhen(true)] out Uri? uri)
    {
        if (Uri.TryCreate(input, UriKind.Absolute, out Uri? parsedUri))
        {
            if (UntrusedHosts.Contains(parsedUri.Host))
            {
                uri = null;
                return false;
            }

            if (IPAddress.TryParse(parsedUri.Host, out _))
            {
                uri = null;
                return false;
            }

            uri = parsedUri;
            return true;
        }

        uri = null;
        return false;
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

    [LoggerMessage(LogLevel.Warning, "Invalid Uri {Uri}")]
    public static partial void InvalidUrl(
        this ILogger<OutgoingHttpClient> logger,
        string uri);
}