namespace DotnetHelp.DevTools.Websocket;

public class Function
{
    private static async Task Main(string[] args)
    {
        Func<string, ILambdaContext, string> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    public static string FunctionHandler(string input, ILambdaContext context)
    {
        return input.ToUpper();
    }
}

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}