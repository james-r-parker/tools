using HttpRequest = DotnetHelp.DevTools.Api.Application.HttpRequest;

namespace DotnetHelp.DevTools.Api.Handlers.Http;

internal static class HttpRequestHandler
{
    internal static async Task<IResult> New(
        [FromRoute] string bucket,
        [FromServices] IHttpRequestRepository db,
        [FromServices] IWebsocketClient wss,
        [FromServices] IHttpContextAccessor http)
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
            headers.TryAdd(header.Key, header.Value.ToString());
        }

        var query = new Dictionary<string, string>();
        if (http.HttpContext.Request.Query.Any())
        {
            foreach (var q in http.HttpContext.Request.Query)
            {
                query.TryAdd(q.Key, q.Value.ToString());
            }
        }

        string body;

        using (StreamReader reader
               = new StreamReader(http.HttpContext.Request.Body, Encoding.UTF8, true, 1024, true))
        {
            body = await reader.ReadToEndAsync();
        }

        await db.Save(new HttpRequest(
            bucket,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(1),
            headers,
            query,
            body));

        await wss.SendMessage(new WebSocketMessage("HTTP_REQUEST", bucket));

        return Results.Ok();
    }

    internal static async Task<IResult> List(
        [FromRoute] string bucket,
        [FromQuery] int from,
        [FromServices] IHttpRequestRepository db)
    {
        if (string.IsNullOrWhiteSpace(bucket))
        {
            return Results.BadRequest();
        }

        if (from <= 0)
        {
            return Results.BadRequest();
        }

        IReadOnlyCollection<HttpRequest> requests = await db.List(bucket, from);

        return Results.Ok(requests);
    }
}