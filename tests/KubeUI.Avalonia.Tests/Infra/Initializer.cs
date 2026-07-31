using Avalonia;
using Avalonia.Headless;
using KubeUI.Avalonia.Tests.Infra;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace KubeUI.Avalonia.Tests.Infra;

[CollectionDefinition("Avalonia")]
public sealed class AvaloniaTestCollectionDefinition
{
}

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

