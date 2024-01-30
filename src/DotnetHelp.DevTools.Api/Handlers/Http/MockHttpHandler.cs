using System.Text.Json.Nodes;
using DotnetHelp.DevTools.Api.Application.Repositories;
using DotnetHelp.DevTools.WebsocketClient;

namespace DotnetHelp.DevTools.Api.Handlers.Http;

internal static class MockHttpHandler
{
    internal static async Task<IResult> Create(
        [FromBody] NewHttpMock request,
        [FromServices] IHttpMockRepository db,
        CancellationToken cancellationToken)
    {
        await db.Create(request, cancellationToken);
        return Results.Created();
    }

    internal static async Task<IResult> Update(
        [FromBody] HttpMock request,
        [FromServices] IHttpMockRepository db,
        CancellationToken cancellationToken)
    {
        await db.Update(request, cancellationToken);
        return Results.Accepted();
    }

    internal static async Task<IResult> Delete(
        [FromRoute] string bucket,
        [FromRoute] long created,
        [FromServices] IHttpMockRepository db,
        CancellationToken cancellationToken)
    {
        await db.Delete(bucket, created, cancellationToken);
        return Results.Accepted();
    }

    internal static async Task<IResult> List(
        [FromRoute] string bucket,
        [FromQuery] long created,
        [FromServices] IHttpMockRepository db,
        CancellationToken cancellationToken)
    {
        var items = await db.List(bucket, created, cancellationToken);
        return Results.Ok(items);
    }

    internal static async Task<IResult> Execute(
        [FromRoute] string bucket,
        [FromRoute] string slug,
        [FromServices] IHttpMockRepository db,
        [FromServices] IWebsocketClient wss,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        HttpMock? item = await db.Get(bucket, slug, cancellationToken);

        if (item is null)
        {
            return Results.BadRequest();
        }

        foreach (KeyValuePair<string, string> header in item.Headers)
        {
            http.Response.Headers.Append(header.Key, header.Value);
        }

        return Results.Ok(JsonNode.Parse(item.Body));
    }
}