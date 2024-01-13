using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.CodeDeploy;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Constructs;

namespace DotnetHelp.DevTools.Api.Cdk;

public class DotnetHelpDevToolsApiStack : Stack
{
    internal DotnetHelpDevToolsApiStack(Construct scope, string id, Props props) : base(scope, id, props)
    {
        var httpRequestTable = new Table(this, "APIHttpRequestTable", new TableProps()
        {
            PartitionKey = new Attribute() { Name = "bucket", Type = AttributeType.STRING },
            SortKey = new Attribute() { Name = "created", Type = AttributeType.NUMBER },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY,
            Encryption = TableEncryption.AWS_MANAGED,
            PointInTimeRecovery = false,
            TimeToLiveAttribute = "ttl",
        });
        
        var cacheTable = new Table(this, "APICacheTable", new TableProps()
        {
            PartitionKey = new Attribute() { Name = "id", Type = AttributeType.STRING },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY,
            Encryption = TableEncryption.AWS_MANAGED,
            PointInTimeRecovery = false,
            TimeToLiveAttribute = "ttl",
        });

        var connectionTable =
            Table.FromTableName(this, "WSS_CONNECTION_TABLE", Fn.ImportValue("DOTNETHELP:DEVTOOLS:WSS:TABLE"));

        var apiRole = new Role(this, "ApiRole", new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
            ManagedPolicies = new IManagedPolicy[]
            {
                ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole"),
                ManagedPolicy.FromManagedPolicyName(this, "WSS_DB_POLICY",Fn.ImportValue("DOTNETHELP:DEVTOOLS:WSS:DB:POLICY")),
                ManagedPolicy.FromManagedPolicyName(this, "WSS_API_POLICY",Fn.ImportValue("DOTNETHELP:DEVTOOLS:WSS:API:POLICY"))
            },
        });

        var apiFunction = new Function(this, "API", new FunctionProps
        {
            Architecture = Architecture.X86_64,
            Runtime = Runtime.PROVIDED_AL2023,
            MemorySize = 512,
            Description = "DotnetHelp.DevTools.API",
            Handler = "bootstrap",
            Code = Code.FromAsset("../app/"),
            Timeout = Duration.Seconds(20),
            Role = apiRole,
            Environment = new Dictionary<string, string>()
            {
                { "CONNECTION_TABLE_NAME", connectionTable.TableName },
                { "HTTP_REQUEST_TABLE_NAME", httpRequestTable.TableName },
                { "CACHE_TABLE_NAME", cacheTable.TableName },
                { "WEBSOCKET_URL", Fn.ImportValue("DOTNETHELP:DEVTOOLS:WSS:URL") },
                { "AWS_STS_REGIONAL_ENDPOINTS", "regional" }
            }
        });

        httpRequestTable.GrantReadWriteData(apiRole);
        cacheTable.GrantReadWriteData(apiRole);

        var preTrafficRole = new Role(this, "PreTrafficRole", new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
            ManagedPolicies = new IManagedPolicy[]
            {
                ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaRole"),
                ManagedPolicy.FromAwsManagedPolicyName("AWSCodeDeployFullAccess"),
                ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole")
            }
        });

        var apiPreTrafficFunction = new Function(this, "API-PreTraffic", new FunctionProps
        {
            Architecture = Architecture.X86_64,
            Runtime = Runtime.PROVIDED_AL2023,
            MemorySize = 256,
            Description = "DotnetHelp.DevTools.API.PreTraffic",
            Handler = "bootstrap",
            Code = Code.FromAsset("../pre-traffic/"),
            Timeout = Duration.Seconds(60),
            Environment = new Dictionary<string, string>()
            {
                { "API_FUNCTION_NAME", apiFunction.FunctionName },
                { "API_FUNCTION_VERSION", apiFunction.CurrentVersion.Version },
                { "AWS_STS_REGIONAL_ENDPOINTS", "regional" }
            },
            Role = preTrafficRole
        });

        var stage = new Alias(this, "Stage", new AliasProps()
        {
            AliasName = "live",
            Version = apiFunction.CurrentVersion
        });

        var url = new FunctionUrl(this, "URL", new FunctionUrlProps
        {
            Function = stage,
            AuthType = FunctionUrlAuthType.NONE,
            InvokeMode = InvokeMode.BUFFERED,
        });

        var group = new LambdaDeploymentGroup(this, "DeploymentGroup", new LambdaDeploymentGroupProps()
        {
            Alias = stage,
            DeploymentConfig = LambdaDeploymentConfig.ALL_AT_ONCE,
            DeploymentGroupName = "DotnetHelp.DevTools.API",
            PreHook = apiPreTrafficFunction
        });

        new CfnOutput(this, "API_URL", new CfnOutputProps
        {
            Value = url.Url,
            ExportName = "DOTNETHELP:DEVTOOLS:API:URL",
        });
    }
}