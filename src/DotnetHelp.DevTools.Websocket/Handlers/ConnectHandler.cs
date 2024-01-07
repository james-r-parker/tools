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
        input.QueryStringParameters.TryGetValue("bucket", out var bucket);

        context.Logger.LogInformation($"Bucket: {bucket}");

        await Db.PutItemAsync(new PutItemRequest
        {
            TableName = Constants.ConnectionTableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "connectionId", new AttributeValue(input.RequestContext.ConnectionId) },
                { "bucket", bucket is null ? new AttributeValue { NULL = true } : new AttributeValue(bucket) },
                { "createdAt", new AttributeValue { N = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() } },
                { "ttl", new AttributeValue { N = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds().ToString() } },
            }
        });
    }
}