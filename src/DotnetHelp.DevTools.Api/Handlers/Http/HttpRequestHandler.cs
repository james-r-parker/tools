using DotnetHelp.DevTools.WebsocketClient;

namespace DotnetHelp.DevTools.Api.Handlers.Http;

internal static class HttpRequestHandler
{
    internal static async Task<IResult> New(
        [FromRoute] string bucket,
        [FromServices] IHttpRequestRepository db,
        [FromServices] IWebsocketClient wss,
        [FromServices] IHttpContextAccessor http,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(bucket))
        {
            return Results.BadRequest();
        }

        if (http?.HttpContext is null)
        {
            return Results.BadRequest();
        }

        var headers = new Dictionary<string, string>();
        foreach (var header in http.HttpContext.Request.Headers)
        {
            if (Constants.IgnoreHeaders.Contains(header.Key))
            {
                continue;
            }

            headers.TryAdd(header.Key, header.Value.ToString());
        }

        var query = new Dictionary<string, string>();
        if (http.HttpContext.Request.Query.Count > 0)
        {
            foreach (var q in http.HttpContext.Request.Query)
            {
                query.TryAdd(q.Key, q.Value.ToString());
            }
        }

        string body;

        using (var reader
               = new StreamReader(
                   http.HttpContext.Request.Body,
                   Encoding.UTF8,
                   true,
                   1024,
                   true))
        {
            body = await reader.ReadToEndAsync(cancellationToken);
        }

        await db.Save(new BucketHttpRequest(
                bucket,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddDays(1),
                http.HttpContext.Connection.RemoteIpAddress?.ToString(),
                headers,
                query,
                body),
            cancellationToken);

        await wss.SendMessage(
            new WebSocketMessage("HTTP_REQUEST", bucket),
            cancellationToken);

        return Results.Ok();
    }

    internal static async Task<IResult> List(
        [FromRoute] string bucket,
        [FromQuery] long from,
        [FromServices] IHttpRequestRepository db,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(bucket))
        {
            return Results.BadRequest();
        }

        if (from < 0)
        {
            return Results.BadRequest();
        }

        IReadOnlyCollection<BucketHttpRequest> requests =
            await db.List(bucket, from, cancellationToken);

        return Results.Ok(requests);
    }

    internal static async Task<IResult> Send(
        [FromBody] OutgoingHttpRequest request,
        [FromServices] OutgoingHttpClient client)
    {
        var result = await client.Send(request, CancellationToken.None);
        return Results.Ok(result);
    }
}