namespace DotnetHelp.DevTools.Api.Application;

public interface IHttpRequestRepository
{
    Task Save(HttpRequest request);
    Task<IReadOnlyCollection<HttpRequest>> List(string bucket, int from);
}

public class HttpRequestRepository(IAmazonDynamoDB db) : IHttpRequestRepository
{
    public Task Save(HttpRequest request)
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

        return db.PutItemAsync(new PutItemRequest
        {
            TableName = Constants.HttpRequestTableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "bucket", new AttributeValue(request.Bucket) },
                { "created", new AttributeValue { N = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() } },
                { "ttl", new AttributeValue { N = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds().ToString() } },
                { "headers", headers },
                { "query", query },
                { "body", new AttributeValue(request.Body) }
            }
        });
    }

    public async Task<IReadOnlyCollection<HttpRequest>> List(string bucket, int from)
    {
        QueryResponse response = await db.QueryAsync(new QueryRequest
        {
            TableName = Constants.HttpRequestTableName,
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
        });

        return response.Items.Select(x => new HttpRequest(
                x["bucket"].S,
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(x["created"].N)),
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(x["ttl"].N)),
                x["headers"].M.ToDictionary(y => y.Key, y => y.Value.S),
                x["query"].M.ToDictionary(y => y.Key, y => y.Value.S),
                x["body"].S))
            .ToFrozenSet();
    }
}

public record HttpRequest(
    string Bucket,
    DateTimeOffset Created,
    DateTimeOffset Ttl,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyDictionary<string, string> Query,
    string Body);