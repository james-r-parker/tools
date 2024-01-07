using Amazon.CDK;

namespace DotnetHelp.DevTools.Api.Cdk;

sealed class Program
{
    public static void Main(string[] args)
    {
        var app = new App();
        new DotnetHelpDevToolsApiStack(app, "DotnetHelpDevToolsApiStack", new StackProps());
        app.Synth();
    }
}