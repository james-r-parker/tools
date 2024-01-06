namespace DotnetHelp.DevTools.Api.PreTraffic;

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}