using System.Net.Http.Json;

namespace DotnetHelp.DevTools.Web;

public class ApiHttpClient(HttpClient httpClient)
{
    public async Task<TextApiResponse?> Base64Encode(TextApiRequest request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = 
            await httpClient.PostAsJsonAsync("/api/base64/encode", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TextApiResponse>(cancellationToken);
    }

    public async Task<IReadOnlyCollection<BucketHttpRequest>> GetHttpRequests(string bucket, long from,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = 
            await httpClient.GetAsync($"/api/http/{bucket}?from={from}", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<BucketHttpRequest>>(cancellationToken);
    }
}