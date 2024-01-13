using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DotnetHelp.DevTools.Web;
using DotnetHelp.DevTools.Web.Application;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddHttpClient<ApiHttpClient>(client => 
        client.BaseAddress = new Uri("https://www.dotnethelp.co.uk"));

await builder.Build().RunAsync();