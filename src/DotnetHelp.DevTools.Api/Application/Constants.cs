namespace DotnetHelp.DevTools.Api.Application;

public static class Constants
{
    public static string ConnectionTableName =>
        Environment.GetEnvironmentVariable("CONNECTION_TABLE_NAME")
        ?? throw new Exception("CONNECTION_TABLE_NAME environment variable not set");

    public static string HttpRequestTableName =>
        Environment.GetEnvironmentVariable("HTTP_REQUEST_TABLE_NAME")
        ?? throw new Exception("CONNECTION_TABLE_NAME environment variable not set");
    
    public static string CacheTableName =>
        Environment.GetEnvironmentVariable("CACHE_TABLE_NAME")
        ?? throw new Exception("CACHE_TABLE_NAME environment variable not set");
    
    public static string WebsocketUrl =>
        Environment.GetEnvironmentVariable("WEBSOCKET_URL")
        ?? throw new Exception("WEBSOCKET_URL environment variable not set");
}