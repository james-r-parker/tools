using Amazon.S3;

namespace DotnetHelp.DevTools.Email;

public class Function
{
    private static readonly AmazonS3Client S3 = new();

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

    private static async Task ProcessFile(S3Event.S3EventNotificationRecord record, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Processing file {record.S3.Object.Key}");
            using var file = await S3.GetObjectAsync(record.S3.Bucket.Name, record.S3.Object.Key);
            using var email = await MimeKit.MimeMessage.LoadAsync(file.ResponseStream);
            
            context.Logger.LogInformation($"From {email.From}");
            context.Logger.LogInformation($"To {email.To}");
            context.Logger.LogInformation($"Subject {email.Subject}");
            context.Logger.LogInformation($"Subject {email.Body}");
        }
        catch (Exception e)
        {
            context.Logger.LogError(e.Message);
        }
    }
}

[JsonSerializable(typeof(S3Event))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}