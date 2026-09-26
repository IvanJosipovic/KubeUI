using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.DynamicTableView;

namespace KubeUI.Avalonia.Features.Clusters.Catalog;

public sealed partial class ClusterListView : ViewBase<ClusterListViewModel>
{
    public ClusterListView()
    {
        if (Design.IsDesignMode)
            DataContext = DesignTimePreview.Get<ClusterListViewModel>();
    }

    protected override object Build(ClusterListViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        return new KubeUI.DynamicTableView.DynamicTableView
        {
            Source = vm.TableSource,
            ContextMenu = new ContextMenu(),
            ContextMenuItemsFactory = targets => CreateContextMenuItems(vm, targets)
        };
    }

    private static IEnumerable<MenuItem> CreateContextMenuItems(ClusterListViewModel vm, IReadOnlyList<object> targets)
    {
        foreach (var cluster in targets.OfType<ClusterWorkspace>())
        {
            yield return new MenuItem()
                .Command(vm, x => x.DeleteCommand)
                .CommandParameter(cluster)
                .Header(Assets.Resources.ClusterListView_Delete)
                .Icon(new FluentIcon().Icon(Icon.Delete));
        }
    }
}
