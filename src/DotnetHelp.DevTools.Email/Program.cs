namespace DotnetHelp.DevTools.Email;

public class Function
{
    private static async Task Main(string[] args)
    {
        Func<S3Event, ILambdaContext, Task> handler = FunctionHandler;
        await LambdaBootstrapBuilder
            .Create(handler, new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    public static async Task FunctionHandler(S3Event input,
        ILambdaContext context)
    {
        foreach (var record in input.Records)
        {
            await ProcessFile(record, context);
        }
    }

    private static Task ProcessFile(S3Event.S3EventNotificationRecord record, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Processing file {record.S3.Object.Key}");
        }
        catch (Exception e)
        {
            context.Logger.LogError(e.Message);
        }

        return Task.CompletedTask;
    }
}

[JsonSerializable(typeof(S3Event))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}