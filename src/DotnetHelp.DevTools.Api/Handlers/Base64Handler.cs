namespace DotnetHelp.DevTools.Api.Handlers;

public static class Base64Handler
{
    internal static IResult Encode([FromBody]TextApiRequest request)
    {
        var encoding = new UTF8Encoding();
        return Results.Ok(new TextApiResponse(Convert.ToBase64String(encoding.GetBytes(request.Request))));
    }
}