using Avalonia.Headless;
using KubeUI.Avalonia.Styles;
using KubeUI.Avalonia.Tests.Infra;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]
namespace KubeUI.Avalonia.Tests.Infra;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApp>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions()
        {
            UseHeadlessDrawing = false
        })
        .ConfigureFonts(fontManager =>
        {
            fontManager.AddFontCollection(new CascadiaMonoFontCollection());
        })
        .WithInterFont()
        .UseSkia();
}
