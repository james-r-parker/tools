using Amazon.DynamoDBv2.Model;
using DotnetHelp.DevTools.Websocket.Application;

namespace DotnetHelp.DevTools.Websocket.Handlers;

public static class ConnectHandler
{
    private static readonly AmazonDynamoDBClient Db;

    static ConnectHandler()
    {
        Db = new AmazonDynamoDBClient();
    }

    public static async Task HandleAsync(APIGatewayProxyRequest input, ILambdaContext context)
    {
        context.Logger.LogInformation($"Bucket: {input.Headers["x-bucket"]}");
        
        await Db.PutItemAsync(new PutItemRequest
        {
            TableName = Constants.ConnectionTableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "connectionId", new AttributeValue(input.RequestContext.ConnectionId) },
                { "bucket", new AttributeValue(input.Headers["x-bucket"]) },
                { "createdAt", new AttributeValue { N = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() } }
            }
        });
    }
}