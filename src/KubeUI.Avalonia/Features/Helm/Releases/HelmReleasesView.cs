using FluentIcons.Avalonia;
using FluentIcons.Common;
using Helm.NET;

namespace KubeUI.Avalonia.Features.Helm.Releases;

public sealed class HelmReleasesView : ViewBase<HelmReleasesViewModel>
{
    protected override object Build(HelmReleasesViewModel vm)
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
                    .Cols("Auto,Auto,Auto,Auto,Auto,Auto,*,Auto")
                    .Children(
                        new Button()
                            .Col(0)
                            .Margin(0, 0, 6, 0)
                            .Command(vm, x => x.NewReleaseCommand)
                            .Content(new FluentIcon().Icon(Icon.Add))
                            .ToolTip_Tip(Assets.Resources.HelmReleasesView_Install),
                        new CheckBox()
                            .Col(1)
                            .Margin(0, 0, 6, 0)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Content(Assets.Resources.HelmReleasesView_AllNamespaces)
                            .IsChecked(vm, x => x.AllNamespaces, BindingMode.TwoWay),
                        new TextBlock()
                            .Col(2)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(Assets.Resources.HelmReleasesView_RollbackRevision),
                        new NumericUpDown()
                            .Col(3)
                            .Width(90)
                            .Minimum(0)
                            .Value(vm, x => x.RollbackRevision, BindingMode.TwoWay),
                        new Button()
                            .Col(4)
                            .Command(vm, x => x.RollbackCommand)
                            .CommandParameter(vm, x => x.SelectedRelease)
                            .Content(Assets.Resources.HelmReleasesView_Rollback),
                        new CheckBox()
                            .Col(5)
                            .Margin(6, 0, 6, 0)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Content(Assets.Resources.HelmReleasesView_Wait)
                            .IsChecked(vm, x => x.WaitForActions, BindingMode.TwoWay),
                        new TextBlock()
                            .Col(6)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(vm, x => x.Status),
                        new Button()
                            .Col(7)
                            .Command(vm, x => x.RefreshCommand)
                            .Content(new FluentIcon().Icon(Icon.ArrowSync))
                            .ToolTip_Tip(Assets.Resources.HelmReleasesView_Refresh)),
                table.Row(1));
    }

    private static IEnumerable<MenuItem> CreateContextMenuItems(HelmReleasesViewModel vm, IReadOnlyList<object> targets)
    {
        foreach (var release in targets.OfType<HelmRelease>())
        {
            yield return new MenuItem()
                .Header(Assets.Resources.HelmReleasesView_Visualize)
                .Command(vm, x => x.VisualizeReleaseCommand)
                .CommandParameter(release)
                .Icon(new FluentIcon().Icon(Icon.DataUsage));
            yield return new MenuItem()
                .Header(Assets.Resources.HelmReleasesView_StatusAction)
                .Command(vm, x => x.ViewStatusCommand)
                .CommandParameter(release)
                .Icon(new FluentIcon().Icon(Icon.Info));
            yield return new MenuItem()
                .Header(Assets.Resources.HelmReleasesView_Values)
                .Command(vm, x => x.ViewValuesCommand)
                .CommandParameter(release)
                .Icon(new FluentIcon().Icon(Icon.Code));
            yield return new MenuItem()
                .Header(Assets.Resources.HelmReleasesView_History)
                .Command(vm, x => x.ViewHistoryCommand)
                .CommandParameter(release)
                .Icon(new FluentIcon().Icon(Icon.History));
            yield return new MenuItem()
                .Header(Assets.Resources.HelmReleasesView_Upgrade)
                .Command(vm, x => x.UpgradeReleaseCommand)
                .CommandParameter(release)
                .Icon(new FluentIcon().Icon(Icon.Edit));
            yield return new MenuItem()
                .Header(Assets.Resources.HelmReleasesView_Rollback)
                .Command(vm, x => x.RollbackCommand)
                .CommandParameter(release)
                .Icon(new FluentIcon().Icon(Icon.ArrowUndo));
            yield return new MenuItem()
                .Header(Assets.Resources.HelmReleasesView_Uninstall)
                .Command(vm, x => x.UninstallCommand)
                .CommandParameter(release)
                .Icon(new FluentIcon().Icon(Icon.Delete));
        }
    }
}
