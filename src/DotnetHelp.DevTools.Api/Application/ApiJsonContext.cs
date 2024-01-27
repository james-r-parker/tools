using System.Collections.Immutable;
using Amazon.Lambda.APIGatewayEvents;

namespace DotnetHelp.DevTools.Api.Application;

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(TextApiResponse))]
[JsonSerializable(typeof(TextCollectionApiResponse))]
[JsonSerializable(typeof(TextApiRequest))]
[JsonSerializable(typeof(HashApiRequest))]
[JsonSerializable(typeof(JwtApiRequest))]
[JsonSerializable(typeof(JwtApiResponse))]
[JsonSerializable(typeof(IReadOnlyCollection<BucketHttpRequest>))]
[JsonSerializable(typeof(IReadOnlyCollection<IncomingEmail>))]
[JsonSerializable(typeof(OutgoingHttpRequest))]
[JsonSerializable(typeof(OutgoingHttpResponse))]
[JsonSerializable(typeof(DnsLookupResponses))]
[JsonSerializable(typeof(NewHttpMock))]
[JsonSerializable(typeof(HttpMock))]
[JsonSerializable(typeof(IReadOnlyCollection<HttpMock>))]
internal partial class ApiJsonContext : JsonSerializerContext
{
}