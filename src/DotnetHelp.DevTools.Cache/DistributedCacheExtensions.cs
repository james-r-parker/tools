using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Caching.Distributed;

namespace DotnetHelp.DevTools.Cache;

public static class DistributedCacheExtensions
{
    public static async Task<TValue?> GetAsync<TValue>(
        this IDistributedCache cache,
        string key,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        byte[]? cached = await cache.GetAsync(key, cancellationToken);

        if (cached is null)
        {
            return default;
        }

        return JsonSerializer.Deserialize(cached, jsonTypeInfo);
    }

    public static Task SetAsync<TValue>(
        this IDistributedCache cache,
        string key,
        TValue value,
        JsonTypeInfo<TValue> jsonTypeInfo,
        TimeSpan absoluteExpirationRelativeToNow,
        CancellationToken cancellationToken = default)
    {
        return cache.SetAsync(
            key,
            JsonSerializer.SerializeToUtf8Bytes(value, jsonTypeInfo),
            new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow },
            cancellationToken);
    }
}