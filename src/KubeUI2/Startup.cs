using k8s;
using KubeUI.Services;
using KubeUI2;
using Microsoft.AspNetCore.Blazor.Http;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FluentValidation;
using KubeUI.Validators;

namespace KubeUI2
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

            services.AddTransient<WebAssemblyHttpMessageHandler>();

            var config = new KubernetesClientConfiguration { Host = "http://127.0.0.1:5000" };
            services.AddSingleton(config);

            // Setup the http client
            services.AddHttpClient("K8s")
                .ConfigurePrimaryHttpMessageHandler<WebAssemblyHttpMessageHandler>()
                .AddTypedClient<IKubernetes>((httpClient, serviceProvider) => new Kubernetes(serviceProvider.GetRequiredService<KubernetesClientConfiguration>(), httpClient));

            var cfg = new FluentValidationMvcConfiguration();
            cfg.RegisterValidatorsFromAssemblyContaining<Startup>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
