using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DotnetHelp.DevTools.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddBlazoredLocalStorageAsSingleton();

builder.Services
    .AddHttpClient<ApiHttpClient>(client => 
        client.BaseAddress = new Uri("https://www.dotnethelp.co.uk"));

await builder.Build().RunAsync();