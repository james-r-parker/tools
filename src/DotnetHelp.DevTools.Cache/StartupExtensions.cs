using Amazon.DynamoDBv2;
using DotnetHelp.DevTools.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotnetHelp.DevTools;

public static class StartupExtensions
{
    public static IServiceCollection AddDotnetHelpCache(this IServiceCollection services)
    {
        services
            .TryAddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
        
        services.AddSingleton<IDistributedCache, DynamoDbDistributedCache>();
        
        return services;
    }
}