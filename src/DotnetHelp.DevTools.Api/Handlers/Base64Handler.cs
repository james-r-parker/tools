namespace DotnetHelp.DevTools.Api.Handlers;

public static class Base64Handler
{
    internal static IResult Encode([FromBody]TextApiRequest request) =>
        Results.Ok(new TextApiResponse(Convert.ToBase64String(Encoding.UTF8.GetBytes(request.Request))));
}