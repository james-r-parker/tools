using Amazon.CDK;

namespace DotnetHelp.DevTools.Email.Cdk;

sealed class Program
{
    public static void Main(string[] args)
    {
        var app = new App();
        new DotnetHelpDevToolsEmailStack(app, "DotnetHelpDevToolsEmailStack", new Props
        {
        });
        app.Synth();
    }
}