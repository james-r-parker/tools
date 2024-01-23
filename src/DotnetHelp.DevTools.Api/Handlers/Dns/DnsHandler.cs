using DnsClient;
using DnsClient.Protocol;

namespace DotnetHelp.DevTools.Api.Handlers.Dns;

public static class DnsHandler
{
    private static readonly IReadOnlyCollection<QueryType> SupportedTypes = new[]
    {
        QueryType.A,
        QueryType.AAAA,
        QueryType.CNAME,
        QueryType.MX,
        QueryType.NS,
        QueryType.PTR,
        QueryType.SOA,
        QueryType.SRV,
        QueryType.TXT
    };

    internal static async Task<IResult> Lookup(
        [FromRoute] string domain,
        [FromServices] ILookupClient dnsClient,
        [FromServices] IDistributedCache cache,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return Results.BadRequest();
        }

        var cacheKey = $"dns:{domain}";

        DnsLookupResponses? cached = await cache.GetAsync(
            cacheKey,
            ApiJsonContext.Default.DnsLookupResponses,
            cancellationToken);

        if (cached is not null)
        {
            return Results.Ok(cached);
        }

        var responses = new System.Collections.Concurrent.ConcurrentBag<DnsLookupResponse>();

        foreach (var supportedType in SupportedTypes)
        {
            await dnsClient.QueryAsync(domain, supportedType, QueryClass.IN, cancellationToken)
                .ContinueWith((result) =>
                {
                    foreach (DnsResourceRecord record in result.Result.Answers)
                    {
                        if (record is ARecord aRecord)
                        {
                            responses.Add(new(aRecord.RecordType.ToString(),
                                aRecord.Address.ToString(), aRecord.TimeToLive));
                        }
                        else if (record is AaaaRecord aaaaRecord)
                        {
                            responses.Add(new(aaaaRecord.RecordType.ToString(),
                                aaaaRecord.Address.ToString(), aaaaRecord.TimeToLive));
                        }
                        else if (record is CNameRecord cnameRecord)
                        {
                            responses.Add(new(cnameRecord.RecordType.ToString(),
                                cnameRecord.CanonicalName.ToString(), cnameRecord.TimeToLive));
                        }
                        else if (record is MxRecord mxRecord)
                        {
                            responses.Add(new(mxRecord.RecordType.ToString(),
                                mxRecord.Exchange.ToString(), mxRecord.TimeToLive));
                        }
                        else if (record is NsRecord nsRecord)
                        {
                            responses.Add(new(nsRecord.RecordType.ToString(),
                                nsRecord.NSDName.ToString(), nsRecord.TimeToLive));
                        }
                        else if (record is PtrRecord ptrRecord)
                        {
                            responses.Add(new(ptrRecord.RecordType.ToString(),
                                ptrRecord.PtrDomainName.ToString(), ptrRecord.TimeToLive));
                        }
                        else if (record is SoaRecord soaRecord)
                        {
                            responses.Add(new(soaRecord.RecordType.ToString(),
                                soaRecord.ToString(), soaRecord.TimeToLive));
                        }
                        else if (record is SrvRecord srvRecord)
                        {
                            responses.Add(new(srvRecord.RecordType.ToString(),
                                srvRecord.Target.ToString(), srvRecord.TimeToLive));
                        }
                        else if (record is TxtRecord txtRecord)
                        {
                            foreach (var txt in txtRecord.Text)
                            {
                                responses.Add(new(txtRecord.RecordType.ToString(),
                                    txt, txtRecord.TimeToLive));
                            }
                        }
                    }
                }, cancellationToken);
        }

        var model = new DnsLookupResponses(domain, responses);
        
        await cache.SetAsync(
            cacheKey, 
            model,
            ApiJsonContext.Default.DnsLookupResponses, 
            TimeSpan.FromMinutes(1),
            cancellationToken);
        
        return Results.Ok(model);
    }
}