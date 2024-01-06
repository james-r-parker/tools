using Amazon.CodeDeploy;
using Amazon.CodeDeploy.Model;

public class Function
{
    private static async Task Main(string[] args)
    {
        Func<CodeDeployPreTafficRequest, ILambdaContext, Task> handler = FunctionHandler;
        await LambdaBootstrapBuilder
            .Create(handler, new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    private static async Task FunctionHandler(CodeDeployPreTafficRequest input, ILambdaContext context)
    {
        var functionName = Environment.GetEnvironmentVariable("API_FUNCTION_NAME");
        var functionVersion = Environment.GetEnvironmentVariable("API_FUNCTION_VERSION");

        using var codeDeploy = new AmazonCodeDeployClient();

        if (string.IsNullOrWhiteSpace(functionName) ||
            string.IsNullOrWhiteSpace(functionVersion))
        {
            var failedRequest = new PutLifecycleEventHookExecutionStatusRequest
            {
                LifecycleEventHookExecutionId = input.LifecycleEventHookExecutionId,
                DeploymentId = input.DeploymentId,
                Status = LifecycleEventStatus.Failed
            };

            await codeDeploy.PutLifecycleEventHookExecutionStatusAsync(failedRequest);
        }

        using var lambda = new AmazonLambdaClient();

        var request = new InvokeRequest
        {
            FunctionName = functionName,
            Qualifier = functionVersion,
            InvocationType = InvocationType.RequestResponse,
            Payload = JsonSerializer.Serialize(new APIGatewayHttpApiV2ProxyRequest
            {
                RawPath = "/health",
                RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext()
                {
                    Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription()
                    {
                        Method = "GET",
                        Path = "/health",
                        SourceIp = "127.0.0.1",
                        UserAgent = "DotnetHelp.DevTools.Api.PreTraffic",
                        Protocol = "HTTP/1.1"
                    }
                }
            }, AppJsonSerializerContext.Default.APIGatewayHttpApiV2ProxyRequest)
        };

        var response = await lambda.InvokeAsync(request);

        var responsePayload =
            JsonSerializer.Deserialize(
                response.Payload,
                AppJsonSerializerContext.Default.APIGatewayHttpApiV2ProxyResponse);

        if (responsePayload?.StatusCode != 200)
        {
            var failedRequest = new PutLifecycleEventHookExecutionStatusRequest
            {
                LifecycleEventHookExecutionId = input.LifecycleEventHookExecutionId,
                DeploymentId = input.DeploymentId,
                Status = LifecycleEventStatus.Failed
            };

            await codeDeploy.PutLifecycleEventHookExecutionStatusAsync(failedRequest);
        }

        var putLifecycleEventHookExecutionStatusRequest = new PutLifecycleEventHookExecutionStatusRequest
        {
            LifecycleEventHookExecutionId = input.LifecycleEventHookExecutionId,
            DeploymentId = input.DeploymentId,
            Status = LifecycleEventStatus.Succeeded
        };

        await codeDeploy.PutLifecycleEventHookExecutionStatusAsync(putLifecycleEventHookExecutionStatusRequest);
    }
}

public record CodeDeployPreTafficRequest(string DeploymentId, string LifecycleEventHookExecutionId);

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}