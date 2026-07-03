using System.Globalization;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Fluent;
using FluentAvalonia.Styling;
using Semi.Avalonia;
using Ursa.Themes.Semi;

namespace KubeUI.Avalonia.Styles;

internal static class ApplicationThemeStyles
{
    private static readonly Uri BaseUri = new("avares://KubeUI.Avalonia");
    private static readonly CultureInfo Locale = CultureInfo.GetCultureInfo("en-US");

    public static void AddTo(global::Avalonia.Styling.Styles styles)
    {
        ArgumentNullException.ThrowIfNull(styles);

        styles.Add(new SemiTheme { Locale = Locale });
        styles.Add(new UrsaSemiTheme { Locale = Locale });
        styles.Add(new FluentAvaloniaTheme());
        styles.Add(CreateFluentTheme());
        styles.Add(CreateStyleInclude("avares://Avalonia.Controls.DataGrid/Themes/Fluent.v2.xaml"));
        styles.Add(CreateStyleInclude("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml"));
        styles.Add(new DockFluentTheme());
        styles.Add(CreateStyleInclude("avares://SvcSystems.UI.Terminal/Styles/Colors.axaml"));
        styles.Add(new Fluent());
    }

    private static FluentTheme CreateFluentTheme()
    {
        var theme = new FluentTheme
        {
            DensityStyle = DensityStyle.Compact
        };

        theme.Palettes.Add(ThemeVariant.Light, CreateLightPalette());
        theme.Palettes.Add(ThemeVariant.Dark, CreateDarkPalette());

        return theme;
    }

    private static ColorPaletteResources CreateLightPalette()
    {
        return new ColorPaletteResources
        {
            Accent = Color("#ff0073cf"),
            AltHigh = Color("White"),
            AltLow = Color("White"),
            AltMedium = Color("White"),
            AltMediumHigh = Color("White"),
            AltMediumLow = Color("White"),
            BaseHigh = Color("Black"),
            BaseLow = Color("#ffcccccc"),
            BaseMedium = Color("#ff898989"),
            BaseMediumHigh = Color("#ff5d5d5d"),
            BaseMediumLow = Color("#ff737373"),
            ChromeAltLow = Color("#ff5d5d5d"),
            ChromeBlackHigh = Color("Black"),
            ChromeBlackLow = Color("#ffcccccc"),
            ChromeBlackMedium = Color("#ff5d5d5d"),
            ChromeBlackMediumLow = Color("#ff898989"),
            ChromeDisabledHigh = Color("#ffcccccc"),
            ChromeDisabledLow = Color("#ff898989"),
            ChromeGray = Color("#ff737373"),
            ChromeHigh = Color("#ffcccccc"),
            ChromeLow = Color("#ffececec"),
            ChromeMedium = Color("#ffe6e6e6"),
            ChromeMediumLow = Color("#ffececec"),
            ChromeWhite = Color("White"),
            ListLow = Color("#ffe6e6e6"),
            ListMedium = Color("#ffcccccc"),
            RegionColor = Color("#EEEEF2")
        };
    }

    private static ColorPaletteResources CreateDarkPalette()
    {
        return new ColorPaletteResources
        {
            Accent = Color("#ff0073cf"),
            AltHigh = Color("Black"),
            AltLow = Color("Black"),
            AltMedium = Color("Black"),
            AltMediumHigh = Color("Black"),
            AltMediumLow = Color("Black"),
            BaseHigh = Color("White"),
            BaseLow = Color("#ff333333"),
            BaseMedium = Color("#ff9a9a9a"),
            BaseMediumHigh = Color("#ffb4b4b4"),
            BaseMediumLow = Color("#ff676767"),
            ChromeAltLow = Color("#ffb4b4b4"),
            ChromeBlackHigh = Color("Black"),
            ChromeBlackLow = Color("#ffb4b4b4"),
            ChromeBlackMedium = Color("Black"),
            ChromeBlackMediumLow = Color("Black"),
            ChromeDisabledHigh = Color("#ff333333"),
            ChromeDisabledLow = Color("#ff9a9a9a"),
            ChromeGray = Color("Gray"),
            ChromeHigh = Color("Gray"),
            ChromeLow = Color("#ff151515"),
            ChromeMedium = Color("#ff1d1d1d"),
            ChromeMediumLow = Color("#ff2c2c2c"),
            ChromeWhite = Color("White"),
            ListLow = Color("#ff1d1d1d"),
            ListMedium = Color("#ff333333"),
            RegionColor = Color("#1E1E1E")
        };
    }

    private static StyleInclude CreateStyleInclude(string source)
    {
        return new StyleInclude(BaseUri)
        {
            Source = new Uri(source)
        };
    }

    private static Color Color(string value)
    {
        return global::Avalonia.Media.Color.Parse(value);
    }
}
