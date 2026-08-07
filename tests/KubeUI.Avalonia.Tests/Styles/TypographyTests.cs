using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Headless.XUnit;
using KubeUI.Avalonia.Options;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Styles;

public sealed class TypographyTests
{
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
        Application.Current.TryGetResource("KubeUITitleFontSize", ThemeVariant.Default, out var titleFontSize);
        Application.Current.TryGetResource("KubeUIMetadataFontSize", ThemeVariant.Default, out var metadataFontSize);

        appFontFamily.ShouldBeOfType<FontFamily>().Name.ShouldBe("Inter");
        titleFontSize.ShouldBe(24d);
        metadataFontSize.ShouldBe(11d);
    }

    [Fact]
    public void Console_font_size_defaults_to_shared_code_size()
    {
        new AppearanceSettings().ConsoleFontSize.ShouldBe(12m);
    }
}
