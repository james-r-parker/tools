using System.Collections.Frozen;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace DotnetHelp.DevTools.Api.Handlers;

internal static class JwtHandler
{
    private static readonly IMemoryCache Cache = new MemoryCache(new MemoryCacheOptions
    {
        ExpirationScanFrequency = TimeSpan.FromMinutes(5),
        TrackStatistics = true,
        SizeLimit = 4096000,
    });

    internal static async Task<IResult> Decode(
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromBody] JwtApiRequest request)
    {
        SecurityKey key = await GetSecurityKey(httpClientFactory, request);

        var validations = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
        };

        ClaimsPrincipal? claims =
            new JwtSecurityTokenHandler().ValidateToken(
                request.Token,
                validations,
                out SecurityToken? tokenSecure);

        var response = new JwtApiResponse
        {
            Issuer = tokenSecure.Issuer,
            ValidFrom = tokenSecure.ValidFrom,
            ValidTo = tokenSecure.ValidTo,
            Claims = claims.Claims
                .Select(x => new KeyValuePair<string, string>(x.Type, x.Value))
                .ToFrozenSet(),
        };

        return Results.Ok(response);
    }

    private static async Task<SecurityKey> GetSecurityKey(
        IHttpClientFactory httpClientFactory,
        JwtApiRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Secret))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(request.Secret);
            return new SymmetricSecurityKey(bytes);
        }

        JwtSecurityToken? jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(request.Token);

        if (jwtSecurityToken is null)
        {
            throw new ApplicationException("Invalid token");
        }

        var key = await GetRsaSecurityKey(
            httpClientFactory,
            jwtSecurityToken,
            request.JwksUrl ?? $"{jwtSecurityToken.Issuer}/.well-known/openid-configuration/jwks");

        if (key is null)
        {
            throw new ApplicationException("Unable to get security key");
        }

        return key;
    }

    private static async Task<RsaSecurityKey?> GetRsaSecurityKey(
        IHttpClientFactory factory,
        JwtSecurityToken token,
        string jwksUrl)
    {
        var cacheKey = $"{jwksUrl}:{token.Header.Kid}";
        if (Cache.TryGetValue(cacheKey, out RsaSecurityKey? cachedKey))
        {
            return cachedKey;
        }

        var httpClient = factory.CreateClient(jwksUrl);
        using var response = await httpClient.GetAsync(jwksUrl);

        if (!response.IsSuccessStatusCode)
        {
            Cache.Set<RsaSecurityKey?>(cacheKey, null, new MemoryCacheEntryOptions()
            {
                Priority = CacheItemPriority.Low,
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                Size = 1
            });
            throw new ApplicationException("Invalid HTTP response from JWKS endpoint");
        }

        Jwks? jwks =
            await response.Content.ReadFromJsonAsync(JwtHandlerJsonSerializerContext.Default.Jwks);

        if (jwks is null)
        {
            Cache.Set<RsaSecurityKey?>(cacheKey, null, new MemoryCacheEntryOptions()
            {
                Priority = CacheItemPriority.Low,
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                Size = 1
            });
            throw new ApplicationException("Invalid JSON response from JWKS endpoint");
        }

        Jwks.Key? key = jwks?.Keys?.FirstOrDefault(k => k.Kid == token.Header.Kid);

        if (key is null)
        {
            Cache.Set<RsaSecurityKey?>(cacheKey, null, new MemoryCacheEntryOptions()
            {
                Priority = CacheItemPriority.Low,
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                Size = 1
            });
            throw new ApplicationException("No key found in JWKS endpoint");
        }

        var rsa = RSA.Create();
        rsa.ImportParameters(new RSAParameters
        {
            Modulus = Base64UrlEncoder.DecodeBytes(key.N),
            Exponent = Base64UrlEncoder.DecodeBytes(key.E)
        });

        var rsaSecurityKey = new RsaSecurityKey(rsa);
        Cache.Set(cacheKey, rsaSecurityKey, new MemoryCacheEntryOptions()
        {
            Priority = CacheItemPriority.High,
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            Size = rsaSecurityKey.KeySize
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