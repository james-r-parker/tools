using Amazon.ApiGatewayManagementApi;
using Amazon.DynamoDBv2;
using DotnetHelp.DevTools.WebsocketClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotnetHelp.DevTools;

public static class StartupExtensions
{
    public static IServiceCollection AddDotnetHelpWebsocketClient(this IServiceCollection services)
    {
        services
            .TryAddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
        
        services.TryAddSingleton<IAmazonApiGatewayManagementApi>(new AmazonApiGatewayManagementApiClient(
            new AmazonApiGatewayManagementApiConfig
            {
                ServiceURL = Constants.WebsocketUrl
            }));
        
        services.AddSingleton<IWebsocketClient, ApiGatewayWebsocketClient>();
        
        return services;
    }
}