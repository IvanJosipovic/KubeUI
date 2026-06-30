using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Declarative;
using FluentAvalonia.UI.Controls;
using k8s.Models;
using KubeUI.Avalonia.Controls;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public partial class PortForwarderListView : ViewBase<PortForwarderListViewModel>
{
    private DataGrid? _grid;

    protected override object Build(PortForwarderListViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        DataGrid grid = new();
        grid.CanUserReorderColumns = true;
        grid.CanUserResizeColumns = true;
        grid.GridLinesVisibility = DataGridGridLinesVisibility.All;
        grid.IsReadOnly = true;
        grid.ItemsSource = vm.Cluster.PortForwarders;
        grid.SelectedItem = vm.SelectedItem;
        grid.ContextMenu = new ContextMenu()
            .Items(
                new MenuItem()
                    .Header(Assets.Resources.PortForwarderListView_OpenInBrowser)
                    .Command(vm, x => x.OpenCommand)
                    .CommandParameter(vm, x => x.SelectedItem),
                new MenuItem()
                    .Header(Assets.Resources.PortForwarderListView_Remove)
                    .Command(vm, x => x.RemoveCommand)
                    .CommandParameter(vm, x => x.SelectedItem));

        grid.SelectionChanged += (_, _) => vm.SelectedItem = grid.SelectedItem as PortForwarder;

        _grid = grid;

        _grid.Columns.Add(CreateColumn(nameof(PortForwarder.Type), Assets.Resources.PortForwarderListView_Type!, 80));
        _grid.Columns.Add(CreateColumn(nameof(PortForwarder.Name), Assets.Resources.PortForwarderListView_Name!, 1, true));
        _grid.Columns.Add(CreateColumn(nameof(PortForwarder.Namespace), Assets.Resources.PortForwarderListView_Namespace!, 120));
        _grid.Columns.Add(CreateColumn(nameof(PortForwarder.Port), Assets.Resources.PortForwarderListView_Port!, 80));
        _grid.Columns.Add(CreateColumn(nameof(PortForwarder.LocalPort), Assets.Resources.PortForwarderListView_LocalPort!, 100));
        _grid.Columns.Add(CreateColumn(nameof(PortForwarder.Connections), Assets.Resources.PortForwarderListView_Connections!, 120));
        _grid.Columns.Add(CreateColumn(nameof(PortForwarder.Status), Assets.Resources.PortForwarderListView_Status!, 140));

        return new Grid().Children(_grid);
    }

    private static MyDataGridTextColumn CreateColumn(string bindingPath, string header, double width, bool isStar = false)
    {
        MyDataGridTextColumn column = new();
        column.Binding = new Binding(bindingPath);
        column.Header = header;
        column.Width = isStar ? new DataGridLength(width, DataGridLengthUnitType.Star) : new DataGridLength(width);

        return column;
    }
}
