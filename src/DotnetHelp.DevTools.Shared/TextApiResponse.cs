namespace DotnetHelp.DevTools.Shared;

public record TextApiRequest(string Request);

public record TextApiResponse(string Response);

public record TextCollectionApiResponse(IReadOnlyCollection<TextApiResponse> Response);