using Avalonia;
using Avalonia.Controls;
using KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

namespace KubeUI.Avalonia.Features.Clusters.Overview.Controls;

public partial class ClusterOverviewRingControl : UserControl
{
    public static readonly StyledProperty<ClusterOverviewRingChartViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<ClusterOverviewRingControl, ClusterOverviewRingChartViewModel?>(nameof(ViewModel));

    public ClusterOverviewRingChartViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public ClusterOverviewRingControl()
    {
        InitializeComponent();
    }
}
