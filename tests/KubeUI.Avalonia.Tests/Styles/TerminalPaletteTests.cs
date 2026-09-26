using Avalonia.Headless.XUnit;
using Avalonia.Controls;
using Avalonia.Media;
using KubeUI.Avalonia.Styles;
using Avalonia.Styling;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Styles;

public sealed class TerminalPaletteTests
{
    [AvaloniaFact]
    public void terminal_palette_provides_all_256_xterm_colors()
    {
        var resources = TerminalPalette.CreateResources();

        resources.Count.ShouldBe(256);
        GetColor(resources, 0).ShouldBe(Color.Parse("#000000"));
        GetColor(resources, 1).ShouldBe(Color.Parse("#800000"));
        GetColor(resources, 15).ShouldBe(Color.Parse("#FFFFFF"));
        GetColor(resources, 16).ShouldBe(Color.Parse("#000000"));
        GetColor(resources, 17).ShouldBe(Color.Parse("#00005F"));
        GetColor(resources, 231).ShouldBe(Color.Parse("#FFFFFF"));
        GetColor(resources, 232).ShouldBe(Color.Parse("#080808"));
        GetColor(resources, 255).ShouldBe(Color.Parse("#EEEEEE"));
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

    private static Color GetColor(ResourceDictionary resources, int index)
    {
        return resources[$"SvcSystems.UI.TerminalColor{index}"].ShouldBeOfType<SolidColorBrush>().Color;
    }
}
