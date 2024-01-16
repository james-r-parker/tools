﻿namespace DotnetHelp.DevTools.Api.Application;

public interface IWebsocketClient
{
    Task SendMessage(WebSocketMessage request, CancellationToken cancellationToken);
}

public class WebsocketClient(IAmazonApiGatewayManagementApi wss, IAmazonDynamoDB db, ILogger<WebsocketClient> logger)
    : IWebsocketClient
{
    public async Task SendMessage(WebSocketMessage request, CancellationToken cancellationToken)
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
            },
            cancellationToken);

        string payload =
            JsonSerializer.Serialize(request, WebsocketJsonSerializerContext.Default.WebSocketMessage);

        foreach (var connection in connections.Items)
        {
            try
            {
                await wss.PostToConnectionAsync(new PostToConnectionRequest
                    {
                        ConnectionId = connection["connectionId"].S,
                        Data = new MemoryStream(Encoding.UTF8.GetBytes(payload)),
                    },
                    cancellationToken);
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

[JsonSerializable(typeof(WebSocketMessage))]
internal partial class WebsocketJsonSerializerContext : JsonSerializerContext
{
}