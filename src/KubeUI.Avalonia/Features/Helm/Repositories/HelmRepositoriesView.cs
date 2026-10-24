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
                    .MinHeight(32)
                    .Margin(0, 2, 0, 2)
                    .Cols("Auto,Auto,*,Auto")
                    .Children(
                        new Button()
                            .Col(0)
                            .Margin(2, 0, 0, 0)
                            .Command(vm, x => x.NewRepositoryCommand)
                            .Content(new FluentIcon().Icon(Icon.AddSquare))
                            .ToolTip_Tip(Assets.Resources.HelmRepositoriesView_New),
                        new Label()
                            .Col(1)
                            .Margin(2, 0, 0, 0)
                            .VerticalContentAlignment(VerticalAlignment.Center)
                            .Content(CompiledBinding.Create<HelmRepositoriesViewModel, int>(
                                x => x.ItemCount, source: vm,
                                stringFormat: Assets.Resources.HelmRepositoriesView_ItemsFormat)),
                        new StackPanel()
                            .Col(3)
                            .MaxWidth(700)
                            .HorizontalAlignment(HorizontalAlignment.Right)
                            .Orientation(Orientation.Horizontal)
                            .Children(
                                new TextBlock()
                                    .MaxWidth(300)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .TextWrapping(TextWrapping.Wrap)
                                    .Text(vm, x => x.Status),
                                new Button()
                                    .Margin(0, 0, 2, 0)
                                    .Command(vm, x => x.UpdateIndexesCommand)
                                    .Content(new FluentIcon().Icon(Icon.ArrowDownload))
                                    .ToolTip_Tip(Assets.Resources.HelmRepositoriesView_UpdateIndexes),
                                new Button()
                                    .Command(vm, x => x.RefreshCommand)
                                    .Content(new FluentIcon().Icon(Icon.ArrowSync))
                                    .ToolTip_Tip(Assets.Resources.Shared_Refresh))),
                table.Row(1));
    }

    private static IEnumerable<MenuItem> CreateContextMenuItems(HelmRepositoriesViewModel vm, IReadOnlyList<object> targets)
    {
        foreach (var repository in targets.OfType<HelmRepository>())
        {
            yield return new MenuItem()
                .Header(Assets.Resources.HelmRepositoriesView_ViewCharts)
                .Command(vm, x => x.ViewChartsCommand)
                .CommandParameter(repository)
                .Icon(new FluentIcon().Icon(Icon.DataUsage));
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
