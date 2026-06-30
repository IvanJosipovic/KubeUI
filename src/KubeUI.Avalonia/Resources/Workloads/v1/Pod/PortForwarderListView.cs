using System.Linq.Expressions;
using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Controls;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public partial class PortForwarderListView : ViewBase<PortForwarderListViewModel>
{
    protected override object Build(PortForwarderListViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new DataGrid()
            .CanUserReorderColumns(true)
            .CanUserResizeColumns(true)
            .GridLinesVisibility(DataGridGridLinesVisibility.All)
            .IsReadOnly(true)
            .ItemsSource(vm, x => x.Cluster.PortForwarders)
            .SelectedItem(vm, x => x.SelectedItem)
            .ContextMenu(new ContextMenu()
                .Items(
                    new MenuItem()
                        .Header(Assets.Resources.PortForwarderListView_OpenInBrowser)
                        .Command(vm, x => x.OpenCommand)
                        .CommandParameter(vm, x => x.SelectedItem),
                    new MenuItem()
                        .Header(Assets.Resources.PortForwarderListView_Remove)
                        .Command(vm, x => x.RemoveCommand)
                        .CommandParameter(vm, x => x.SelectedItem)))
            .OnSelectionChanged(args => vm.SelectedItem = (args.Source as DataGrid)?.SelectedItem as PortForwarder)
            .Columns([
                CreateColumn(x => x.Type, Assets.Resources.PortForwarderListView_Type!, 80),
                CreateColumn(x => x.Name, Assets.Resources.PortForwarderListView_Name!, 1, true),
                CreateColumn(x => x.Namespace, Assets.Resources.PortForwarderListView_Namespace!, 120),
                CreateColumn(x => x.Port, Assets.Resources.PortForwarderListView_Port!, 80),
                CreateColumn(x => x.LocalPort, Assets.Resources.PortForwarderListView_LocalPort!, 100),
                CreateColumn(x => x.Connections, Assets.Resources.PortForwarderListView_Connections!, 120),
                CreateColumn(x => x.Status, Assets.Resources.PortForwarderListView_Status!, 140)
            ]);
    }

    private static MyDataGridTextColumn CreateColumn<TValue>(Expression<Func<PortForwarder, TValue>> bindingExpression, string header, double width, bool isStar = false)
    {
        var column = new MyDataGridTextColumn()
            .Header(header)
            .Width(isStar ? new DataGridLength(width, DataGridLengthUnitType.Star) : new DataGridLength(width));

        column.Binding = CompiledBinding.Create(bindingExpression);

        return column;
    }
}
