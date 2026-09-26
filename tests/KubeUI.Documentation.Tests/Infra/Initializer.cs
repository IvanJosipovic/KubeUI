using Avalonia;
using Avalonia.Headless;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Tests.Infra;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: AvaloniaTestApplication(typeof(KubeUI.Documentation.Tests.Infra.DocumentationTestAppBuilder))]

namespace KubeUI.Documentation.Tests.Infra;

public static class DocumentationTestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApp>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions
        {
            UseHeadlessDrawing = false,
        })
        .ConfigureFonts(fontManager => fontManager.AddFontCollection(new CascadiaMonoFontCollection()))
        .WithInterFont()
        .UseSkia();
}
