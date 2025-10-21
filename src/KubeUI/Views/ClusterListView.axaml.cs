using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Threading;

namespace KubeUI.Views;

public sealed partial class ClusterListView : UserControl
{
    public ClusterListView()
    {
        InitializeComponent();

        // Apply initial ascending sort to the first column after layout.
        ClustersGrid.AttachedToVisualTree += (_, __) =>
            Dispatcher.UIThread.Post(() =>
            {
                if (ClustersGrid.Columns?.Count > 0)
                    ClustersGrid.Columns[0].Sort(ListSortDirection.Ascending);
            });
    }
}
