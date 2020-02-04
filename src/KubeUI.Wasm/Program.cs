using k8s;
using KubeUI.Core;
using KubeUI.Services;
using Microsoft.AspNetCore.Blazor.Hosting;
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

            var config = new KubernetesClientConfiguration { Host = "http://127.0.0.1:8888" };
            builder.Services.AddSingleton(config);

            // Setup the http client
            builder.Services.AddSingleton<IKubernetes>((serviceProvider) => new Kubernetes(serviceProvider.GetRequiredService<KubernetesClientConfiguration>()));

            await builder.Build().RunAsync();
        }
    }
}
