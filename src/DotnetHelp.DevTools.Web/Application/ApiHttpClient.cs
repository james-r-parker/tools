using System.Net;
using System.Net.Http.Json;

namespace DotnetHelp.DevTools.Web;

internal class ApiHttpClient(HttpClient httpClient)
{
	public async Task<IReadOnlyCollection<HttpMockOverview>> ListHttpMocks(string bucket, long from,
		CancellationToken cancellationToken)
	{
		using HttpResponseMessage response =
			await httpClient.GetAsync($"/api/mock/{bucket}?created={from}", cancellationToken);

		response.EnsureSuccessStatusCode();

		IReadOnlyCollection<HttpMockOverview>? result =
			await response.Content.ReadFromJsonAsync<IReadOnlyCollection<HttpMockOverview>>(cancellationToken);

		return result ?? Array.Empty<HttpMockOverview>();
	}

	public async Task CreateHttpMock(NewHttpMock model,
		CancellationToken cancellationToken)
	{
		using HttpResponseMessage response =
			await httpClient.PostAsJsonAsync($"/api/mock", model, cancellationToken);

		response.EnsureSuccessStatusCode();
	}

	public async Task UpdateHttpMock(HttpMock model,
		CancellationToken cancellationToken)
	{
		using HttpResponseMessage response =
			await httpClient.PutAsJsonAsync($"/api/mock", model, cancellationToken);

		response.EnsureSuccessStatusCode();
	}

	public async Task DeleteHttpMock(string bucket, long from,
		CancellationToken cancellationToken)
	{
		using HttpResponseMessage response =
			await httpClient.DeleteAsync($"/api/mock/{bucket}/{from}", cancellationToken);

		response.EnsureSuccessStatusCode();
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

	public async Task DeleteHttpRequest(string bucket, long created, CancellationToken cancellationToken)
	{
		HttpResponseMessage response =
			await httpClient.DeleteAsync($"/api/http/{bucket}/{created}", cancellationToken);

		response.EnsureSuccessStatusCode();
	}

	public async Task DeleteIncomingEmail(string bucket, long created, CancellationToken cancellationToken)
	{
		HttpResponseMessage response =
			await httpClient.DeleteAsync($"/api/email/{bucket}/{created}", cancellationToken);

		response.EnsureSuccessStatusCode();
	}

	public async Task<bool> ApiHealth(CancellationToken cancellationToken)
	{
		HttpResponseMessage response =
			await httpClient.GetAsync("/api/_health", cancellationToken);

		if (response.StatusCode != HttpStatusCode.OK)
		{
			return false;
		}

		var body = await response.Content.ReadAsStringAsync(cancellationToken);

		if (body == "Healthy")
		{
			return true;
		}

		return false;
	}
}