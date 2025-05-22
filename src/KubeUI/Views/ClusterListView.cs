namespace KubeUI.Views;

public sealed class ClusterListView : MyViewBase<ClusterListViewModel>
{
    protected override object Build(ClusterListViewModel? vm)
    {
        return new Grid()
                    .Children([
                        new TreeDataGrid()
                        .Source(vm.Source)
                        .ContextMenu(new ContextMenu()
                                        .Items([
                                            new MenuItem()
                                                .Command(vm.DeleteCommand)
                                                .CommandParameter(vm.Source.RowSelection.SelectedItem)
                                                .Header("Delete")
                                                .Icon(new PathIcon() { Data = (Geometry)Application.Current.FindResource("delete_regular") })
                                        ])
                        )
                        ]);
    }
}
