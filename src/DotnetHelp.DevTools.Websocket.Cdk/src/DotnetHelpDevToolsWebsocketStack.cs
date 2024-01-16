using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AwsApigatewayv2Integrations;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Constructs;

namespace DotnetHelp.DevTools.Websocket.Cdk;

public class DotnetHelpDevToolsWebsocketStack : Stack
{
    internal DotnetHelpDevToolsWebsocketStack(Construct scope, string id, Props props) : base(scope, id,
        props)
    {
        var connectionTable = new Table(this, "WebsocketConnectionsTable", new TableProps()
        {
            PartitionKey = new Attribute() { Name = "connectionId", Type = AttributeType.STRING },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY,
            Encryption = TableEncryption.AWS_MANAGED,
            PointInTimeRecovery = false,
            TimeToLiveAttribute = "ttl",
        });

        connectionTable.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        {
            IndexName = "ix_bucket_connection",
            PartitionKey = new Attribute { Name = "bucket", Type = AttributeType.STRING, },
            SortKey = new Attribute { Name = "connectionId", Type = AttributeType.STRING, },
            ProjectionType = ProjectionType.KEYS_ONLY,
        });

        var dbPolicy = new ManagedPolicy(this, "DBPolicy", new ManagedPolicyProps());
        connectionTable.GrantReadData(dbPolicy);

        var websocketFunction = new Function(this, "WebSocketFunction", new FunctionProps
        {
            Architecture = Architecture.X86_64,
            Runtime = Runtime.PROVIDED_AL2023,
            MemorySize = 512,
            Description = "DotnetHelp.DevTools.Websocket",
            Handler = "bootstrap",
            Code = Code.FromAsset("../web-socket/"),
            Timeout = Duration.Seconds(20),
            ReservedConcurrentExecutions = 10,
            Environment = new Dictionary<string, string>
            {
                { "CONNECTION_TABLE_NAME", connectionTable.TableName },
                { "AWS_STS_REGIONAL_ENDPOINTS", "regional" }
            }
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

        websocketFunction.AddEnvironment("WEBSOCKET_URL", stage.Url);

        api.GrantManageConnections(websocketFunction);

        var apiPolicy = new ManagedPolicy(this, "APIPolicy", new ManagedPolicyProps());
        api.GrantManageConnections(apiPolicy);

        var domain = new DomainName(this, "WSS_DOMAIN", new DomainNameProps()
        {
            DomainName = props.CustomDomain,
            Certificate = Certificate.FromCertificateArn(this, "WSS_CERTIFICATE", props.CertificateArn),
            SecurityPolicy = SecurityPolicy.TLS_1_2,
        });

        new CfnApiMapping(this, "WSS_DOMAIN_MAPPING", new CfnApiMappingProps()
        {
            DomainName = domain.Name,
            Stage = stage.StageName,
            ApiId = api.ApiId,
        });

        new CfnOutput(this, "WSS_DOMAIN_CNAME", new CfnOutputProps
        {
            Value = domain.RegionalDomainName,
        });

        new CfnOutput(this, "WSS_URL", new CfnOutputProps
        {
            Value = stage.Url,
            ExportName = "DOTNETHELP:DEVTOOLS:WSS:URL",
        });

        new CfnOutput(this, "WSS_TABLE", new CfnOutputProps
        {
            Value = connectionTable.TableName,
            ExportName = "DOTNETHELP:DEVTOOLS:WSS:TABLE",
        });

        new CfnOutput(this, "WSS_ID", new CfnOutputProps
        {
            Value = api.ApiId,
            ExportName = "DOTNETHELP:DEVTOOLS:WSS:API",
        });

        new CfnOutput(this, "WSS_DB_POLICY", new CfnOutputProps
        {
            Value = dbPolicy.ManagedPolicyName,
            ExportName = "DOTNETHELP:DEVTOOLS:WSS:DB:POLICY",
        });

        var cfnOutput = new CfnOutput(this, "WSS_API_POLICY", new CfnOutputProps
        {
            Value = apiPolicy.ManagedPolicyName,
            ExportName = "DOTNETHELP:DEVTOOLS:WSS:API:POLICY",
        });
    }
}