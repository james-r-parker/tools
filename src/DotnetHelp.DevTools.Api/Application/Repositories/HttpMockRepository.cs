using System.Collections.Immutable;

namespace DotnetHelp.DevTools.Api.Application.Repositories;

internal interface IHttpMockRepository
{
    Task Create(NewHttpMock request, CancellationToken cancellationToken);
    Task Update(HttpMock request, CancellationToken cancellationToken);
    Task Delete(string bucket, long created, CancellationToken cancellationToken);
    Task<HttpMock?> Get(string bucket, string slug, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<HttpMock>> List(string bucket, long from, CancellationToken cancellationToken);
}

internal class HttpMockRepository(IAmazonDynamoDB db) : IHttpMockRepository
{
    private const string Prefix = "http_mock-";

    public Task Create(NewHttpMock request, CancellationToken cancellationToken)
    {
        var headers = new AttributeValue();
        foreach (var header in request.Headers)
        {
            headers.M ??= new Dictionary<string, AttributeValue>();
            headers.M.TryAdd(header.Key, new AttributeValue(header.Value));
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
                    { "body", new AttributeValue(request.Body) }
                }
            },
            cancellationToken);
    }

    public Task Update(HttpMock request, CancellationToken cancellationToken)
    {
        var headers = new AttributeValue();
        foreach (var header in request.Headers)
        {
            headers.M ??= new Dictionary<string, AttributeValue>();
            headers.M.TryAdd(header.Key, new AttributeValue(header.Value));
        }

        return db.UpdateItemAsync(new UpdateItemRequest
            {
                TableName = Constants.DbTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "bucket", new AttributeValue(request.Bucket) },
                    { "created", new AttributeValue { N = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() } },
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
                    { "slug", new AttributeValueUpdate(new AttributeValue(request.Slug), AttributeAction.PUT) },
                    { "method", new AttributeValueUpdate(new AttributeValue(request.Method), AttributeAction.PUT) },
                    { "headers", new AttributeValueUpdate(headers, AttributeAction.PUT) },
                    { "body", new AttributeValueUpdate(new AttributeValue(request.Body), AttributeAction.PUT) }
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
                    { "bucket", new AttributeValue(bucket) },
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
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(x["ttl"].N)),
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(x["created"].N))))
            .FirstOrDefault();
    }

    public async Task<IReadOnlyCollection<HttpMock>> List(string bucket, long from,
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

        return response.Items.Select(x => new HttpMock(
                x["bucket"].S,
                x["slug"].S,
                x["method"].S,
                x["headers"].M.ToDictionary(y => y.Key, y => y.Value.S),
                x["body"].S,
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(x["ttl"].N)),
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(x["created"].N))))
            .ToImmutableList();
    }
}