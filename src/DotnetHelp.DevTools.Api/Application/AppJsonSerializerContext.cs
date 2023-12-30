namespace DotnetHelp.DevTools.Api.Application;

[JsonSerializable(typeof(TextApiResponse))]
[JsonSerializable(typeof(TextApiRequest))]
[JsonSerializable(typeof(HmacApiRequest))]
[JsonSerializable(typeof(JwtApiRequest))]
[JsonSerializable(typeof(JwtApiResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}