using k8s;
using KubeUI.Services;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using KubeUI.Validators;
using System.Net.Http;
using System.Reflection;
using System;

namespace KubeUI.Wasm
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => builder
#if DEBUG
                    .SetMinimumLevel(LogLevel.Trace)
#else
                    .SetMinimumLevel(LogLevel.Warning)
#endif
            );

            services.AddSingleton<IState, State>();

            services.AddScoped<IAppInsights, AppInsights>();

            var config = new KubernetesClientConfiguration { Host = "http://127.0.0.1:8888" };
            services.AddSingleton(config);

            // Setup the http client
            services.AddHttpClient("K8s")
                .ConfigurePrimaryHttpMessageHandler( _ =>
                {
                    var wasmHttpMessageHandlerType = Assembly.Load("WebAssembly.Net.Http").GetType("WebAssembly.Net.Http.HttpClient.WasmHttpMessageHandler");
                    return (HttpMessageHandler)Activator.CreateInstance(wasmHttpMessageHandlerType);
                })
                .AddTypedClient<IKubernetes>((httpClient, serviceProvider) => new Kubernetes(serviceProvider.GetRequiredService<KubernetesClientConfiguration>(), httpClient));

            var cfg = new FluentValidationMvcConfiguration();
            cfg.RegisterValidatorsFromAssemblyContaining<Startup>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<Core.App>("app");
        }
    }
}
