using System.Diagnostics;
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
    
    [InlineData("SHA1", Message, "UTF8", "HEX", null, "76BB8AAE622823F6054A857E4E79C68E64E44DC5")]
    [InlineData("SHA1", Message, "UTF32", "HEX", null, "3C5C6DC2263D75DCD9F69E420B61E0EA71DD49A8")]
    [InlineData("SHA1", Message, "ASCII", "HEX", null, "F5BDA7F83E19F9F889683094A44787AB4007D955")]
    [InlineData("SHA1", Message, "UNICODE", "HEX", null, "216159458120EF0B63111E326D1C9EADC6A0C758")]
    
    [InlineData("SHA256", Message, "UTF8", "BASE64", null, "0T1fdi1YiCpaOgak24RC2w/iNIee1fCukU/u+P4WUrk=")]
    [InlineData("SHA256", Message, "UTF32", "BASE64", null, "V4kmS5+AIMkjW5tMlVupEg+707BKOfktkpidI2ToNas=")]
    [InlineData("SHA256", Message, "ASCII", "BASE64", null, "LHrE9URM+0HgkvgYO97MqF95k9e5E5SBiLPUh3hphHo=")]
    [InlineData("SHA256", Message, "UNICODE", "BASE64", null, "/iKIIPHWxH7cTwBMdNTOUS0ebYIG2hwqbrfuS+xVUdE=")]
    
    [InlineData("HMACSHA256", Message, "UTF8", "HEX", Secret, "89C6B7C2275F68DE0682E4DAA47B3F20C8256CD11A5CC2E9D126BFD8FE7BF486")]
    [InlineData("HMACSHA256", Message, "UTF32", "HEX", Secret, "320967E82A476C5FA46B8E577F787C99B3F31A7DF009407A48DF5E03CE0E8FC6")]
    [InlineData("HMACSHA256", Message, "ASCII", "HEX", Secret, "F82750969D924D6942E22EA9DAE05EB557953A01E159FFA7F50864529A6ACB1A")]
    [InlineData("HMACSHA256", Message, "UNICODE", "HEX", Secret, "F2CB4B9A537C0BE243D5BAD3FDFE73424793AE4AAC5B93FB2334ED2E9C1B882D")]
    
    [InlineData("HMACSHA512", Message, "UTF8", "HEX", Secret, "A7EF00031D93703CB552ABFE78A16FA5F34D17DB77DEC82809330823D2DD4F4C028C392986E0A64BA2FE721BB36BAFD6DF5BEA843C626FCBD7F4022B6067DE9E")]
    [InlineData("HMACSHA512", Message, "UTF32", "HEX", Secret, "BEB1997DB6A325E572960A38F7B4290FD4F7809505AD1C81119BDC72D83A5A05DCF87D750A5AB7FC7FFDA98DE949F83A20E55BFEAB2BE300C3E9B79C2BEFA704")]
    [InlineData("HMACSHA512", Message, "ASCII", "HEX", Secret, "5A364062775A086832A954A2D348DFF3B75E3AF8D890FE5D4FD26D871BF7397ABB77F261AA15B3EA75EE1091FE1353D489222B9318C83E08DD2D5D2B26461358")]
    [InlineData("HMACSHA512", Message, "UNICODE", "HEX", Secret, "A779E9FE36B5EC60A8F357A410C37FE17F49186FC4B58FFA7026AC8EAEE45119B3EC1DF9363860403A905E2063E1B05B106D19F8EEBD492CC46FE19865BA62CC")]
    public void HashTests(string algorithm, string message, string encoding, string format, string? secret, string expected)
    {
        IResult result = HashHandler.Hash(new HashApiRequest(algorithm, message, encoding, format, secret));
        Ok<TextApiResponse> response = Assert.IsType<Ok<TextApiResponse>>(result);
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response);
        Assert.NotNull(response.Value);
        
        Debug.WriteLineIf(expected != response.Value.Response, $"Expected: {expected} | Actual: {response.Value.Response}");
        Assert.Equal(expected, response.Value.Response);
    }
}