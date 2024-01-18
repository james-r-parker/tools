using System.Collections.Immutable;

namespace DotnetHelp.DevTools.Api.Handlers.Dns;

public static class DnsHandler
{
    internal static async Task<IResult> Lookup(
        [FromBody] TextApiRequest request,
        [FromServices] IDistributedCache cache,
        CancellationToken cancellationToken)
    {
        try
        {
            string key = $"dns:{request.Request}";
            TextCollectionApiResponse? cached = await cache.GetAsync(key,
                ApiJsonContext.Default.TextCollectionApiResponse,
                cancellationToken);

            if (cached is not null)
            {
                return Results.Ok(cached);
            }

            IPAddress[] ips = await System.Net.Dns.GetHostAddressesAsync(request.Request, cancellationToken);

            IReadOnlyCollection<TextApiResponse> results = ips
                .Select(x => new TextApiResponse(x.ToString()))
                .ToImmutableArray();

            var model = new TextCollectionApiResponse(results);

            await cache.SetAsync(
                key,
                model,
                ApiJsonContext.Default.TextCollectionApiResponse,
                TimeSpan.FromMinutes(1),
                cancellationToken);

            return Results.Ok(model);
        }
        catch
        {
            return Results.BadRequest();
        }
    }
}