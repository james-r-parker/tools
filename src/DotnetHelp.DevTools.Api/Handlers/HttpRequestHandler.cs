using System.Text.Json;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2.Model;

namespace DotnetHelp.DevTools.Api.Handlers;

internal static class HttpRequestHandler
{
    internal static async Task<IResult> New(
        [FromRoute] string bucket,
        [FromServices] IAmazonDynamoDB db,
        [FromServices] IAmazonApiGatewayManagementApi wss,
        [FromServices] IHttpContextAccessor http)
    {
        await db.PutItemAsync(new PutItemRequest
        {
            TableName = Constants.HttpRequestTableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "bucket", new AttributeValue(bucket) },
                { "created", new AttributeValue(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()) },
                { "ttl", new AttributeValue((DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds()).ToString()) }
            }
        });

        QueryResponse? connections = await db.QueryAsync(new QueryRequest
        {
            TableName = Constants.ConnectionTableName,
            IndexName = "ix_bucket_connection",
            KeyConditionExpression = "bucket = :bucket",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":bucket", new AttributeValue(bucket) }
            },
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