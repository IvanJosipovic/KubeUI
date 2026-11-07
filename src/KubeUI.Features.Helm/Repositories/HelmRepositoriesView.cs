using FluentIcons.Avalonia;
using FluentIcons.Common;
using Helm.NET;
using KubeUI.Controls.Layout;
using KubeUI.Controls.Presentation;

namespace KubeUI.Features.Helm.Repositories;

public sealed class HelmRepositoriesView : KubeUIView<HelmRepositoriesViewModel>
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
                new CompactToolbar
                {
                    Leading = new Button()
                        .Command(vm, x => x.NewRepositoryCommand)
                        .Content(new FluentIcon().Icon(Icon.AddSquare))
                        .ToolTip_Tip(HelmLanguageManager.Instance.HelmRepositoriesView_New.ToBinding()),
                    Count = new Label()
                        .VerticalContentAlignment(VerticalAlignment.Center)
                        .Content(CompiledBinding.Create<HelmRepositoriesViewModel, int>(
                            x => x.ItemCount, source: vm,
                            stringFormat: HelmLanguageManager.Instance.HelmRepositoriesView_ItemsFormat.CurrentValue()!)),
                    Trailing = new StackPanel()
                        .MaxWidth(700)
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
                                .ToolTip_Tip(HelmLanguageManager.Instance.HelmRepositoriesView_UpdateIndexes.ToBinding()),
                            new Button()
                                .Command(vm, x => x.RefreshCommand)
                                .Content(new FluentIcon().Icon(Icon.ArrowSync))
                                .ToolTip_Tip(FoundationLanguageManager.Instance.Shared_Refresh.ToBinding()))
                }.Row(0),
                table.Row(1));
    }

    private static IEnumerable<MenuItem> CreateContextMenuItems(HelmRepositoriesViewModel vm, IReadOnlyList<object> targets)
    {
        foreach (var repository in targets.OfType<HelmRepository>())
        {
            yield return new MenuItem()
                .Header(HelmLanguageManager.Instance.HelmRepositoriesView_ViewCharts.ToBinding())
                .Command(vm, x => x.ViewChartsCommand)
                .CommandParameter(repository)
                .Icon(new FluentIcon().Icon(Icon.DataUsage));
            yield return new MenuItem()
                .Header(HelmLanguageManager.Instance.HelmRepositoriesView_Edit.ToBinding())
                .Command(vm, x => x.EditRepositoryCommand)
                .CommandParameter(repository)
                .Icon(new FluentIcon().Icon(Icon.Edit));
            yield return new MenuItem()
                .Header(HelmLanguageManager.Instance.HelmRepositoriesView_Delete.ToBinding())
                .Command(vm, x => x.DeleteRepositoryCommand)
                .CommandParameter(repository)
                .Icon(new FluentIcon().Icon(Icon.Delete));
        }
    }
}
