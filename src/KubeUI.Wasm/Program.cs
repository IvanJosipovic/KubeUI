using BlazorStrap;
using k8s;
using KubeUI.Core;
using KubeUI.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace KubeUI.Wasm
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            
            builder.RootComponents.Add<App>("app");

            builder.Services.AddLogging(builder => builder
#if DEBUG
                    .SetMinimumLevel(LogLevel.Trace)
#else
                    .SetMinimumLevel(LogLevel.Warning)
#endif
            );

            builder.Services.AddSingleton<IState, State>();

            builder.Services.AddScoped<IAppInsights, AppInsights>();
            
            builder.Services.AddSingleton<Updater>();

            builder.Services.AddBootstrapCss();

            await builder.Build().RunAsync();
        }
    }
}
