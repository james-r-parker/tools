using System.Collections.Immutable;

namespace DotnetHelp.DevTools.Api.Application.Repositories;

internal interface IHttpMockRepository
{
    Task<HttpMockOverview> Increment(HttpMock mock, CancellationToken cancellationToken);
    Task Create(NewHttpMock request, CancellationToken cancellationToken);
    Task Delete(string bucket, long created, CancellationToken cancellationToken);
    Task<HttpMock?> Get(string bucket, string slug, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<HttpMockOverview>> List(string bucket, long from, CancellationToken cancellationToken);
}

internal class HttpMockRepository(IAmazonDynamoDB db) : IHttpMockRepository
{
    private const string Prefix = "http_mock-";

    public async Task<HttpMockOverview> Increment(HttpMock mock, CancellationToken cancellationToken)
    {
        var result = await db.UpdateItemAsync(new UpdateItemRequest
            {
                TableName = Constants.DbTableName,
                ReturnValues = ReturnValue.UPDATED_NEW,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "bucket", new AttributeValue(mock.Bucket) },
                    { "created", new AttributeValue { N = mock.Created.ToUnixTimeSeconds().ToString() } }
                },
                AttributeUpdates = new Dictionary<string, AttributeValueUpdate>
                {
                    {
                        "ttl",
                        new AttributeValueUpdate(
                            new AttributeValue
                                { N = DateTimeOffset.UtcNow.AddMonths(1).ToUnixTimeSeconds().ToString() },
                            AttributeAction.PUT)
                    },
                    {
                        "executions",
                        new AttributeValueUpdate(
                            new AttributeValue { N = "1" },
                            AttributeAction.ADD)
                    },
                    {
                        "last_executed",
                        new AttributeValueUpdate(
                            new AttributeValue { N = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
                            AttributeAction.PUT)
                    }
                }
            },
            cancellationToken);

        if (result is null)
        {
            throw new ApplicationException("Failed to update Http Mock");
        }

        return new HttpMockOverview(
            mock.Slug,
            mock.Method,
            int.Parse(result.Attributes["executions"].N),
            mock.Created,
            DateTimeOffset.FromUnixTimeSeconds(long.Parse(result.Attributes["last_executed"].N)));
    }

    public Task Create(NewHttpMock request, CancellationToken cancellationToken)
    {
        var headers = new AttributeValue { M = new Dictionary<string, AttributeValue>() };
        if (request.Headers.Count > 0)
        {
            foreach (var header in request.Headers)
            {
                headers.M.TryAdd(header.Key, new AttributeValue(header.Value));
            }
        }
        else
        {
            headers.M.TryAdd("Content-Type", new AttributeValue("text/plain"));
        }

        return db.PutItemAsync(new PutItemRequest
            {
                TableName = Constants.DbTableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "bucket", new AttributeValue($"{Prefix}{request.Bucket}") },
                    { "created", new AttributeValue { N = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() } },
                    {
                        "ttl",
                        new AttributeValue { N = DateTimeOffset.UtcNow.AddMonths(1).ToUnixTimeSeconds().ToString() }
                    },
                    { "slug", new AttributeValue(request.Slug) },
                    { "method", new AttributeValue(request.Method) },
                    { "headers", headers },
                    { "body", new AttributeValue(request.Body) },
                    { "executions", new AttributeValue() { N = "0" } },
                    { "updated", new AttributeValue { N = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() } },
                    { "last_executed", new AttributeValue { NULL = true } },
                }
            },
            cancellationToken);
    }

    public Task Delete(string bucket, long created, CancellationToken cancellationToken)
    {
        return db.DeleteItemAsync(new DeleteItemRequest
            {
                TableName = Constants.DbTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "bucket", new AttributeValue($"{Prefix}{bucket}") },
                    { "created", new AttributeValue { N = created.ToString() } }
                }
            },
            cancellationToken);
    }

    public async Task<HttpMock?> Get(string bucket, string slug, CancellationToken cancellationToken)
    {
        var response = await db.QueryAsync(new QueryRequest()
        {
            TableName = Constants.DbTableName,
            IndexName = "ix_bucket_slug",
            KeyConditionExpression = "#b = :b AND #s = :s",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":b", new AttributeValue($"{Prefix}{bucket}") },
                { ":s", new AttributeValue(slug) }
            },
            ExpressionAttributeNames = new Dictionary<string, string>()
            {
                { "#b", "bucket" },
                { "#s", "slug" }
            },
            Limit = 1,
        }, cancellationToken);

        return response.Items.Select(x => new HttpMock(
                x["bucket"].S,
                x["slug"].S,
                x["method"].S,
                x["headers"].M.ToDictionary(y => y.Key, y => y.Value.S),
                x["body"].S,
                int.Parse(x["executions"].N),
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(x["ttl"].N)),
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(x["created"].N)),
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(x["updated"].N)),
                x["last_executed"].NULL ? null : DateTimeOffset.FromUnixTimeSeconds(long.Parse(x["last_executed"].N))))
            .FirstOrDefault();
    }

    public async Task<IReadOnlyCollection<HttpMockOverview>> List(string bucket, long from,
        CancellationToken cancellationToken)
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

        return response.Items.Select(x => new HttpMockOverview(
                x["slug"].S,
                x["method"].S,
                int.Parse(x["executions"].N),
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(x["created"].N)),
                x["last_executed"].NULL ? null : DateTimeOffset.FromUnixTimeSeconds(long.Parse(x["last_executed"].N))))
            .ToImmutableList();
    }
}