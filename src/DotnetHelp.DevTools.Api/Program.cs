using DnsClient;
using DotnetHelp.DevTools;
using DotnetHelp.DevTools.Api.Handlers.Dns;
using DotnetHelp.DevTools.Api.Handlers.Email;
using DotnetHelp.DevTools.Api.Handlers.Hash;
using DotnetHelp.DevTools.Api.Handlers.Http;
using DotnetHelp.DevTools.Api.Handlers.Jwt;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Logging
    .ClearProviders()
    .AddJsonConsole();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, ApiJsonContext.Default);
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
                ApiJsonContext>();
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
        client.DefaultRequestHeaders.UserAgent.Add(Constants.UserAgent);
    });

builder.Services.AddHttpClient<OutgoingHttpClient>()
    .ConfigureHttpClient((sp, client) =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
        client.DefaultRequestVersion = HttpVersion.Version11;
        client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        client.DefaultRequestHeaders.UserAgent.Add(Constants.UserAgent);
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(20))
    .AddPolicyHandler((sp, request) =>
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg =>
                msg.StatusCode is
                    HttpStatusCode.ServiceUnavailable or
                    HttpStatusCode.BadGateway or
                    HttpStatusCode.GatewayTimeout)
            .RetryAsync(1);
    });

builder.Services.AddSingleton<ILookupClient, LookupClient>();

builder.Services.TryAddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();

builder.Services
    .AddHttpContextAccessor()
    .AddSingleton<IHttpRequestRepository, HttpRequestRepository>()
    .AddSingleton<IEmailRepository, EmailRepository>();

builder.Services
    .AddDotnetHelpWebsocketClient()
    .AddDotnetHelpCache();

WebApplication app = builder.Build();

app
    .UseForwardedHeaders()
    .UseCors()
    .UseHealthChecks("/_health");

RouteGroupBuilder api = app.MapGroup("/api");

api.MapPost("/hash", HashHandler.Hash);

api.MapPost("/jwt/decode", JwtHandler.Decode);

api.MapPost("/http/{bucket}", HttpRequestHandler.New);
api.MapGet("/http/{bucket}", HttpRequestHandler.List);
api.MapPost("/http", HttpRequestHandler.Send);

api.MapGet("/dns/{domain}", DnsHandler.Lookup);

api.MapGet("/email/{bucket}", EmailRequestHandler.List);

app.Run();