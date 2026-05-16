using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

namespace KubeUI.Avalonia.Features.Clusters.Overview.Controls;

public sealed partial class ClusterOverviewControl : UserControl
{
    public ClusterOverviewControl()
    {
        InitializeComponent();
    }

    private void ClusterOverviewMode_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton toggleButton || toggleButton.Tag is not string tag)
        {
            return;
        }

        if (DataContext is not ClusterViewModel viewModel)
        {
            return;
        }

        if (tag == "CPU" && viewModel.ClusterOverviewModes.Count > 0)
        {
            viewModel.SelectedClusterOverviewMode = viewModel.ClusterOverviewModes[0];
        }
        else if (tag == "Memory" && viewModel.ClusterOverviewModes.Count > 1)
        {
            viewModel.SelectedClusterOverviewMode = viewModel.ClusterOverviewModes[1];
        }
    }
}
