using Avalonia.Styling;

namespace KubeUI.Styles;

public class FluentStyles : Avalonia.Styling.Styles
{
    public static ColorPaletteResources LightPalette { get; } = new()
    {
        Accent = Color.Parse("#ff0073cf"),
        AltHigh = Colors.White,
        AltLow = Colors.White,
        AltMedium = Colors.White,
        AltMediumHigh = Colors.White,
        AltMediumLow = Colors.White,
        BaseHigh = Colors.Black,
        BaseLow = Color.Parse("#ffcccccc"),
        BaseMedium = Color.Parse("#ff898989"),
        BaseMediumHigh = Color.Parse("#ff5d5d5d"),
        BaseMediumLow = Color.Parse("#ff737373"),
        ChromeAltLow = Color.Parse("#ff5d5d5d"),
        ChromeBlackHigh = Colors.Black,
        ChromeBlackLow = Color.Parse("#ffcccccc"),
        ChromeBlackMedium = Color.Parse("#ff5d5d5d"),
        ChromeBlackMediumLow = Color.Parse("#ff898989"),
        ChromeDisabledHigh = Color.Parse("#ffcccccc"),
        ChromeDisabledLow = Color.Parse("#ff898989"),
        ChromeGray = Color.Parse("#ff737373"),
        ChromeHigh = Color.Parse("#ffcccccc"),
        ChromeLow = Color.Parse("#ffececec"),
        ChromeMedium = Color.Parse("#ffe6e6e6"),
        ChromeMediumLow = Color.Parse("#ffececec"),
        ChromeWhite = Colors.White,
        ListLow = Color.Parse("#ffe6e6e6"),
        ListMedium = Color.Parse("#ffcccccc"),
        RegionColor = Color.Parse("#EEEEF2")
    };

    public static ColorPaletteResources DarkPalette { get; } = new()
    {
        Accent = Color.Parse("#ff0073cf"),
        AltHigh = Colors.Black,
        AltLow = Colors.Black,
        AltMedium = Colors.Black,
        AltMediumHigh = Colors.Black,
        AltMediumLow = Colors.Black,
        BaseHigh = Colors.White,
        BaseLow = Color.Parse("#ff333333"),
        BaseMedium = Color.Parse("#ff9a9a9a"),
        BaseMediumHigh = Color.Parse("#ffb4b4b4"),
        BaseMediumLow = Color.Parse("#ff676767"),
        ChromeAltLow = Color.Parse("#ffb4b4b4"),
        ChromeBlackHigh = Colors.Black,
        ChromeBlackLow = Color.Parse("#ffb4b4b4"),
        ChromeBlackMedium = Colors.Black,
        ChromeBlackMediumLow = Colors.Black,
        ChromeDisabledHigh = Color.Parse("#ff333333"),
        ChromeDisabledLow = Color.Parse("#ff9a9a9a"),
        ChromeGray = Colors.Gray,
        ChromeHigh = Colors.Gray,
        ChromeLow = Color.Parse("#ff151515"),
        ChromeMedium = Color.Parse("#ff1d1d1d"),
        ChromeMediumLow = Color.Parse("#ff2c2c2c"),
        ChromeWhite = Colors.White,
        ListLow = Color.Parse("#ff1d1d1d"),
        ListMedium = Color.Parse("#ff333333"),
        RegionColor = Color.Parse("#1E1E1E")
    };

    public FluentStyles()
    {
        var lightTheme = new ResourceDictionary
        {
            [nameof(ColorPaletteResources.RegionColor)] = Color.Parse("#EEEEF2"),

            // TreeDataGrid
            //["ColumnHeaderBackgroundBrush"] = new SolidColorBrush(Color.Parse("#FF2B2B2B"))
        };

        var darkTheme = new ResourceDictionary
        {
            [nameof(ColorPaletteResources.RegionColor)] = Color.Parse("#1F1F1F"),

            // TreeDataGrid
            //["ColumnHeaderBackgroundBrush"] = new SolidColorBrush(Color.Parse("#FFF4F4F4")),
            //["TreeDataGridHeaderBorderBrushPointerOverBrush"] = new SolidColorBrush(Color.Parse("#FFF4F4F4")),
            //["TreeDataGridHeaderBorderBrushPressedBrush"] = new SolidColorBrush(Colors.Transparent)
        };

        Resources = new ResourceDictionary
        {
            ThemeDictionaries =
            {
                [ThemeVariant.Light] = lightTheme,
                [ThemeVariant.Dark] = darkTheme
            }
        };
    }
}
