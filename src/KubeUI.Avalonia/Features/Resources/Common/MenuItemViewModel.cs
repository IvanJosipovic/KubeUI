using System.Windows.Input;
using Avalonia.Collections;
using FluentIcons.Common;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure.Presentation;

namespace KubeUI.Avalonia.Features.Resources.Common;

public sealed partial class MenuItemViewModel : ViewModelBase
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
