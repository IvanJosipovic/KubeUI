using KubernetesCRDModelGen;
using KubeUI.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KubeUI.Core.Client;

public static class ConfigureServices
{
    public static void Configure(IConfiguration configuration, IServiceCollection services)
    {
        services.AddHttpClient();
        services.TryAddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient());

        services.AddSingleton<ClusterManager>();
        services.AddSingleton<ICRDGenerator, CRDGenerator>();

        services.AddSingleton<Updater>();
    }
}
