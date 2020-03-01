using k8s;
using KubeUI.Core;
using KubeUI.Services;
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http;
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

            await builder.Build().RunAsync();
        }
    }
}
