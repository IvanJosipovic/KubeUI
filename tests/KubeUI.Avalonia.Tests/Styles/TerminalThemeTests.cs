using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using KubeUI.Avalonia.Styles;
using Shouldly;
using SvcSystems.UI.Terminal;

namespace KubeUI.Avalonia.Tests.Styles;

public sealed class TerminalThemeTests
{
    [AvaloniaFact]
    public void terminal_theme_provides_all_256_xterm_colors()
    {
        var theme = new TerminalTheme();

        theme.Resources.Count.ShouldBe(256);
        GetColor(theme.Resources, 0).ShouldBe(Color.Parse("#000000"));
        GetColor(theme.Resources, 1).ShouldBe(Color.Parse("#800000"));
        GetColor(theme.Resources, 15).ShouldBe(Color.Parse("#FFFFFF"));
        GetColor(theme.Resources, 16).ShouldBe(Color.Parse("#000000"));
        GetColor(theme.Resources, 17).ShouldBe(Color.Parse("#00005F"));
        GetColor(theme.Resources, 231).ShouldBe(Color.Parse("#FFFFFF"));
        GetColor(theme.Resources, 232).ShouldBe(Color.Parse("#080808"));
        GetColor(theme.Resources, 255).ShouldBe(Color.Parse("#EEEEEE"));
    }

    [AvaloniaFact]
    public void fluent_styles_register_terminal_colors_without_a_xaml_include()
    {
        var application = Application.Current!;
        var styles = new Fluent();

        application.Styles.Add(styles);
        try
        {
            application.TryGetResource("SvcSystems.UI.TerminalColor255", ThemeVariant.Default, out var value).ShouldBeTrue();
            value.ShouldBeOfType<SolidColorBrush>().Color.ShouldBe(Color.Parse("#EEEEEE"));
        }
        finally
        {
            application.Styles.Remove(styles);
        }
    }

    private static Color GetColor(IResourceDictionary resources, int index)
    {
        return resources[$"SvcSystems.UI.TerminalColor{index}"].ShouldBeOfType<SolidColorBrush>().Color;
    }
}
