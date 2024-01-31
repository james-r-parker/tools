using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DotnetHelp.DevTools.Shared;
using Microsoft.Extensions.Logging;

namespace DotnetHelp.DevTools.WebsocketClient;

public interface IWebsocketClient
{
    Task SendMessage(WebSocketMessage request, CancellationToken cancellationToken);
}

internal class ApiGatewayWebsocketClient(
    IAmazonApiGatewayManagementApi wss,
    IAmazonDynamoDB db,
    ILogger<ApiGatewayWebsocketClient> logger)
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
                
                logger.Send(connection["connectionId"].S);
            }
            catch (GoneException)
            {
                logger.Gone(connection["connectionId"].S);
            }
            catch (Exception e)
            {
                logger.Error(e, connection["connectionId"].S);
            }
        }
    }
}

internal static partial class WebsocketClientLogger
{
    [LoggerMessage(LogLevel.Error, "Failed to send websocket message to {connectionId}")]
    public static partial void Error(this ILogger<ApiGatewayWebsocketClient> logger, Exception ex, string connectionId);

    [LoggerMessage(LogLevel.Information, "Connection {connectionId} has gone away")]
    public static partial void Gone(this ILogger<ApiGatewayWebsocketClient> logger, string connectionId);

    [LoggerMessage(LogLevel.Information, "Sent message to connection {connectionId}")]
    public static partial void Send(this ILogger<ApiGatewayWebsocketClient> logger, string connectionId);
}

[JsonSerializable(typeof(WebSocketMessage))]
internal partial class WebsocketJsonSerializerContext : JsonSerializerContext
{
}