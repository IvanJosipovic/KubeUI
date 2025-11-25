using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Threading;

namespace KubeUI.Views;

public partial class PortForwarderListView : UserControl
{
    public PortForwarderListView()
    {
        InitializeComponent();
        // Initial ascending sort on first column (Type) after layout.
        Dispatcher.UIThread.Post(() =>
        {
            if (PortForwardersGrid.Columns.Count > 0)
            {
                PortForwardersGrid.Columns[0].Sort(ListSortDirection.Ascending);
            }
        });
    }
}
