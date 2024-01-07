using Amazon.CDK;

namespace DotnetHelp.DevTools.Websocket.Cdk;

sealed class Program
{
    public static void Main(string[] args)
    {
        var app = new App();
        new DotnetHelpDevToolsWebsocketStack(app, "DotnetHelpDevToolsWebsocketStack", new StackProps());
        app.Synth();
    }
}