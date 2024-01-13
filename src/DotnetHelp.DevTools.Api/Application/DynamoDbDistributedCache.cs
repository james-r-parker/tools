using System.Runtime.CompilerServices;

namespace DotnetHelp.DevTools.Api.Application;

public class DynamoDbDistributedCache(IAmazonDynamoDB db) : IDistributedCache
{
    private const string PartitionKeyAttributeName = "id";
    private const string TTLAttributeName = "ttl";
    private const string ValueAttributeName = "value";
    private const string TTLWindowAttributeName = "ttl_window";
    private const string TTLDeadlineAttributeName = "ttl_deadline";

    public byte[]? Get(string key)
    {
        var byteArray = GetAsync(key).GetAwaiter().GetResult();
        return byteArray;
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        return await GetAndRefreshAsync(key, cancellationToken);
    }

    public void Refresh(string key)
    {
        RefreshAsync(key).GetAwaiter().GetResult();
    }

    public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
    {
        await GetAndRefreshAsync(key, cancellationToken);
    }

    public void Remove(string key)
    {
        RemoveAsync(key).GetAwaiter().GetResult();
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await db.DeleteItemAsync(new DeleteItemRequest
            {
                TableName = Constants.CacheTableName,
                Key = CreateDictionaryWithKey(key)
            }, cancellationToken);
        }
        catch (Exception e)
        {
            throw new DynamoDbDistributedCacheException(
                $"Failed to delete item with key {key}, Caused by {e.Message}",
                e);
        }
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        SetAsync(key, value, options).GetAwaiter().GetResult();
    }

    public async Task SetAsync(
        string key,
        byte[] value,
        DistributedCacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        var ttlDate = CalculateTTL(options);
        var ttlWindow = CalculateSlidingWindow(options);
        var ttlDeadline = CalculateTTLDeadline(options);

        try
        {
            await db.PutItemAsync(new PutItemRequest
            {
                TableName = Constants.CacheTableName,
                Item = new Dictionary<string, AttributeValue>()
                {
                    {
                        PartitionKeyAttributeName, new AttributeValue { S = key }
                    },
                    {
                        ValueAttributeName, new AttributeValue { B = new MemoryStream(value) }
                    },
                    {
                        TTLAttributeName, ttlDate
                    },
                    {
                        TTLWindowAttributeName, ttlWindow
                    },
                    {
                        TTLDeadlineAttributeName, ttlDeadline
                    }
                }
            }, cancellationToken);
        }
        catch (Exception e)
        {
            throw new DynamoDbDistributedCacheException(
                $"Failed to put item with key {key}. Caused by {e.Message}",
                e);
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Dictionary<string, AttributeValue> CreateDictionaryWithKey(string key)
    {
        return new Dictionary<string, AttributeValue>()
        {
            {
                PartitionKeyAttributeName, new AttributeValue { S = key }
            }
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DateTimeOffset UnixSecondsToDateTimeOffset(string seconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds((long)double.Parse(seconds));
    }

    private async Task<byte[]?> GetAndRefreshAsync(string key, CancellationToken cancellationToken = default)
    {
        GetItemResponse response;
        try
        {
            response = await db.GetItemAsync(new GetItemRequest
            {
                TableName = Constants.CacheTableName,
                Key = CreateDictionaryWithKey(key),
            }, cancellationToken);
        }
        catch (Exception e)
        {
            throw new DynamoDbDistributedCacheException(
                $"Failed to get Item with key {key}. Caused by {e.Message}",
                e);
        }

        if (!response.Item.TryGetValue(ValueAttributeName, out var value))
        {
            return null;
        }

        //Check if the TTL has expired. DynamoDB can take up to 48 hours to remove expired items.
        if (response.Item.TryGetValue(TTLAttributeName, out var ttl) &&
            ttl.N is not null &&
            DateTimeOffset.UtcNow.CompareTo(UnixSecondsToDateTimeOffset(ttl.N)) > 0)
        {
            return null;
        }

        if (!response.Item.TryGetValue(TTLWindowAttributeName, out var ttlWindow) ||
            ttlWindow.S is null ||
            ttl?.N is null)
        {
            return value.B.ToArray();
        }
        
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.Parse(ttlWindow.S),
        };

        if (response.Item.TryGetValue(TTLDeadlineAttributeName, out var ttlDeadline) &&
            ttlDeadline.N is not null)
        {
            options.AbsoluteExpiration = UnixSecondsToDateTimeOffset(ttlDeadline.N);
        }

        var ttlAttribute = CalculateTTL(options);

        try
        {
            await db.UpdateItemAsync(new UpdateItemRequest
            {
                TableName = Constants.CacheTableName,
                Key = CreateDictionaryWithKey(key),
                AttributeUpdates = new Dictionary<string, AttributeValueUpdate>
                {
                    {
                        TTLAttributeName, new AttributeValueUpdate
                        {
                            Value = ttlAttribute
                        }
                    }
                }
            }, cancellationToken);
        }
        catch (Exception e)
        {
            throw new DynamoDbDistributedCacheException(
                $"Failed to update TTL for item with key {key}. Caused by {e.Message}",
                e);
        }
        
        return value.B.ToArray();
    }
    
    private static AttributeValue CalculateTTLDeadline(DistributedCacheEntryOptions options)
    {
        if (options is { AbsoluteExpiration: not null, AbsoluteExpirationRelativeToNow: null })
        {
            return new AttributeValue { N = options.AbsoluteExpiration.Value.ToUnixTimeSeconds().ToString() };
        }

        if (options is { AbsoluteExpirationRelativeToNow: not null })
        {
            var ttl = DateTimeOffset.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value).ToUnixTimeSeconds();
            return new AttributeValue { N = ttl.ToString() };
        }

        return new AttributeValue { NULL = true };
    }

    private static AttributeValue CalculateTTL(DistributedCacheEntryOptions options)
    {
        //if the sliding window is present, then now + window
        if (options.SlidingExpiration != null)
        {
            var ttl = DateTimeOffset.UtcNow.Add(((TimeSpan)options.SlidingExpiration));
            //Cannot be later than the deadline
            var absoluteTTL = CalculateTTLDeadline(options);
            if (absoluteTTL.NULL)
            {
                return new AttributeValue { N = ttl.ToUnixTimeSeconds().ToString() };
            }

            //return smaller of the two. Either the TTL based on the sliding window or the deadline
            if (long.Parse(absoluteTTL.N) < ttl.ToUnixTimeSeconds())
            {
                return absoluteTTL;
            }

            return new AttributeValue { N = ttl.ToUnixTimeSeconds().ToString() };
        }
        else //just return the absolute TTL
        {
            return CalculateTTLDeadline(options);
        }
    }
    
    private static AttributeValue CalculateSlidingWindow(DistributedCacheEntryOptions options)
    {
        if (options.SlidingExpiration != null)
        {
            return new AttributeValue { S = options.SlidingExpiration.ToString() };
        }

        return new AttributeValue { NULL = true };
    }
}

public class DynamoDbDistributedCacheException(string message, Exception innerException)
    : Exception(message, innerException);