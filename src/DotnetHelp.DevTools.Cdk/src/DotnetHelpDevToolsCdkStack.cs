using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.CodeDeploy;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Constructs;

namespace DotnetHelp.DevTools.Cdk;

public class DotnetHelpDevToolsStack : Stack
{
    internal DotnetHelpDevToolsStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var apiFunction = new Function(this, "API", new FunctionProps
        {
            Architecture = Architecture.X86_64,
            Runtime = Runtime.PROVIDED_AL2023,
            MemorySize = 256,
            Description = "DotnetHelp.DevTools.API",
            Handler = "bootstrap",
            Code = Code.FromAsset("../app/"),
            Timeout = Duration.Seconds(20),
        });

        var preTafficRole = new Role(this, "PreTrafficRole", new RoleProps
        {
            AssumedBy = new ServicePrincipal("codedeploy.amazonaws.com"),
            ManagedPolicies = new IManagedPolicy[]
            {
                ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSCodeDeployRoleForLambda"),
                ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaRole"),
                ManagedPolicy.FromAwsManagedPolicyName("AWSCodeDeployFullAccess")
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
                { "API_FUNCTION_VERSION", apiFunction.CurrentVersion.Version }
            },
            Role = preTafficRole
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
            InvokeMode = InvokeMode.RESPONSE_STREAM,
        });

        var group = new LambdaDeploymentGroup(this, "DeploymentGroup", new LambdaDeploymentGroupProps()
        {
            Alias = stage,
            DeploymentConfig = LambdaDeploymentConfig.ALL_AT_ONCE,
            DeploymentGroupName = "DotnetHelp.DevTools.API",
        });

        new CfnOutput(this, "API_URL", new CfnOutputProps
        {
            Value = url.Url
        });
    }
}