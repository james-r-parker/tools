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

        services
            .AddSingleton<IDistributedCache, DynamoDbDistributedCache>()
            .AddMemoryCache((o) =>
            {
                o.SizeLimit = 20 * (1024 * 1024); // 20 MB
                o.CompactionPercentage = 0.1; // 10% compaction rate
                o.ExpirationScanFrequency = TimeSpan.FromMinutes(1); // 1 minute expiration scan frequency
            });

        return services;
    }
}