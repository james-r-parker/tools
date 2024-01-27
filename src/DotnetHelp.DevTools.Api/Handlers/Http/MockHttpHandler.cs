using DotnetHelp.DevTools.Api.Application.Repositories;

namespace DotnetHelp.DevTools.Api.Handlers.Http;

internal static class MockHttpHandler
{
    internal static async Task<IResult> Create(
        [FromBody] NewHttpMock request,
        [FromServices] IHttpMockRepository db,
        CancellationToken cancellationToken)
    {
        try
        {
            await db.Create(request, cancellationToken);
            return Results.Created();
        }
        catch
        {
            return Results.BadRequest();
        }
    }

    internal static async Task<IResult> Update(
        [FromBody] HttpMock request,
        [FromServices] IHttpMockRepository db,
        CancellationToken cancellationToken)
    {
        try
        {
            await db.Update(request, cancellationToken);
            return Results.Accepted();
        }
        catch
        {
            return Results.BadRequest();
        }
    }

    internal static async Task<IResult> Delete(
        [FromRoute] string bucket,
        [FromRoute] long created,
        [FromServices] IHttpMockRepository db,
        CancellationToken cancellationToken)
    {
        try
        {
            await db.Delete(bucket, created, cancellationToken);
            return Results.Accepted();
        }
        catch
        {
            return Results.BadRequest();
        }
    }
    
    internal static async Task<IResult> List(
        [FromRoute] string bucket,
        [FromQuery] long created,
        [FromServices] IHttpMockRepository db,
        CancellationToken cancellationToken)
    {
        try
        {
            var items = await db.List(bucket, created, cancellationToken);
            return Results.Ok(items);
        }
        catch
        {
            return Results.BadRequest();
        }
    }
    
    internal static async Task<IResult> Execute(
        [FromRoute] string bucket,
        [FromRoute] string slug,
        [FromServices] IHttpMockRepository db,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok();
        }
        catch
        {
            return Results.BadRequest();
        }
    }
}