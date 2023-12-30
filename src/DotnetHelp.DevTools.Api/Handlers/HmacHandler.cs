namespace DotnetHelp.DevTools.Api.Handlers;

internal static class HmacHandler
{
    internal static IResult Hash(
        [FromBody]HmacApiRequest request)
    {
        var encoding = new UTF8Encoding();
        var keyBytes = encoding.GetBytes(request.Key);
        var messageBytes = encoding.GetBytes(request.Message);
        using var hash256 = new HMACSHA256(keyBytes);
        var hashMessage = hash256.ComputeHash(messageBytes);
        return Results.Ok(new TextApiResponse(Convert.ToBase64String(hashMessage)));
    }
}