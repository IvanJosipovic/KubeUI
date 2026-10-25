using Avalonia;
using Avalonia.Headless;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: AvaloniaTestApplication(typeof(KubeUI.Documentation.Tests.Infra.DocumentationTestAppBuilder))]

namespace KubeUI.Documentation.Tests.Infra;

public static class DocumentationTestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<DocumentationTestApp>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions
        {
            UseHeadlessDrawing = false,
        })
        .ConfigureFonts(fontManager => fontManager.AddFontCollection(new CascadiaMonoFontCollection()))
        .WithInterFont()
        .UseSkia();
}

public sealed class DocumentationTestApp : TestApp
{
    public DocumentationTestApp()
        : base(ConfigureServices)
    {
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        HelmWalkthroughService.ConfigureHelmRepositoryPaths();
        services.AddSingleton<HelmService>();
        services.Replace(ServiceDescriptor.Singleton(provider =>
            HelmWalkthroughService.Create(provider.GetRequiredService<HelmService>())));
    }
}
