using System.Collections.Immutable;
using Amazon.Lambda.APIGatewayEvents;

namespace DotnetHelp.DevTools.Api.Application;

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(TextApiResponse))]
[JsonSerializable(typeof(TextCollectionApiResponse))]
[JsonSerializable(typeof(TextApiRequest))]
[JsonSerializable(typeof(HmacApiRequest))]
[JsonSerializable(typeof(JwtApiRequest))]
[JsonSerializable(typeof(JwtApiResponse))]
[JsonSerializable(typeof(IReadOnlyCollection<BucketHttpRequest>))]
[JsonSerializable(typeof(OutgoingHttpRequest))]
[JsonSerializable(typeof(OutgoingHttpResponse))]
internal partial class ApiJsonContext : JsonSerializerContext
{
}