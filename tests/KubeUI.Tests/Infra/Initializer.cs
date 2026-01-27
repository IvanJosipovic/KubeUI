using Avalonia;
using Avalonia.Headless;
using KubeUI;
using KubeUI.Tests.Infra;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

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
