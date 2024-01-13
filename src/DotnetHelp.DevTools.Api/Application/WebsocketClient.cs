namespace DotnetHelp.DevTools.Api.Application;

public interface IWebsocketClient
{
    Task SendMessage(NewHttpRequestWssMessage request);
}

public class WebsocketClient(IAmazonApiGatewayManagementApi wss, IAmazonDynamoDB db, ILogger<WebsocketClient> logger)
    : IWebsocketClient
{
    public async Task SendMessage(NewHttpRequestWssMessage request)
    {
        QueryResponse? connections = await db.QueryAsync(new QueryRequest
        {
            TableName = Constants.ConnectionTableName,
            IndexName = "ix_bucket_connection",
            KeyConditionExpression = "#b = :b",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":b", new AttributeValue(request.Bucket) }
            },
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#b", "bucket" }
            }
        });

        string payload =
            JsonSerializer.Serialize(request, WebsocketJsonSerializerContext.Default.NewHttpRequestWssMessage);

        foreach (var connection in connections.Items)
        {
            try
            {
                await wss.PostToConnectionAsync(new PostToConnectionRequest
                {
                    ConnectionId = connection["connectionId"].S,
                    Data = new MemoryStream(Encoding.UTF8.GetBytes(payload)),
                });
            }
            catch (Exception e)
            {
                logger.SendMessage(e, connection["connectionId"].S);
            }
        }
    }
}

internal static partial class WebsocketClientLogger
{
    [LoggerMessage(LogLevel.Warning, "Failed to send websocket message to {connectionId}")]
    public static partial void SendMessage(this ILogger<WebsocketClient> logger, Exception ex, string connectionId);
}

public record NewHttpRequestWssMessage(string Action, string Bucket, string? Payload = null);

[JsonSerializable(typeof(NewHttpRequestWssMessage))]
internal partial class WebsocketJsonSerializerContext : JsonSerializerContext
{
}