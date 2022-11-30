using KristofferStrube.Blazor.FileSystemAccess;
using KubernetesCRDModelGen;
using KubeUI.Core.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MudBlazor.Services;
using System.Runtime.InteropServices;

namespace KubeUI.Core.Client;

public static class ConfigureServices
{
    public static void Configure(IConfiguration configuration, IServiceCollection services)
    {
        services.TryAddScoped<IErrorBoundaryLogger, ErrorBoundaryLogger>();
        services.AddMudServices();
        services.AddHttpClient();
        services.TryAddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient());

        services.AddSingleton<ClusterManager>();
        services.AddSingleton<ICRDGenerator, CRDGenerator>();

        services.AddSingleton<Updater>();

        services.AddFileSystemAccessService();
    }
}
