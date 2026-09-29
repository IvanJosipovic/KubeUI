namespace KubeUI.Avalonia.Styles;

internal static class TerminalPalette
{
    private static readonly Color[] s_systemColors =
    [
        Color.FromRgb(0, 0, 0),
        Color.FromRgb(128, 0, 0),
        Color.FromRgb(0, 128, 0),
        Color.FromRgb(128, 128, 0),
        Color.FromRgb(0, 0, 128),
        Color.FromRgb(128, 0, 128),
        Color.FromRgb(0, 128, 128),
        Color.FromRgb(192, 192, 192),
        Color.FromRgb(128, 128, 128),
        Color.FromRgb(255, 0, 0),
        Color.FromRgb(0, 255, 0),
        Color.FromRgb(255, 255, 0),
        Color.FromRgb(0, 0, 255),
        Color.FromRgb(255, 0, 255),
        Color.FromRgb(0, 255, 255),
        Color.FromRgb(255, 255, 255),
    ];

    private static readonly byte[] s_colorCubeLevels = [0, 95, 135, 175, 215, 255];

    public static ResourceDictionary CreateResources()
    {
        var resources = new ResourceDictionary();

        for (var index = 0; index < 16; index++)
        {
            AddColor(resources, index, s_systemColors[index]);
        }

        for (var index = 16; index < 232; index++)
        {
            var cubeIndex = index - 16;
            var red = s_colorCubeLevels[cubeIndex / 36];
            var green = s_colorCubeLevels[cubeIndex / 6 % 6];
            var blue = s_colorCubeLevels[cubeIndex % 6];
            AddColor(resources, index, Color.FromRgb(red, green, blue));
        }

        for (var index = 232; index < 256; index++)
        {
            var level = (byte)(8 + (index - 232) * 10);
            AddColor(resources, index, Color.FromRgb(level, level, level));
        }

        return resources;
    }

    private static void AddColor(ResourceDictionary resources, int index, Color color)
    {
        resources[$"SvcSystems.UI.TerminalColor{index}"] = new SolidColorBrush(color);
    }
}
