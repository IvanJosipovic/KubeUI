using Avalonia.Headless;
using Avalonia;
using Avalonia.Themes.Fluent;
using Avalonia.Markup.Xaml.Styling;
using KubeUI.Controls.Styles;
using Semi.Avalonia;
using Ursa.Themes.Semi;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: AvaloniaTestApplication(typeof(KubeUI.Controls.Tests.Infra.MinimalControlsTestApplicationBuilder))]

namespace KubeUI.Controls.Tests.Infra;

public static class MinimalControlsTestApplicationBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<MinimalControlsTestApplication>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
        .WithInterFont()
        .UseSkia();
}

public sealed class MinimalControlsTestApplication : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new StyleInclude(new Uri("avares://KubeUI.Avalonia"))
        {
            Source = new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml")
        });
        Styles.Add(new SemiTheme());
        Styles.Add(new UrsaSemiTheme());
        Styles.Add(new SharedControlStyles());
    }
}
