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
                                    },
                                ]);

                                Dispatcher.UIThread.Post(() => x.Columns[0].Sort(ListSortDirection.Ascending));
                                return x;
                            }),
                        ]);
    }
}
