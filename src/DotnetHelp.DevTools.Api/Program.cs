using DotnetHelp.DevTools.Api.Handlers.Base64;
using DotnetHelp.DevTools.Api.Handlers.Hmac;
using DotnetHelp.DevTools.Api.Handlers.Http;
using DotnetHelp.DevTools.Api.Handlers.Jwt;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Logging
    .ClearProviders()
    .AddJsonConsole();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 3;
});

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi,
    (c) =>
    {
        c.Serializer =
            new Amazon.Lambda.Serialization.SystemTextJson.SourceGeneratorLambdaJsonSerializer<
                AppJsonSerializerContext>();
    });

builder.Services.AddHealthChecks();

builder.Services.AddCors(c => c.AddDefaultPolicy(p =>
    p.AllowCredentials().AllowAnyHeader().AllowAnyMethod()
        .WithOrigins("http://localhost:5250", "https://www.dothethelp.co.uk")));

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

WebApplication app = builder.Build();

app
    .UseForwardedHeaders()    
    .UseCors()
    .UseHealthChecks("/_health");

RouteGroupBuilder api = app.MapGroup("/api");

api.MapPost("/hmac", HmacHandler.Hash);
api.MapPost("/base64/encode", Base64Handler.Encode);
api.MapPost("/jwt/decode", JwtHandler.Decode);
api.MapPost("/http/{bucket}", HttpRequestHandler.New);
api.MapGet("/http/{bucket}", HttpRequestHandler.List);

app.Run();