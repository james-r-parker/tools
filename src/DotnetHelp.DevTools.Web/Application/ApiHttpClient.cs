using System.Net.Http.Json;
using DotnetHelp.DevTools.Shared;

namespace DotnetHelp.DevTools.Web.Application;

public class ApiHttpClient(HttpClient httpClient)
{
    public async Task<TextApiResponse> Base64Encode(TextApiRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("/api/base64/encode", request);
        return await response.Content.ReadFromJsonAsync<TextApiResponse>();
    }
}