using Amazon.CDK;
using Amazon.CDK.AWS.CodeDeploy;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
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