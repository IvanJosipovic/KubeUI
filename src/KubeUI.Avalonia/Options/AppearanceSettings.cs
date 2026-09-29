namespace KubeUI.Avalonia.Options;

public enum LocalThemeVariant
{
    Default,
    Light,
    Dark,
}

public sealed partial class AppearanceSettings : ObservableObject
{
    internal const decimal MinimumFontSize = 8m;
    internal const decimal MaximumFontSize = 32m;

    [ObservableProperty]
    public partial LocalThemeVariant Theme { get; set; } = LocalThemeVariant.Dark;

    [ObservableProperty]
    public partial decimal FontSize { get; set; } = 13;

    [ObservableProperty]
    public partial decimal ConsoleFontSize { get; set; } = 12;

}
