using Amazon.CDK;

namespace DotnetHelp.DevTools.Web.Cdk;

sealed class Program
{
    public static void Main(string[] args)
    {
        var app = new App();
        new DotnetHelpDevToolsWebStack(app, "DotnetHelpDevToolsWebStack", new StackProps());
        app.Synth();
    }
}