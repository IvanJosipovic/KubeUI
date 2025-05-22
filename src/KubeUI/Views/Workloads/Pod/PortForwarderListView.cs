namespace KubeUI.Views;

public sealed class PortForwarderListView : MyViewBase<PortForwarderListViewModel>
{
    protected override object Build(PortForwarderListViewModel? vm)
    {
        return new Grid()
                    .Children([
                        new TreeDataGrid()
                        .Source(vm.Source)
                        .ContextMenu(new ContextMenu()
                                        .Items([
                                            new MenuItem()
                                                .Command(vm.OpenCommand)
                                                .CommandParameter(vm.Source.RowSelection.SelectedItem)
                                                .Header("Open in Browser"),
                                            new MenuItem()
                                                .Command(vm.RemoveCommand)
                                                .CommandParameter(vm.Source.RowSelection.SelectedItem)
                                                .Header("Remove"),
                                        ])
                        ),
                ]);
    }
}
