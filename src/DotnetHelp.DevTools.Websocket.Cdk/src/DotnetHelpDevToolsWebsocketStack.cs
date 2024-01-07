using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AwsApigatewayv2Integrations;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Constructs;

namespace DotnetHelp.DevTools.Websocket.Cdk;

public class DotnetHelpDevToolsWebsocketStack : Stack
{
    internal DotnetHelpDevToolsWebsocketStack(Construct scope, string id, IStackProps props = null) : base(scope, id,
        props)
    {
        var connectionTable = new Table(this, "WebsocketConnectionsTable", new TableProps()
        {
            PartitionKey = new Attribute() { Name = "connectionId", Type = AttributeType.STRING },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY,
            Encryption = TableEncryption.AWS_MANAGED,
            PointInTimeRecovery = false
        });
        
        var websocketFunction = new Function(this, "WebSocketFunction", new FunctionProps
        {
            Architecture = Architecture.X86_64,
            Runtime = Runtime.PROVIDED_AL2023,
            MemorySize = 256,
            Description = "DotnetHelp.DevTools.Websocket",
            Handler = "bootstrap",
            Code = Code.FromAsset("../web-socket/"),
            Timeout = Duration.Seconds(20),
        });

        connectionTable.GrantReadWriteData(websocketFunction);
        
        var api = new WebSocketApi(this, "WebSocketApi", new WebSocketApiProps
        {
            ApiName = "DotnetHelp.DevTools.Websocket",
            Description = "DotnetHelp.DevTools.Websocket",
            RouteSelectionExpression = "$request.body.action",
            ConnectRouteOptions = new WebSocketRouteOptions
            {
                Integration = new WebSocketLambdaIntegration("ConnectIntegration", websocketFunction),
            },
            DisconnectRouteOptions = new WebSocketRouteOptions
            {
                Integration = new WebSocketLambdaIntegration("DisconnectIntegration", websocketFunction),
            },
            DefaultRouteOptions = new WebSocketRouteOptions
            {
                Integration = new WebSocketLambdaIntegration("DefaultIntegration", websocketFunction),
            }
        });
        
        var stage = new WebSocketStage(this, "WebSocketApiStage", new WebSocketStageProps
        {
            StageName = "wss",
            AutoDeploy = true,
            WebSocketApi = api
        });
        
        api.GrantManageConnections(websocketFunction);
        
        new CfnOutput(this, "WSS_URL", new CfnOutputProps
        {
            Value = stage.Url,
            ExportName = "DOTNETHELP:DEVTOOLS:WSS:URL",
        });
    }
}