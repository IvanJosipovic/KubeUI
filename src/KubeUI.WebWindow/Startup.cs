using k8s;
using KubeUI.Core;
using KubeUI.Services;
using KubeUI.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebWindows.Blazor;

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
            
            services.AddSingleton<Updater>();

            var cfg = new FluentValidationMvcConfiguration();
            cfg.RegisterValidatorsFromAssemblyContaining<Startup>();
        }

        public void Configure(DesktopApplicationBuilder app, WebWindows.WebWindow window)
        {
            window.SetIconFile("wwwroot/favicon.ico");

            app.AddComponent<App>("app");
        }
    }
}
