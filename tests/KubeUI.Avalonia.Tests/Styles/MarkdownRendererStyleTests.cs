using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using KubeUI.Avalonia.Styles;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Styles;

public sealed class MarkdownRendererStyleTests
{
    [AvaloniaFact]
    public void creating_a_second_fluent_style_does_not_reuse_palette_resources()
    {
        var application = Application.Current!;
        var styles = new Fluent();

        application.Styles.Add(styles);
        application.Styles.Remove(styles);
    }

    [AvaloniaFact]
    public void markdown_colors_follow_fluent_theme_variants()
    {
        var application = Application.Current!;
        var originalTheme = application.RequestedThemeVariant;

        try
        {
            application.RequestedThemeVariant = ThemeVariant.Light;
            Dispatcher.UIThread.RunJobs();

            GetColor(application, "BorderColor").ShouldBe(Color.Parse("#737373"));
            GetColor(application, "ForegroundColor").ShouldBe(Color.Parse("#000000"));
            GetColor(application, "CardBackgroundColor").ShouldBe(Color.Parse("#EEEEF2"));
            GetColor(application, "SecondaryCardBackgroundColor").ShouldBe(Color.Parse("#CCCCCC"));
            GetColor(application, "CodeInlineColor").ShouldBe(Color.Parse("#5D5D5D"));
            GetColor(application, "QuoteBorderColor").ShouldBe(Color.Parse("#737373"));

            application.RequestedThemeVariant = ThemeVariant.Dark;
            Dispatcher.UIThread.RunJobs();

            GetColor(application, "BorderColor").ShouldBe(Color.Parse("#676767"));
            GetColor(application, "ForegroundColor").ShouldBe(Color.Parse("#FFFFFF"));
            GetColor(application, "CardBackgroundColor").ShouldBe(Color.Parse("#1E1E1E"));
            GetColor(application, "SecondaryCardBackgroundColor").ShouldBe(Color.Parse("#333333"));
            GetColor(application, "CodeInlineColor").ShouldBe(Color.Parse("#B4B4B4"));
            GetColor(application, "QuoteBorderColor").ShouldBe(Color.Parse("#676767"));
        }
        finally
        {
            application.RequestedThemeVariant = originalTheme;
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static Color GetColor(Application application, string key)
    {
        application.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        return value.ShouldBeOfType<Color>();
    }
}
