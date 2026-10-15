using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Declarative;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Styles;

public sealed class TypographyTests
{
    [AvaloniaFact]
    public void context_menu_items_follow_the_app_font_size()
    {
        var target = new Border();
        var item = new MenuItem().Header("Resource action");
        var contextMenu = new ContextMenu().Items(item);
        target.ContextMenu = contextMenu;
        var window = new Window().Content(target);
        var settings = Application.Current.GetRequiredTestService<ISettingsService>();
        var originalFontSize = settings.Appearance.FontSize;
        settings.Appearance.FontSize = 13m;
        settings.ApplySettings();
        try
        {
            window.Show();
            contextMenu.Open(target);
            Dispatcher.UIThread.RunJobs();

            contextMenu.IsOpen.ShouldBeTrue();
            item.FontSize.ShouldBe(13d);
            item.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == "Resource action")
                .FontSize.ShouldBe(13d);

            settings.Appearance.FontSize = 18m;
            settings.ApplySettings();
            Dispatcher.UIThread.RunJobs();
            item.FontSize.ShouldBe(18d);
            item.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == "Resource action")
                .FontSize.ShouldBe(18d);
        }
        finally
        {
            window.Close();
            settings.Appearance.FontSize = originalFontSize;
            settings.ApplySettings();
        }
    }

    [AvaloniaFact]
    public void Code_typography_resources_define_shared_font_and_size()
    {
        Application.Current.TryGetResource("KubeUICodeFontFamily", ThemeVariant.Default, out var fontFamily);
        Application.Current.TryGetResource("KubeUICodeFontSize", ThemeVariant.Default, out var fontSize);

        fontFamily.ShouldBeOfType<FontFamily>().Name.ShouldBe("Cascadia Mono");
        fontSize.ShouldBe(12d);
    }

    [AvaloniaFact]
    public void Application_typography_resources_define_shared_ui_roles()
    {
        Application.Current.TryGetResource("KubeUIAppFontFamily", ThemeVariant.Default, out var appFontFamily);
        Application.Current.TryGetResource("KubeUIAppFontSize", ThemeVariant.Default, out var appFontSize);
        Application.Current.TryGetResource("KubeUITitleFontSize", ThemeVariant.Default, out var titleFontSize);
        Application.Current.TryGetResource("KubeUIMetadataFontSize", ThemeVariant.Default, out var metadataFontSize);

        appFontFamily.ShouldBeOfType<FontFamily>().Name.ShouldBe("Inter");
        appFontSize.ShouldBe(13d);
        titleFontSize.ShouldBe(24d);
        metadataFontSize.ShouldBe(11d);
    }

    [Fact]
    public void Console_font_size_defaults_to_shared_code_size()
    {
        new AppearanceSettings().ConsoleFontSize.ShouldBe(12m);
    }
}
