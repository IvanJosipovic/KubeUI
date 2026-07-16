using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Avalonia.Controls;
using KubeUI.Avalonia.Features.Clusters.Catalog.ViewModels;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;

namespace KubeUI.Avalonia.Features.Clusters.Catalog.Views;

public sealed partial class ClusterListView : ViewBase<ClusterListViewModel>
{
    public ClusterListView()
    {
        if (Design.IsDesignMode)
        {
            DataContext = DesignTimePreview.Get<ClusterListViewModel>();
        }
    }

    protected override object Build(ClusterListViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Children(
                new DataGrid()
                    .CanUserReorderColumns(true)
                    .CanUserResizeColumns(true)
                    .GridLinesVisibility(DataGridGridLinesVisibility.All)
                    .IsReadOnly(true)
                    .ItemsSource(vm, x => x.ClusterCatalog.Clusters)
                    .SelectedItem(vm, x => x.SelectedItem)
                    .ContextMenu(
                        new ContextMenu()
                            .Items(
                                new MenuItem()
                                    .Command(vm, x => x.DeleteCommand)
                                    .CommandParameter(vm, x => x.SelectedItem)
                                    .Header(Assets.Resources.ClusterListView_Delete)
                                    .Icon(new FluentIcon().Icon(Icon.Delete))))
                    .Columns([
                        new MyDataGridTextColumn
                        {
                            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                            Binding = CompiledBinding.Create<ClusterWorkspaceViewModel, string>(x => x.Name),
                            Header = Assets.Resources.ClusterListView_Name,
                            SortDirection = ListSortDirection.Ascending
                        },
                        new MyDataGridTextColumn
                        {
                            Binding = CompiledBinding.Create<ClusterWorkspaceViewModel, string?>(x => x.KubeConfigPath),
                            Header = Assets.Resources.ClusterListView_KubeConfig
                        }
                    ]));
    }
}

