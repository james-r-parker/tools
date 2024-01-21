using System.Net;

namespace DotnetHelp.DevTools.Web.Application;

public class AlgoliaSearchClient(HttpClient client)
{
	public async Task<IReadOnlyCollection<SearchResult>> Search(string query, CancellationToken cancellationToken)
	{
		try
		{
			var response = await client.GetAsync($"/1/indexes/dotnethelp?query={WebUtility.UrlEncode(query)}", cancellationToken);
			response.EnsureSuccessStatusCode();
			string? content = await response.Content.ReadAsStringAsync(cancellationToken);
			if (content is null)
			{
				return Array.Empty<SearchResult>();
			}
			AlgoliaHits? result = JsonSerializer.Deserialize<AlgoliaHits>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
			return result?.Hits ?? Array.Empty<SearchResult>();
		}
		catch
		{

		}

		return Array.Empty<SearchResult>();
	}
}

public record AlgoliaHits(IReadOnlyCollection<SearchResult> Hits);

public record SearchResult(string ObjectId, string Group, string Name, string Description, string[] Tags);
