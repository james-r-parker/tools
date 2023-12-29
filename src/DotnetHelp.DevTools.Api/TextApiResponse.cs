internal record TextApiResponse(string Response)
{
}

[JsonSerializable(typeof(TextApiResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}