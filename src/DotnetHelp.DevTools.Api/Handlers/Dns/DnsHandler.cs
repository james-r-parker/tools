using DnsClient;
using DnsClient.Protocol;

namespace DotnetHelp.DevTools.Api.Handlers.Dns;

public static class DnsHandler
{
    internal static async Task<IResult> Lookup(
        [FromRoute] string type,
        [FromRoute] string domain,
        [FromServices] ILookupClient dnsClient,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(type, out QueryType queryType))
        {
            return Results.BadRequest();
        }

        if (string.IsNullOrWhiteSpace(domain))
        {
            return Results.BadRequest();
        }

        IDnsQueryResponse? result =
            await dnsClient.QueryAsync(domain, queryType, QueryClass.IN, cancellationToken);

        List<DnsLookupResponse> responses = new();
        foreach (DnsResourceRecord record in result.Answers)
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

        var model = new DnsLookupResponses(domain, responses);
        return Results.Ok(model);
    }
}