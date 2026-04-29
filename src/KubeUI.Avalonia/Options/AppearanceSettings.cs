namespace KubeUI.Avalonia.Options;

public enum LocalThemeVariant
{
    Default,
    Light,
    Dark,
}

public sealed partial class AppearanceSettings : ObservableObject
{
    [ObservableProperty]
    public partial LocalThemeVariant Theme { get; set; } = LocalThemeVariant.Dark;

    [ObservableProperty]
    public partial decimal FontSize { get; set; } = 13;

    [ObservableProperty]
    public partial decimal ConsoleFontSize { get; set; } = 12;

    [ObservableProperty]
    public partial decimal ListRowHeight { get; set; } = 22;
}
