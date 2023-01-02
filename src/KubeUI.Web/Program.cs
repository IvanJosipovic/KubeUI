using KubeUI.UI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

ConfigureServices.Configure(builder.Configuration, builder.Services);
KubeUI.Core.Client.ConfigureServices.Configure(builder.Configuration, builder.Services);
builder.Services.AddHttpClient(string.Empty, client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
