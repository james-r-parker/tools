﻿using DotnetHelp.DevTools.Api.Application.Repositories;

namespace DotnetHelp.DevTools.Api.Handlers.Email;

internal static class EmailRequestHandler
{
    internal static async Task<IResult> List(
        [FromRoute] string bucket,
        [FromQuery] long from,
        [FromServices] IEmailRepository db,
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

        IReadOnlyCollection<IncomingEmail> emails =
            await db.List(bucket, from, cancellationToken);

        return Results.Ok(emails);
    }
    
    internal static async Task<IResult> Delete(
        [FromRoute] string bucket,
        [FromRoute] long created,
        [FromServices] IEmailRepository db,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(bucket))
        {
            return Results.BadRequest();
        }

        if (created < 0)
        {
            return Results.BadRequest();
        }

        await db.Delete(bucket, created, cancellationToken);

        return Results.NoContent();
    }
}