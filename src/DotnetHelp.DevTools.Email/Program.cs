using Amazon.DynamoDBv2;
using Amazon.S3;
using DotnetHelp.DevTools.Email.Handler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DotnetHelp.DevTools.Email;

public class Function
{
    static Function()
    {
        var services = new ServiceCollection();

        services
            .AddLogging(c =>
            {
                c.ClearProviders();
                c.AddJsonConsole();
            })
            .AddSingleton<IEmailEventHandler, EmailStorage>()
            .AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>()
            .AddSingleton<IAmazonS3, AmazonS3Client>()
            .AddDotnetHelpWebsocketClient();

        Services = services.BuildServiceProvider();
    }

    private static readonly IServiceProvider Services;

    private static async Task Main(string[] args)
    {
        Func<S3Event, ILambdaContext, Task> handler = FunctionHandler;
        await LambdaBootstrapBuilder
            .Create(handler, new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    private static async Task FunctionHandler(S3Event input,
        ILambdaContext context)
    {
        using CancellationTokenSource cts = new CancellationTokenSource(context.RemainingTime);
        using var scope = Services.CreateScope();

        IEnumerable<IEmailEventHandler> handlers = scope.ServiceProvider.GetServices<IEmailEventHandler>();
        ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        ILogger<Function> logger = loggerFactory.CreateLogger<Function>();

        foreach (var record in input.Records)
        {
            foreach (var handler in handlers)
            {
                try
                {
                    await handler.ProcessEmailAsync(record.S3.Bucket.Name, record.S3.Object.Key, cts.Token);
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }
            }
        }
    }
}

internal static partial class FunctionLogger
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to process file")]
    public static partial void Error(this ILogger<Function> logger, Exception ex);
}

[JsonSerializable(typeof(S3Event))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}