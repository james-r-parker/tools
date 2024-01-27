using System.Collections.Immutable;

namespace DotnetHelp.DevTools.Api.Application.Repositories;

internal interface IHttpRequestRepository
{
    Task Save(BucketHttpRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BucketHttpRequest>> List(string bucket, long from, CancellationToken cancellationToken);
}

internal class HttpRequestRepository(IAmazonDynamoDB db) : IHttpRequestRepository
{
    private const string Prefix = "http_request-";
    
    public Task Save(BucketHttpRequest request, CancellationToken cancellationToken)
    {
        var headers = new AttributeValue();
        foreach (var header in request.Headers)
        {
            headers.M ??= new Dictionary<string, AttributeValue>();
            headers.M.TryAdd(header.Key, new AttributeValue(header.Value));
        }

        var query = new AttributeValue();
        if (request.Query.Count > 0)
        {
            foreach (var q in request.Query)
            {
                query.M ??= new Dictionary<string, AttributeValue>();
                query.M.TryAdd(q.Key, new AttributeValue(q.Value));
            }
        }
        else
        {
            query.NULL = true;
        }

        var ip = new AttributeValue();
        if (!string.IsNullOrWhiteSpace(request.IpAddress))
        {
            ip.S = request.IpAddress;
        }
        else
        {
            ip.NULL = true;
        }

        return db.PutItemAsync(new PutItemRequest
            {
                TableName = Constants.BinTableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "bucket", new AttributeValue($"{Prefix}{request.Bucket}") },
                    { "created", new AttributeValue { N = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() } },
                    {
                        "ttl",
                        new AttributeValue { N = DateTimeOffset.UtcNow.AddDays(Constants.TTLDays).ToUnixTimeSeconds().ToString() }
                    },
                    { "ip", ip },
                    { "headers", headers },
                    { "query", query },
                    { "body", new AttributeValue(request.Body) }
                }
            },
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<BucketHttpRequest>> List(string bucket, long from,
        CancellationToken cancellationToken)
    {
        QueryResponse response = await db.QueryAsync(new QueryRequest
            {
                TableName = Constants.BinTableName,
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

        return response.Items.Select(x => new BucketHttpRequest(
                x["bucket"].S,
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(x["created"].N)),
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(x["ttl"].N)),
                x["ip"].NULL ? null : x["ip"].S,
                x["headers"].M.ToDictionary(y => y.Key, y => y.Value.S),
                x["query"].M.ToDictionary(y => y.Key, y => y.Value.S),
                x["body"].S))
            .ToImmutableList();
    }
}