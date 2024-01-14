namespace DotnetHelp.DevTools.Shared;

public record WebSocketMessage(string Action, string Bucket, string? Payload = null);