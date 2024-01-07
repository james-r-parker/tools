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
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IAmazonApiGatewayManagementApi>(new AmazonApiGatewayManagementApiClient(
    new AmazonApiGatewayManagementApiConfig
    {
        ServiceURL = Constants.WebsocketUrl
    }));

builder.Services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient());

var app = builder.Build();

app.MapGet("/health", () => "OK");

var api = app.MapGroup("/api");

api.MapPost("/hmac", HmacHandler.Hash);
api.MapPost("/base64/encode", Base64Handler.Encode);
api.MapPost("/jwt/decode", JwtHandler.Decode);
api.MapPost("/http/{bucket}", HttpRequestHandler.New);

app.Run();