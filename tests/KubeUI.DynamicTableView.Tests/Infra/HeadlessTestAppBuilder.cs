using Avalonia;
using Avalonia.Headless;

namespace KubeUI.DynamicTableView.Tests.Infra;

public sealed class HeadlessTestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<HeadlessTestApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
