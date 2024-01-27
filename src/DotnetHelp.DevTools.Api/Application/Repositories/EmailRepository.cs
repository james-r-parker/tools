using System.Collections.Immutable;

namespace DotnetHelp.DevTools.Api.Application.Repositories;

internal interface IEmailRepository
{
    Task<IReadOnlyCollection<IncomingEmail>> List(
        string bucket, long from, CancellationToken cancellationToken);

    Task Delete(string bucket, long from, CancellationToken cancellationToken);
}

internal class EmailRepository(IAmazonDynamoDB db) : IEmailRepository
{
    private const string Prefix = "incoming_email-";

    public async Task<IReadOnlyCollection<IncomingEmail>> List(
        string bucket, long from, CancellationToken cancellationToken)
    {
        QueryResponse response = await db.QueryAsync(new QueryRequest
            {
                TableName = Constants.DbTableName,
                KeyConditionExpression = "#b = :b AND #c > :c",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":b", new AttributeValue($"{Prefix}{bucket}") },
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

    public Task Delete(string bucket, long from, CancellationToken cancellationToken)
    {
        return db.DeleteItemAsync(new DeleteItemRequest
        {
            TableName = Constants.DbTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "bucket", new AttributeValue($"{Prefix}{bucket}") },
                {
                    "created", new AttributeValue { N = from.ToString() }
                }
            }
        }, cancellationToken);
    }
}