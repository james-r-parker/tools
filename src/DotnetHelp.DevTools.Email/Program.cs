using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using DotnetHelp.DevTools.Shared;
using DotnetHelp.DevTools.WebsocketClient;
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
            .AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>()
            .AddSingleton<IAmazonS3, AmazonS3Client>()
            .AddDotnetHelpWebsocketClient();

        Services = services.BuildServiceProvider();
    }

    private static readonly IServiceProvider Services;

    private static readonly string TableName = Environment.GetEnvironmentVariable("EMAIL_TABLE_NAME") ??
                                               throw new ApplicationException(
                                                   "TABLE_NAME environment variable not set");

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

        IAmazonDynamoDB db = scope.ServiceProvider.GetRequiredService<IAmazonDynamoDB>();
        IAmazonS3 s3 = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        IWebsocketClient wss = scope.ServiceProvider.GetRequiredService<IWebsocketClient>();
        ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        ILogger<Function> logger = loggerFactory.CreateLogger<Function>();

        foreach (var record in input.Records)
        {
            try
            {
                logger.ProcessingFile(record.S3.Object.Key);
                
                using var file =
                    await s3.GetObjectAsync(record.S3.Bucket.Name, record.S3.Object.Key, cts.Token);
                using var email =
                    await MimeKit.MimeMessage.LoadAsync(file.ResponseStream, cts.Token);

                logger.From(email.From.ToString());
                logger.Subject(email.Subject);

                string? to =
                    email.To.Mailboxes
                        .Select(x => x.Address)
                        .FirstOrDefault(x => x.EndsWith("tools.dotnethelp.co.uk"));

                if (to is null)
                {
                    continue;
                }

                logger.To(to);

                string bucket = to.Split("@")[0];

                await db.PutItemAsync(new PutItemRequest()
                {
                    TableName = TableName,
                    Item = new Dictionary<string, AttributeValue>()
                    {
                        { "bucket", new AttributeValue(bucket) },
                        { "created", new AttributeValue() { N = email.Date.ToUnixTimeSeconds().ToString() } },
                        { "s3Bucket", new AttributeValue(record.S3.Bucket.Name) },
                        { "s3Key", new AttributeValue(record.S3.Object.Key) },
                        { "messageId", new AttributeValue(email.MessageId) },
                        { "from", new AttributeValue(email.From.ToString()) },
                        { "to", new AttributeValue(email.To.ToString()) },
                        { "subject", new AttributeValue(email.Subject) },
                        {
                            "ttl",
                            new AttributeValue() { N = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds().ToString() }
                        },
                    }
                }, cts.Token);

                await wss.SendMessage(new WebSocketMessage("INCOMING_EMAIL", bucket), cts.Token);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }
    }
}

internal static partial class FunctionLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Processing file {Key}")]
    public static partial void ProcessingFile(this ILogger<Function> logger, string key);

    [LoggerMessage(Level = LogLevel.Information, Message = "From {From}")]
    public static partial void From(this ILogger<Function> logger, string from);

    [LoggerMessage(Level = LogLevel.Information, Message = "To {To}")]
    public static partial void To(this ILogger<Function> logger, string to);

    [LoggerMessage(Level = LogLevel.Information, Message = "Subject {Subject}")]
    public static partial void Subject(this ILogger<Function> logger, string subject);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to process file")]
    public static partial void Error(this ILogger<Function> logger, Exception ex);
}

[JsonSerializable(typeof(S3Event))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}