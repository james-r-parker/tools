using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace DotnetHelp.DevTools.Api.Handlers.Jwt;

internal abstract class JwtKeyHttpClient(HttpClient httpClient, IDistributedCache cache)
{
    public async Task<RsaSecurityKey?> GetRsaSecurityKey(
        JwtSecurityToken token,
        string jwksUrl,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{jwksUrl}:{token.Header.Kid}";
        byte[]? cached = await cache.GetAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return ToRsaKey(Encoding.UTF8.GetString(cached), token.Header.Kid);
        }

        using var response = await httpClient.GetAsync(jwksUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ApplicationException("Invalid HTTP response from JWKS endpoint");
        }

        string json = await response.Content.ReadAsStringAsync(cancellationToken);

        await cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(json), new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        }, cancellationToken);

        return ToRsaKey(json, token.Header.Kid);
    }

    private static RsaSecurityKey? ToRsaKey(string json, string kid)
    {
        Jwks? jwks = JsonSerializer.Deserialize(json, JwtHandlerJsonSerializerContext.Default.Jwks);
        Jwks.Key? key = jwks?.Keys?.FirstOrDefault(k => k.Kid == kid);

        if (key is null)
        {
            return null;
        }

        var rsa = RSA.Create();
        rsa.ImportParameters(new RSAParameters
        {
            Modulus = Base64UrlEncoder.DecodeBytes(key.N),
            Exponent = Base64UrlEncoder.DecodeBytes(key.E)
        });

        return new RsaSecurityKey(rsa);
    }
}

internal record Jwks
{
    [JsonPropertyName("keys")] public List<Key>? Keys { get; set; }

    internal record Key
    {
        [JsonPropertyName("kid")] public string? Kid { get; set; }

        [JsonPropertyName("n")] public string? N { get; set; }

        [JsonPropertyName("e")] public string? E { get; set; }
    }
}

[JsonSerializable(typeof(Jwks))]
internal partial class JwtHandlerJsonSerializerContext : JsonSerializerContext
{
}