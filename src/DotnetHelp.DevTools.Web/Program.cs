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

builder.Services
	.AddHttpClient<AlgoliaSearchClient>(client =>
	{
		client.BaseAddress = new Uri("https://0LIDZMWP6Y-dsn.algolia.net");
		client.DefaultRequestHeaders.Add("X-Algolia-Application-Id", "0LIDZMWP6Y");
		client.DefaultRequestHeaders.Add("X-Algolia-API-Key", "3146a10f247fb1b7669ffd154df4bce4");
	});
		

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