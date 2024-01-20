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
        {
            var tos = i["to"].L.Select(e => new EmailAddress(
                e.M["name"].S,
                e.M["address"].S,
                e.M["domain"].S
            )).ToImmutableList();

            var froms = i["from"].L.Select(e => new EmailAddress(
                e.M["name"].S,
                e.M["address"].S,
                e.M["domain"].S
            )).ToImmutableList();

            var headers = i["headers"].L.Select(e => new EmailHeader(
                e.M["name"].S,
                e.M["value"].S
            )).ToImmutableList();

            var contents = i["content"].L.Select(e => new EmailContent(
                e.M["contentId"].S,
                e.M["contentType"].S,
                e.M["content"].S
            )).ToImmutableList();
            
            return new IncomingEmail(
                tos,
                froms,
                headers,
                contents,
                i["subject"].S,
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(i["created"].N))
            );
        })
        .ToImmutableList();
    }
}