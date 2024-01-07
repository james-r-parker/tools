using Amazon.CDK;

namespace DotnetHelp.DevTools.Cdk;

sealed class Program
{
    public static void Main(string[] args)
    {
        var app = new App();
        new DotnetHelpDevToolsApiStack(app, "DotnetHelpDevToolsApiStack", new StackProps());
        new DotnetHelpDevToolsWebStack(app, "DotnetHelpDevToolsWebStack", new StackProps());
        app.Synth();
    }
}