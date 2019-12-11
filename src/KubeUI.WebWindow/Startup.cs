using k8s;
using KubeUI.Core;
using KubeUI.Services;
using KubeUI.Validators;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KubeUI.WebWindow
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => builder
#if DEBUG
                    .SetMinimumLevel(LogLevel.Information)
#else
                    .SetMinimumLevel(LogLevel.Warning)
#endif
            );

            services.AddSingleton<IState, State>();

            services.AddScoped<IAppInsights, AppInsights>();

            //var config = new KubernetesClientConfiguration { Host = "http://127.0.0.1:8888" };

            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();

            services.AddSingleton(config);

            // Setup the IKubernetes client
            services.AddSingleton<IKubernetes>((serviceProvider) => new Kubernetes(serviceProvider.GetRequiredService<KubernetesClientConfiguration>()));

            var cfg = new FluentValidationMvcConfiguration();
            cfg.RegisterValidatorsFromAssemblyContaining<Startup>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
