using System.Diagnostics;

namespace DotnetHelp.DevTools.Websocket;

public class Function
{
    private static async Task Main(string[] args)
    {
        Func<APIGatewayProxyRequest, ILambdaContext, Task<APIGatewayProxyResponse>> handler = FunctionHandler;
        await LambdaBootstrapBuilder
            .Create(handler, new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>())
            .Build()
            .RunAsync();
    }
    
    public static async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input,
        ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Connection: {input.RequestContext.ConnectionId}");
            context.Logger.LogInformation($"Route: {input.RequestContext.RouteKey}");
            
            Task processor = input.RequestContext.RouteKey switch
            {
                "$connect" => ConnectHandler.HandleAsync(input, context),
                "$disconnect" => DisconnectHandler.HandleAsync(input, context),
                _ => DefaultHandler.HandleAsync(input, context)
            };

            await processor;

            return new APIGatewayProxyResponse
            {
                Body = "OK",
                StatusCode = 200
            };
        }
        catch (Exception e)
        {
            context.Logger.LogError(e.Message);

            return new APIGatewayProxyResponse
            {
                Body = e.Message,
                StatusCode = 500,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}

[JsonSerializable(typeof(APIGatewayProxyRequest))]
[JsonSerializable(typeof(APIGatewayProxyResponse))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}