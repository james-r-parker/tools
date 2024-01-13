using DotnetHelp.DevTools.Api.Handlers.Base64;

namespace DotnetHelp.DevTools.Api.Tests;

public class Base64HandlerTests
{
    [Fact]
    public void Base64Encode()
    {
        IResult result = Base64Handler.Encode(new TextApiRequest("Hello World"));
        Ok<TextApiResponse> response = Assert.IsType<Ok<TextApiResponse>>(result);
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response);
        Assert.NotNull(response.Value);
        Assert.Equal("SGVsbG8gV29ybGQ=", response.Value.Response);
    }
}