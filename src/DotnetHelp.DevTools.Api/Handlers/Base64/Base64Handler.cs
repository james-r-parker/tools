namespace DotnetHelp.DevTools.Api.Handlers.Base64;

public static class Base64Handler
{
    internal static IResult Encode([FromBody]TextApiRequest request)
    {
        return Results.Ok(new TextApiResponse(Convert.ToBase64String(Encoding.UTF8.GetBytes(request.Request))));
    }
    
    internal static IResult Decode([FromBody]TextApiRequest request)
    {
        return Results.Ok(new TextApiResponse(Encoding.UTF8.GetString(Convert.FromBase64String(request.Request))));
    }
}