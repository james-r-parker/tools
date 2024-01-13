using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace DotnetHelp.DevTools.Api.Handlers.Jwt;

internal static class JwtHandler
{
    internal static async Task<IResult> Decode(
        [FromServices] JwtKeyHttpClient httpClient,
        [FromBody] JwtApiRequest request)
    {
        SecurityKey key = await GetSecurityKey(httpClient, request);

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
        JwtKeyHttpClient httpClient,
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

        var key = await httpClient.GetRsaSecurityKey(
            jwtSecurityToken,
            request.JwksUrl ?? $"{jwtSecurityToken.Issuer}/.well-known/openid-configuration/jwks");

        if (key is null)
        {
            throw new ApplicationException("Unable to get security key");
        }

        return key;
    }
}