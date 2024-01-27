namespace DotnetHelp.DevTools.Api.Handlers.Hash;

internal static class HashHandler
{
    internal static IResult Hash(
        [FromBody] HashApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return Results.BadRequest();
        }

        var encoding = GetEncoding(request);
        var hashAlgorithm = GetHashAlgorithm(request, encoding);
        var hash = hashAlgorithm.ComputeHash(encoding.GetBytes(request.Message));
        return Results.Ok(new TextApiResponse(BuildResponse(request, hash)));
    }

    private static Encoding GetEncoding(
        HashApiRequest request)
    {
        return request.Encoding switch
        {
            "UNICODE" => new UnicodeEncoding(),
            "UTF8" => new UTF8Encoding(),
            "UTF32" => new UTF32Encoding(),
            "ASCII" => new ASCIIEncoding(),
            _ => new UTF8Encoding()
        };
    }

    private static HashAlgorithm GetHashAlgorithm(
        HashApiRequest request,
        Encoding encoding)
    {
        return request.Algorithm switch
        {
            "MD5" => MD5.Create(),
            "SHA1" => SHA1.Create(),
            "SHA256" => SHA256.Create(),
            "SHA384" => SHA384.Create(),
            "SHA512" => SHA512.Create(),
            "HMACSHA256" => request.Secret is not null
                ? new HMACSHA256(encoding.GetBytes(request.Secret))
                : new HMACSHA256(),
            "HMACSHA512" => request.Secret is not null
                ? new HMACSHA512(encoding.GetBytes(request.Secret))
                : new HMACSHA512(),
            _ => SHA256.Create()
        };
    }

    private static string BuildResponse(HashApiRequest request, byte[] hash)
    {
        return request.Format switch
        {
            "BASE64" => Convert.ToBase64String(hash),
            "HEX" => BitConverter.ToString(hash).Replace("-", ""),
            "UTF8" => Encoding.UTF8.GetString(hash),
            _ => Convert.ToBase64String(hash)
        };
    }
}