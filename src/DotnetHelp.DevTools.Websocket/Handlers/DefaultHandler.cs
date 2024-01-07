using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using DotnetHelp.DevTools.Websocket.Application;

namespace DotnetHelp.DevTools.Websocket.Handlers;

public static class DefaultHandler
{
    private static readonly AmazonApiGatewayManagementApiClient Wss;

    static DefaultHandler()
    {
        Wss = new AmazonApiGatewayManagementApiClient(
            new AmazonApiGatewayManagementApiConfig
            {
                ServiceURL = Constants.WebsocketUrl
            });
    }

    public static async Task HandleAsync(APIGatewayProxyRequest input, ILambdaContext context)
    {
        await Wss.PostToConnectionAsync(new PostToConnectionRequest
        {
            ConnectionId = input.RequestContext.ConnectionId,
            Data = new MemoryStream("OK"u8.ToArray())
        });
    }
}