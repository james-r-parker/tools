var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi,
    (c) =>
    {
        c.Serializer =
            new Amazon.Lambda.Serialization.SystemTextJson.SourceGeneratorLambdaJsonSerializer<
                AppJsonSerializerContext>();
    });
builder.Services.AddHttpClient();

var app = builder.Build();

app.MapGet("/health", () => "OK");

var api = app.MapGroup("/api");

api.MapPost("/hmac", HmacHandler.Hash);
api.MapPost("/base64/encode", Base64Handler.Encode);
api.MapPost("/jwt/decode", JwtHandler.Decode);

app.Run();