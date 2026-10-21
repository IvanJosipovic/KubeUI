using FluentIcons.Avalonia;
using FluentIcons.Common;
using Helm.NET;

namespace KubeUI.Avalonia.Features.Helm.Repositories;

public sealed class HelmRepositoriesView : ViewBase<HelmRepositoriesViewModel>
{
    protected override object Build(HelmRepositoriesViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        DynamicTableView table = new()
        {
            GridLinesVisibility = DynamicTableViewGridLinesVisibility.All,
            Source = vm.TableSource,
            ContextMenu = new ContextMenu(),
            ContextMenuItemsFactory = targets => CreateContextMenuItems(vm, targets)
        };

        return new Grid()
            .Rows("Auto,*")
            .Children(
                new Grid()
                    .Row(0)
                    .Cols("Auto,Auto,*,Auto")
                    .Children(
                        new Button()
                            .Col(0)
                            .Margin(0, 0, 6, 0)
                            .Command(vm, x => x.NewRepositoryCommand)
                            .Content(new FluentIcon().Icon(Icon.Add))
                            .ToolTip_Tip(Assets.Resources.HelmRepositoriesView_New),
                        new Button()
                            .Col(1)
                            .Command(vm, x => x.UpdateIndexesCommand)
                            .Content(new FluentIcon().Icon(Icon.ArrowSync))
                            .ToolTip_Tip(Assets.Resources.HelmRepositoriesView_UpdateIndexes),
                        new TextBlock()
                            .Col(2)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(vm, x => x.Status),
                        new Button()
                            .Col(3)
                            .Command(vm, x => x.RefreshCommand)
                            .Content(new FluentIcon().Icon(Icon.ArrowSync))
                            .ToolTip_Tip(Assets.Resources.HelmRepositoriesView_Refresh)),
                table.Row(1));
    }

    private static IEnumerable<MenuItem> CreateContextMenuItems(HelmRepositoriesViewModel vm, IReadOnlyList<object> targets)
    {
        foreach (var repository in targets.OfType<HelmRepository>())
        {
            yield return new MenuItem()
                .Header(Assets.Resources.HelmRepositoriesView_UpdateIndexes)
                .Command(vm, x => x.UpdateRepositoryCommand)
                .CommandParameter(repository)
                .Icon(new FluentIcon().Icon(Icon.ArrowSync));
            yield return new MenuItem()
                .Header(Assets.Resources.HelmRepositoriesView_Edit)
                .Command(vm, x => x.EditRepositoryCommand)
                .CommandParameter(repository)
                .Icon(new FluentIcon().Icon(Icon.Edit));
            yield return new MenuItem()
                .Header(Assets.Resources.HelmRepositoriesView_Delete)
                .Command(vm, x => x.DeleteRepositoryCommand)
                .CommandParameter(repository)
                .Icon(new FluentIcon().Icon(Icon.Delete));
        }
    }
}
