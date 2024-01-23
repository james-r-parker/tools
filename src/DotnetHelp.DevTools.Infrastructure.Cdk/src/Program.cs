using Amazon.CDK;

namespace DotnetHelp.DevTools.Infrastructure.Cdk;

sealed class Program
{
    public static void Main(string[] args)
    {
        var app = new App();
        new DotnetHelpDevToolsInfrastructureStack(app, "DotnetHelpDevToolsInfrastructureStack", new Props());
        app.Synth();
    }
}