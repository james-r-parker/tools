using System.Net.Http.Json;

namespace DotnetHelp.DevTools.Web;

internal class ApiHttpClient(HttpClient httpClient)
{
	public async Task<TextApiResponse?> Base64Encode(TextApiRequest request, CancellationToken cancellationToken)
	{
		HttpResponseMessage response =
			await httpClient.PostAsJsonAsync("/api/base64/encode", request, cancellationToken);
		response.EnsureSuccessStatusCode();

		return await response.Content.ReadFromJsonAsync<TextApiResponse>(cancellationToken);
	}

	public async Task<TextApiResponse?> Base64Decode(TextApiRequest request, CancellationToken cancellationToken)
	{
		HttpResponseMessage response =
			await httpClient.PostAsJsonAsync("/api/base64/decode", request, cancellationToken);
		response.EnsureSuccessStatusCode();

		return await response.Content.ReadFromJsonAsync<TextApiResponse>(cancellationToken);
	}

	public async Task<IReadOnlyCollection<BucketHttpRequest>> GetHttpRequests(string bucket, long from,
		CancellationToken cancellationToken)
	{
		using HttpResponseMessage response =
			await httpClient.GetAsync($"/api/http/{bucket}?from={from}", cancellationToken);

		response.EnsureSuccessStatusCode();

		IReadOnlyCollection<BucketHttpRequest>? result =
			await response.Content.ReadFromJsonAsync<IReadOnlyCollection<BucketHttpRequest>>(cancellationToken);

		return result ?? Array.Empty<BucketHttpRequest>();
	}

	public async Task<IReadOnlyCollection<IncomingEmail>> GetIncomingEmails(string bucket, long from,
		CancellationToken cancellationToken)
	{
		using HttpResponseMessage response =
			await httpClient.GetAsync($"/api/email/{bucket}?from={from}", cancellationToken);

		response.EnsureSuccessStatusCode();

		IReadOnlyCollection<IncomingEmail>? result =
			await response.Content.ReadFromJsonAsync<IReadOnlyCollection<IncomingEmail>>(cancellationToken);

		return result ?? Array.Empty<IncomingEmail>();
	}

	public async Task<OutgoingHttpResponse?> SendHttpRequest(OutgoingHttpRequest request, CancellationToken cancellationToken)
	{
		HttpResponseMessage response =
			await httpClient.PostAsJsonAsync("/api/http", request, cancellationToken);
		response.EnsureSuccessStatusCode();

		return await response.Content.ReadFromJsonAsync<OutgoingHttpResponse>(cancellationToken);
	}

	public async Task<DnsLookupResponses?> DnsLookup(string domain, CancellationToken cancellationToken)
	{
		using HttpResponseMessage response =
			await httpClient.GetAsync($"/api/dns/{domain}", cancellationToken);

		response.EnsureSuccessStatusCode();

		return await response.Content.ReadFromJsonAsync<DnsLookupResponses>(cancellationToken);
	}

	public async Task<TextApiResponse?> Hash(HashApiRequest request, CancellationToken cancellationToken)
	{
		HttpResponseMessage response =
			await httpClient.PostAsJsonAsync("/api/hash", request, cancellationToken);

		response.EnsureSuccessStatusCode();


		return await response.Content.ReadFromJsonAsync<TextApiResponse>(cancellationToken);
	}

	public async Task<JwtApiResponse?> JwtDecode(JwtApiRequest request, CancellationToken cancellationToken)
	{
		HttpResponseMessage response =
			await httpClient.PostAsJsonAsync("/api/jwt/decode", request, cancellationToken);

		response.EnsureSuccessStatusCode();


		return await response.Content.ReadFromJsonAsync<JwtApiResponse>(cancellationToken);
	}
}