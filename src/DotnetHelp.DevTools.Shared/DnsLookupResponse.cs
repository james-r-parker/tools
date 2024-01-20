namespace DotnetHelp.DevTools.Shared;

public record DnsLookupResponses(string Domain, IReadOnlyCollection<DnsLookupResponse> Responses);

public record DnsLookupResponse(string Type, string Address, long Ttl);