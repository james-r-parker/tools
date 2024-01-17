namespace DotnetHelp.DevTools.WebsocketClient;

internal static class Constants
{
    public static string WebsocketUrl =>
        Environment.GetEnvironmentVariable("WEBSOCKET_URL")
        ?? throw new Exception("WEBSOCKET_URL environment variable not set");
    
    public static string ConnectionTableName =>
        Environment.GetEnvironmentVariable("CONNECTION_TABLE_NAME")
        ?? throw new Exception("CONNECTION_TABLE_NAME environment variable not set");
}