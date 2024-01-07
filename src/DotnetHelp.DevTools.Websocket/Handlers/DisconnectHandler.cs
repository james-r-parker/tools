using Amazon.DynamoDBv2.Model;
using DotnetHelp.DevTools.Websocket.Application;

namespace DotnetHelp.DevTools.Websocket.Handlers;

public static class DisconnectHandler
{
    private static readonly AmazonDynamoDBClient Db;

    static DisconnectHandler()
    {
        Db = new AmazonDynamoDBClient();
    }

    public static async Task HandleAsync(APIGatewayProxyRequest input, ILambdaContext context)
    {
        await Db.DeleteItemAsync(new DeleteItemRequest
        {
            TableName = Constants.ConnectionTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "connectionId", new AttributeValue(input.RequestContext.ConnectionId) }
            }
        });
    }
}