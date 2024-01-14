using System.Text.Json.Serialization;

namespace DotnetHelp.DevTools.Web.Application;

[JsonSerializable(typeof(TextApiResponse))]
[JsonSerializable(typeof(TextApiRequest))]
[JsonSerializable(typeof(HmacApiRequest))]
[JsonSerializable(typeof(JwtApiRequest))]
[JsonSerializable(typeof(JwtApiResponse))]
[JsonSerializable(typeof(IReadOnlyCollection<BucketHttpRequest>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}