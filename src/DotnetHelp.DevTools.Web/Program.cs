using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DotnetHelp.DevTools.Web;
using WebSocket = DotnetHelp.DevTools.Web.Application.WebSocket;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
	.AddBlazoredLocalStorage();

builder.Services
	.AddHttpClient<ApiHttpClient>(client =>
		client.BaseAddress = new Uri("https://www.dotnethelp.co.uk"));

builder.Services.Configure<WebSocketOptions>((o) =>
{
	o.BaseAddress = "wss://wss.dotnethelp.co.uk";
});

builder.Services.Configure<ApplicationOptions>((o) =>
{
	o.BaseAddress = "https://www.dotnethelp.co.uk";
	o.EmailBaseAddress = "tools.dotnethelp.co.uk";
});

builder.Services
	.AddScoped<IStateManagement, StateManagement>()
	.AddTransient<IWebSocket, WebSocket>();

await builder.Build().RunAsync();