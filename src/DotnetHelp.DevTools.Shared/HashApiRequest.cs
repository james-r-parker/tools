namespace DotnetHelp.DevTools.Shared;

public record HashApiRequest(string Algorithm, string Message, string Encoding, string Format, string? Secret = null);