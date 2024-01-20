using System.Collections.Immutable;
using Amazon.S3;
using Amazon.S3.Model;

namespace DotnetHelp.DevTools.Api.Application;

internal interface IEmailRepository
{
    Task<IReadOnlyCollection<IncomingEmail>> List(
        string bucket, long from, CancellationToken cancellationToken);
}

internal class EmailRepository(IAmazonDynamoDB db, IAmazonS3 s3) : IEmailRepository
{
    public async Task<IReadOnlyCollection<IncomingEmail>> List(
        string bucket, long from, CancellationToken cancellationToken)
    {
        QueryResponse response = await db.QueryAsync(new QueryRequest
            {
                TableName = Constants.EmailTableName,
                KeyConditionExpression = "#b = :b AND #c > :c",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":b", new AttributeValue(bucket) },
                    { ":c", new AttributeValue { N = from.ToString() } }
                },
                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    { "#b", "bucket" },
                    { "#c", "created" }
                },
                ScanIndexForward = false,
                Limit = 25,
            },
            cancellationToken);

        return response.Items.Select(i =>
                new IncomingEmail(
                    i["to"].S,
                    i["from"].S,
                    i["subject"].S,
                    DateTimeOffset.FromUnixTimeSeconds(long.Parse(i["created"].N)),
                    i["s3Key"].S
                ))
            .ToImmutableList();
    }
}