namespace DotnetHelp.DevTools.Cache;

internal static class Constants
{
    public static string CacheTableName =>
        Environment.GetEnvironmentVariable("DISTRIBUTEDCACHE_TABLE_NAME")
        ?? throw new Exception("CACHE_TABLE_NAME environment variable not set");
}