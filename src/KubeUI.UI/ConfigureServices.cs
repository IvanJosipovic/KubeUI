using KristofferStrube.Blazor.FileSystemAccess;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MudBlazor.Services;

namespace KubeUI.UI;

public static class ConfigureServices
{
    public static void Configure(IConfiguration configuration, IServiceCollection services)
    {
        services.TryAddScoped<IErrorBoundaryLogger, ErrorBoundaryLogger>();
        services.AddMudServices();
        services.AddFileSystemAccessService();
    }
}
