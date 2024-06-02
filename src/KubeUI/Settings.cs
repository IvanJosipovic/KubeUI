using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KubeUI;

public partial class Settings : ObservableObject
{
    [ObservableProperty]
    public ThemeVariant _theme;
}
