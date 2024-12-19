using KubeUI.Client;

namespace KubeUI.Views;

public sealed class PortForwarderListView : MyViewBase<PortForwarderListViewModel>
{
    protected override object Build(PortForwarderListViewModel? vm)
    {
        return new Grid()
            .Children([
                new DataGrid()
                    .SelectedItem(@vm.SelectedItem)
                    .CanUserReorderColumns(true)
                    .CanUserResizeColumns(true)
                    .GridLinesVisibility(DataGridGridLinesVisibility.All)
                    .ItemsSource(@vm.Cluster.PortForwarders)
                    .IsReadOnly(true)
                    .Set(x => {
                        x.Columns([
                            new DataGridTextColumn()
                            {
                                Binding = new Binding(nameof(PortForwarder.Type)),
                                Header = "Type",
                            },
                            new DataGridTextColumn()
                            {
                                Binding = new Binding(nameof(PortForwarder.Name)),
                                Header = "Name",
                                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                            },
                            new DataGridTextColumn()
                            {
                                Binding = new Binding(nameof(PortForwarder.Namespace)),
                                Header = "Namespace",
                            },
                            new DataGridTextColumn()
                            {
                                Binding = new Binding(nameof(PortForwarder.Port)),
                                Header = "Port",
                            },
                            new DataGridTextColumn()
                            {
                                Binding = new Binding(nameof(PortForwarder.LocalPort)),
                                Header = "Local Port",
                            },
                            new DataGridTextColumn()
                            {
                                Binding = new Binding(nameof(PortForwarder.Connections)),
                                Header = "Connections",
                            },
                            new DataGridTextColumn()
                            {
                                Binding = new Binding(nameof(PortForwarder.Status)),
                                Header = "Status",
                            },
                        ]);

                        x.ContextMenu(new ContextMenu()
                                        .Items([
                                            new MenuItem()
                                                .Command(vm.OpenCommand)
                                                .CommandParameter(new Binding(nameof(vm.SelectedItem)))
                                                .Header("Open in Browser"),
                                            new MenuItem()
                                                .Command(vm.RemoveCommand)
                                                .CommandParameter(new Binding(nameof(vm.SelectedItem)))
                                                .Header("Remove"),
                                        ])
                        );

                        Dispatcher.UIThread.Post(() => x.Columns[0].Sort(ListSortDirection.Ascending));
                    }),
                ]);
    }
}
