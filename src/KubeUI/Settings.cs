using Avalonia.Styling;

namespace KubeUI;

public partial class Settings : ObservableObject
{
    [ObservableProperty]
    public ThemeVariant _theme;
}
