using DotnetHelp.DevTools.Api.Handlers.Hash;

namespace DotnetHelp.DevTools.Api.Tests;

public class HashHandlerTests
{
    private const string Message = "`#<12345678>` ~ Hello, ŵorld!\ud83e\udee0";
    private const string Secret = "secret";
    
    [Theory]
    [InlineData("MD5", Message, "UTF8", "HEX", null, "D009B5272525F4AD1FE60DC25CEBFA7C")]
    [InlineData("MD5", Message, "UTF32", "HEX", null, "069DB8C48B6DE492AFA14ED35A0D0FF4")]
    [InlineData("MD5", Message, "ASCII", "HEX", null, "A9E48942C3E8C07EB2588E6E6A97A313")]
    [InlineData("MD5", Message, "UNICODE", "HEX", null, "48238465D93D2B6A9CEA729928A8B62C")]
    public void HashTests(string algorithm, string message, string encoding, string format, string? secret, string expected)
    {
        IResult result = HashHandler.Hash(new HashApiRequest(algorithm, message, encoding, format, secret));
        Ok<TextApiResponse> response = Assert.IsType<Ok<TextApiResponse>>(result);
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response);
        Assert.NotNull(response.Value);
        Assert.Equal(expected, response.Value.Response);
    }
}