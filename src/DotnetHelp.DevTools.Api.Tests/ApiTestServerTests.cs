using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DotnetHelp.DevTools.Api.Tests;

public class ApiTestServerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _server;
    private readonly HttpClient _client;

    public ApiTestServerTests()
    {
        Environment.SetEnvironmentVariable(
            "WEBSOCKET_URL",
            "ws://localhost:5000/ws",
            EnvironmentVariableTarget.Process);

        _server = new WebApplicationFactory<Program>();
        _client = _server.CreateClient();
    }

    [Fact]
    public async Task Hash()
    {
        var request =
            new HashApiRequest("MD5", "Hello World!", "UTF8", "HEX", null);

        using HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/hash",
            request);

        TextApiResponse? result = await response.Content.ReadFromJsonAsync<TextApiResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("ED076287532E86365E841E92BFC50D8C", result.Response);
    }

    public void Dispose()
    {
        _client.Dispose();
        _server.Dispose();
    }
}