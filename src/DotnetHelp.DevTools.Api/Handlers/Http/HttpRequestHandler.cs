namespace DotnetHelp.DevTools.Api.Handlers.Http;

internal static class HttpRequestHandler
{
    internal static async Task<IResult> New(
        [FromRoute] string bucket,
        [FromServices] IAmazonDynamoDB db,
        [FromServices] IAmazonApiGatewayManagementApi wss,
        [FromServices] IHttpContextAccessor http)
    {
        if (string.IsNullOrWhiteSpace(bucket))
        {
            return Results.BadRequest();
        }

        if (http?.HttpContext is null)
        {
            return Results.BadRequest();
        }

        var headers = new AttributeValue();
        foreach (var header in http.HttpContext.Request.Headers)
        {
            headers.M ??= new Dictionary<string, AttributeValue>();
            headers.M.Add(header.Key, new AttributeValue(header.Value));
        }

        var query = new AttributeValue();
        if (http.HttpContext.Request.Query.Any())
        {
            foreach (var q in http.HttpContext.Request.Query)
            {
                query.M ??= new Dictionary<string, AttributeValue>();
                query.M.Add(q.Key, new AttributeValue(q.Value));
            }
        }
        else
        {
            query.NULL = true;
        }

        var body = new AttributeValue();

        using (StreamReader reader
               = new StreamReader(http.HttpContext.Request.Body, Encoding.UTF8, true, 1024, true))
        {
            body.S = await reader.ReadToEndAsync();
        }

        await db.PutItemAsync(new PutItemRequest
        {
            TableName = Constants.HttpRequestTableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "bucket", new AttributeValue(bucket) },
                { "created", new AttributeValue { N = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() } },
                { "ttl", new AttributeValue { N = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds().ToString() } },
                { "headers", headers },
                { "query", query },
                { "body", body }
            },
        });

        QueryResponse? connections = await db.QueryAsync(new QueryRequest
        {
            TableName = Constants.ConnectionTableName,
            IndexName = "ix_bucket_connection",
            KeyConditionExpression = "#b = :b",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":b", new AttributeValue(bucket) }
            },
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#b", "bucket" }
            }
        });

        var payload =
            JsonSerializer.Serialize(new NewHttpRequestWssMessage("NEW_HTTP_REQUEST", bucket),
                HttpRequestJsonSerializerContext.Default.NewHttpRequestWssMessage);

        foreach (var connection in connections.Items)
        {
            await wss.PostToConnectionAsync(new PostToConnectionRequest
            {
                ConnectionId = connection["connectionId"].S,
                Data = new MemoryStream(Encoding.UTF8.GetBytes(payload)),
            });
        }

        return Results.Ok();
    }
}

public record NewHttpRequestWssMessage(string Action, string Bucket);

[JsonSerializable(typeof(NewHttpRequestWssMessage))]
internal partial class HttpRequestJsonSerializerContext : JsonSerializerContext
{
}