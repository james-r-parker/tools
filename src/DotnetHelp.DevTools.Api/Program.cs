using DotnetHelp.DevTools.Api.Handlers.Base64;
using DotnetHelp.DevTools.Api.Handlers.Hmac;
using DotnetHelp.DevTools.Api.Handlers.Http;
using DotnetHelp.DevTools.Api.Handlers.Jwt;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Logging
    .ClearProviders()
    .AddJsonConsole();

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

builder.Services.AddHealthChecks();

builder.Services.AddHttpClient<JwtKeyHttpClient>()
    .ConfigureHttpClient((sp, client) =>
    {
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(10);
        client.DefaultRequestVersion = HttpVersion.Version11;
        client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DotnetHelp.DevTools.Api", "1.0"));
    });

builder.Services
    .AddHttpContextAccessor()
    .AddSingleton<IHttpRequestRepository, HttpRequestRepository>()
    .AddSingleton<IWebsocketClient, WebsocketClient>()
    .AddSingleton<IDistributedCache, DynamoDbDistributedCache>()
    .AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>()
    .AddSingleton<IAmazonApiGatewayManagementApi>(new AmazonApiGatewayManagementApiClient(
        new AmazonApiGatewayManagementApiConfig
        {
            ServiceURL = Constants.WebsocketUrl
        }));

var app = builder.Build();

app.UseHealthChecks("/_health");

var api = app.MapGroup("/api");

api.MapPost("/hmac", HmacHandler.Hash);
api.MapPost("/base64/encode", Base64Handler.Encode);
api.MapPost("/jwt/decode", JwtHandler.Decode);
api.MapPost("/http/{bucket}", HttpRequestHandler.New);
api.MapGet("/http/{bucket}", HttpRequestHandler.List);

api.MapFallback(ctx =>
{
    if (ctx.Request.Method == "OPTIONS")
    {
        ctx.Response.StatusCode = 200;
        return Task.CompletedTask;
    }

    ctx.Response.StatusCode = 404;
    return Task.CompletedTask;
});

app.Run();