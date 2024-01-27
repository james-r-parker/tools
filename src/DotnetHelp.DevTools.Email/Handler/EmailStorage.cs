using System.Diagnostics.CodeAnalysis;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using DotnetHelp.DevTools.Email.Application;
using DotnetHelp.DevTools.Shared;
using DotnetHelp.DevTools.WebsocketClient;
using Microsoft.Extensions.Logging;
using MimeKit;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DotnetHelp.DevTools.Email.Handler;

internal sealed class EmailStorage(IAmazonDynamoDB db, IAmazonS3 s3, IWebsocketClient wss, ILogger<EmailStorage> logger)
    : IEmailEventHandler
{
    public async ValueTask ProcessEmailAsync(string s3Bucket, string s3Key, CancellationToken cancellationToken)
    {
        logger.ProcessingFile(s3Key);

        using var file =
            await s3.GetObjectAsync(s3Bucket, s3Key, cancellationToken);

        using var email =
            await MimeMessage.LoadAsync(file.ResponseStream, cancellationToken);

        if (!TryGetBucket(email.To.Mailboxes, out string? bucketName))
        {
            logger.NoBucket();
            return;
        }

        logger.NewEmail(bucketName,email.From.ToString(), email.Subject);
        
        await db.PutItemAsync(new PutItemRequest()
        {
            TableName = Constants.TableName,
            Item = new Dictionary<string, AttributeValue>()
            {
                { "bucket", new AttributeValue($"incoming_email-{bucketName}") },
                { "created", new AttributeValue() { N = email.Date.ToUnixTimeSeconds().ToString() } },
                { "s3Bucket", new AttributeValue(s3Bucket) },
                { "s3Key", new AttributeValue(s3Key) },
                { "messageId", new AttributeValue(email.MessageId) },
                { "from", ParseAddresses(email.From.Mailboxes) },
                { "to", ParseAddresses(email.To.Mailboxes) },
                { "headers", ParseHeaders(email.Headers) },
                { "content", ParseContent(email.BodyParts) },
                { "subject", new AttributeValue(email.Subject) },
                {
                    "ttl",
                    new AttributeValue() { N = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds().ToString() }
                },
            }
        }, cancellationToken);

        await wss.SendMessage(new WebSocketMessage("INCOMING_EMAIL", bucketName), cancellationToken);
    }

    private static bool TryGetBucket(IEnumerable<MailboxAddress> to, [NotNullWhen(true)] out string? bucket)
    {
        string? match =
            to.Select(x => x.Address)
                .FirstOrDefault(x => x.EndsWith("tools.dotnethelp.co.uk"));

        if (match is null)
        {
            bucket = null;
            return false;
        }

        bucket = match.Split("@")[0];
        return true;
    }

    private static AttributeValue ParseAddresses(IEnumerable<MailboxAddress> addresses)
    {
        var output = new AttributeValue() { L = [] };
        foreach (var mailbox in addresses)
        {
            output.L.Add(new AttributeValue()
            {
                M = new Dictionary<string, AttributeValue>()
                {
                    {
                        "name",
                        mailbox.Name is null
                            ? new AttributeValue() { NULL = true }
                            : new AttributeValue(mailbox.Name)
                    },
                    {
                        "address",
                        mailbox.Address is null
                            ? new AttributeValue() { NULL = true }
                            : new AttributeValue(mailbox.Address)
                    },
                    {
                        "domain",
                        mailbox.Domain is null
                            ? new AttributeValue() { NULL = true }
                            : new AttributeValue(mailbox.Domain)
                    }
                }
            });
        }

        return output;
    }

    private static AttributeValue ParseHeaders(HeaderList headers)
    {
        var output = new AttributeValue() { L = [] };
        foreach (var header in headers)
        {
            if (header.Field is not null && header.Value is not null)
            {
                output.L.Add(new AttributeValue()
                {
                    M = new Dictionary<string, AttributeValue>()
                    {
                        { "name", new AttributeValue(header.Field) },
                        { "value", new AttributeValue(header.Value) }
                    }
                });
            }
        }

        return output;
    }
    
    private static AttributeValue ParseContent(IEnumerable<MimeEntity> parts)
    {
        var output = new AttributeValue() { L = [] };
        foreach (var part in parts)
        {
            if (part is TextPart { Text: not null } textPart)
            {
                output.L.Add(new AttributeValue()
                {
                    M = new Dictionary<string, AttributeValue>()
                    {
                        { "contentType", new AttributeValue(part.ContentType?.MimeType ?? "text/plain") },
                        { "contentId", new AttributeValue(part.ContentId ?? Guid.NewGuid().ToString("N")) },
                        { "content", new AttributeValue(textPart.Text) },
                    }
                });
            }
        }

        return output;
    }
}

internal static partial class FunctionLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Processing file {Key}")]
    public static partial void ProcessingFile(this ILogger<EmailStorage> logger, string key);
    
    [LoggerMessage(Level = LogLevel.Information, Message = "New Email To:{To} From:{From} Subject:{Subject}")]
    public static partial void NewEmail(this ILogger<EmailStorage> logger, string to, string from, string subject);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Unable to find bucket")]
    public static partial void NoBucket(this ILogger<EmailStorage> logger);
}