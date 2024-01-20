namespace DotnetHelp.DevTools.Shared;

public record IncomingEmail(
    string To,
    string From,
    string Subject,
    DateTimeOffset Created,
    string Key);