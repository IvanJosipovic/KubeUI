using Avalonia;
using KubeUI.ViewModels;

namespace KubeUI.Views;

public sealed class PortForwarderListView : MyViewBase<PortForwarderListViewModel>
{
    protected override object Build(PortForwarderListViewModel? vm)
    {
        return new Grid()
            .Children([
                new DataGrid()
                    .Set(x => {
                        x.CanUserReorderColumns = true;
                        x.CanUserResizeColumns = true;
                        x.GridLinesVisibility = DataGridGridLinesVisibility.All;
                        x.IsReadOnly = true;
                        x.ItemsSource = @vm.Cluster.PortForwarders;
                        x._set(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedItem)));

                        x.Columns([
                            new DataGridTextColumn()
                            {
                                Binding = new Binding("PodName"),
                                Header = "Pod Name",
                            },
                            new DataGridTextColumn()
                            {
                                Binding = new Binding("Namespace"),
                                Header = "Namespace",
                            },
                            new DataGridTextColumn()
                            {
                                Binding = new Binding("ContainerPort"),
                                Header = "Container Port",
                            },
                            new DataGridTextColumn()
                            {
                                Binding = new Binding("LocalPort"),
                                Header = "Local Port",
                            },
                            new DataGridTextColumn()
                            {
                                Binding = new Binding("Status"),
                                Header = "Status",
                            },
                        ]);

                        x.ContextMenu([
                            new MenuItem()
                                .Command(vm.OpenCommand)
                                .CommandParameter(new Binding(nameof(vm.SelectedItem)))
                                .Header("Open in Browser"),
                            new MenuItem()
                                .Command(vm.RemoveCommand)
                                .CommandParameter(new Binding(nameof(vm.SelectedItem)))
                                .Header("Remove"),
                        ]);

                        Dispatcher.UIThread.Post(() => x.Columns[0].Sort(ListSortDirection.Ascending));
                        return x;
                    }),
                ]);
    }
}
