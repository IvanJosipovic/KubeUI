using Blazor.Extensions.Logging;
using Blazor.FileReader;
using BlazorFileSaver;
using Cloudcrate.AspNetCore.Blazor.Browser.Storage;
using FluentValidation;
using KubeUI.Schema;
using KubeUI.Validators;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KubeUI
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => builder
                    .AddBrowserConsole()
#if DEBUG
                    .SetMinimumLevel(LogLevel.Trace)
#else
                    .SetMinimumLevel(LogLevel.Warning)
#endif
            );

            services.AddTypeDescriptorProviders(typeof(Schema.Deployment).Namespace, typeof(SchemaExtentions.Deployment).Namespace);

            services.AddFileReaderService();

            services.AddBlazorFileSaver();

            services.AddSingleton<IState, State>();

            services.AddTransient<IValidatorFactory, ServiceProviderValidatorFactory>();

            var config = new FluentValidationMvcConfiguration();
            config.RegisterValidatorsFromAssemblyContaining<Startup>();

            AssemblyScanner.FindValidatorsInAssemblies(config.AssembliesToRegister).ForEach(pair => {
                services.Add(ServiceDescriptor.Transient(pair.InterfaceType, pair.ValidatorType));
            });

            services.AddStorage();

            services.AddSingleton<IAppInsights, AppInsights>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
