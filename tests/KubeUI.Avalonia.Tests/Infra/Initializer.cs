using Avalonia.Headless;
using KubeUI.Avalonia.Styles;
using KubeUI.Avalonia.Tests.Infra;

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
        .UseSkia()
        .AfterSetup(_ =>
        {
            var app = (TestApp)Application.Current!;
            ApplicationThemeStyles.AddTo(app.Styles);
            app.InitializeServices();
        });
}

