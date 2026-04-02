using System.Windows.Input;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using KubeUI.Avalonia.Features.Resources.Common;

namespace KubeUI.Avalonia.Features.Resources.Common;

public sealed partial class MenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string? Header { get; set; }

    [ObservableProperty]
    public partial ICommand? Command { get; set; }

    [ObservableProperty]
    public partial object? CommandParameter { get; set; }

    [ObservableProperty]
    public partial AvaloniaList<MenuItemViewModel>? Items { get; set; }

    [ObservableProperty]
    public partial string? IconResource { get; set; }

    [ObservableProperty]
    public partial Icon? FluentIcon { get; set; }

    [ObservableProperty]
    public partial bool IsSeparator { get; set; }
}
