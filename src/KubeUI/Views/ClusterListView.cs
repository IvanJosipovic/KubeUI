namespace KubeUI.Views;

public sealed class ClusterListView : MyViewBase<ClusterListViewModel>
{
    protected override object Build(ClusterListViewModel? vm)
    {
        return new Grid()
                    .Children([
                        new DataGrid()
                            .SelectedItem(@vm.SelectedItem)
                            .CanUserReorderColumns(true)
                            .CanUserResizeColumns(true)
                            .GridLinesVisibility(DataGridGridLinesVisibility.All)
                            .ItemsSource(@vm.ClusterManager.Clusters)
                            .IsReadOnly(true)
                            .ContextMenu(new ContextMenu()
                                            .Items([
                                                new MenuItem()
                                                    .Command(vm.DeleteCommand)
                                                    .CommandParameter(@vm.SelectedItem)
                                                    .Header("Delete")
                                                    .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("delete_regular") })
                                            ])
                            )
                            .Set(x => {
                                x.Columns([
                                    new DataGridTextColumn()
                                    {
                                        Binding = new Binding("Name"),
                                        Header = "Name",
                                        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                                    },
                                    new DataGridTextColumn()
                                    {
                                        Binding = new Binding("KubeConfigPath"),
                                        Header = "KubeConfig",
                                    },
                                ]);

                                Dispatcher.UIThread.Post(() => x.Columns[0].Sort(ListSortDirection.Ascending));
                            }),
                        ]);
    }
}
