internal static class HmacHandler
{
    internal static IResult GetHmac(
        [FromQuery]string key,
        [FromQuery]string message)
    {
        var encoding = new UTF8Encoding();
        var keyBytes = encoding.GetBytes(key);
        var messageBytes = encoding.GetBytes(message);
        using var hash256 = new HMACSHA256(keyBytes);
        var hashMessage = hash256.ComputeHash(messageBytes);
        return Results.Ok(new TextApiResponse(Convert.ToBase64String(hashMessage)));
    }
}