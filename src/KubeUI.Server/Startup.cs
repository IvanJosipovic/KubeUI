using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using k8s;
using KubeUI.Validators;
using KubeUI.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Reflection;

namespace KubeUI.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
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

            var config = new KubernetesClientConfiguration { Host = "http://127.0.0.1:5000" };
            services.AddSingleton(config);

            // Setup the http client
            services.AddHttpClient("K8s")
                .AddTypedClient<IKubernetes>((httpClient, serviceProvider) => new Kubernetes(serviceProvider.GetRequiredService<KubernetesClientConfiguration>(), httpClient));

            var cfg = new FluentValidationMvcConfiguration();
            cfg.RegisterValidatorsFromAssemblyContaining<Startup>();


            services.AddRazorPages();
            services.AddServerSideBlazor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
