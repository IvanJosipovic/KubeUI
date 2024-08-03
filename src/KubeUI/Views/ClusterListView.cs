using KubeUI.ViewModels;

namespace KubeUI.Views;

public sealed class ClusterListView : MyViewBase<ClusterListViewModel>
{
    protected override object Build(ClusterListViewModel? vm)
    {
        return new Grid()
            .Children([
                new DataGrid()
                    .Set(x => {
                        x.CanUserReorderColumns = true;
                        x.CanUserResizeColumns = true;
                        x.GridLinesVisibility = DataGridGridLinesVisibility.All;
                        x.ItemsSource = vm.ClusterManager.Clusters;
                        x.IsReadOnly = true;
                        x.Set(DataGrid.SelectedItemProperty, @vm.SelectedItem);

                        x.Columns([
                            new DataGridTextColumn()
                            {
                                Binding = new Binding("Name"),
                                Header = "Name",
                            }
                            ]);

                        x.ContextMenu([
                            new MenuItem()
                                .Command(vm.DeleteCommand)
                                .CommandParameter(@vm.SelectedItem)
                                .Header("Delete")
                                .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("delete_regular") }),
                            ]);

                        Dispatcher.UIThread.Post(() => x.Columns[0].Sort(ListSortDirection.Ascending));
                        return x;
                    }),
                ]);
    }
}
