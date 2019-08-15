using Blazor.FileReader;
using BlazorFileSaver;
using FluentValidation;
using KubeUI.Validators;
using KubeUI.Services;
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
#if DEBUG
                    .SetMinimumLevel(LogLevel.Trace)
#else
                    .SetMinimumLevel(LogLevel.Warning)
#endif
            );

            TypeDescriptorProviderGenerator.AddTypeDescriptorProviders(typeof(Schema.Deployment).Namespace, typeof(SchemaExtentions.Deployment).Namespace);

            services.AddFileReaderService();

            services.AddBlazorFileSaver();

            services.AddSingleton<IState, State>();

            services.AddTransient<IValidatorFactory, ServiceProviderValidatorFactory>();

            var config = new FluentValidationMvcConfiguration();
            config.RegisterValidatorsFromAssemblyContaining<Startup>();

            AssemblyScanner.FindValidatorsInAssemblies(config.AssembliesToRegister).ForEach(pair => {
                services.Add(ServiceDescriptor.Transient(pair.InterfaceType, pair.ValidatorType));
            });

            services.AddSingleton<IAppInsights, AppInsights>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
