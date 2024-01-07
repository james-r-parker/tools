namespace DotnetHelp.DevTools.Websocket;

public class Function
{
    private static async Task Main(string[] args)
    {
        Func<APIGatewayHttpApiV2ProxyRequest, ILambdaContext, Task> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    public static Task FunctionHandler(APIGatewayHttpApiV2ProxyRequest input, ILambdaContext context)
    {
        context.Logger.LogInformation(input.RouteKey);
        return Task.CompletedTask;
    }
}

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}