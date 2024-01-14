namespace DotnetHelp.DevTools.Shared;

public record JwtApiRequest(string Token, string? Secret, string? JwksUrl);

public record JwtApiResponse()
{
    public required string Issuer { get; init; }
    public required DateTime ValidFrom { get; init; }
    public required DateTime ValidTo { get; init; }
    public required IReadOnlyCollection<KeyValuePair<string, string>> Claims { get; init; }
}