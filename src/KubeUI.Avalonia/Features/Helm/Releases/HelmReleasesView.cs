using Avalonia.Controls.Templates;
using Avalonia.Controls.Primitives;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Helm.NET;
using k8s.Models;
using Ursa.Controls;

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
                    .Cols("Auto,Auto,Auto,Auto,Auto,Auto,Auto,*,Auto")
                    .Children(
                        new Button()
                            .Col(0)
                            .Margin(0, 0, 6, 0)
                            .Command(vm, x => x.NewReleaseCommand)
                            .Content(new FluentIcon().Icon(Icon.Add))
                            .ToolTip_Tip(Assets.Resources.HelmReleasesView_Install),
                        CreateNamespaceSelector(vm).Col(1),
                        new ToggleButton()
                            .Col(2)
                            .Margin(0, 0, 6, 0)
                            .IsChecked(vm, x => x.IsNamespaceSelectionLinked, BindingMode.TwoWay)
                            .ToolTip_Tip(Assets.Resources.ResourceListView_NamespaceLink)
                            .Content(new FluentIcon().Icon(Icon.Link)),
                        new TextBlock()
                            .Col(3)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(Assets.Resources.HelmReleasesView_RollbackRevision),
                        new global::Avalonia.Controls.NumericUpDown()
                            .Col(4)
                            .Width(90)
                            .Minimum(0)
                            .Value(vm, x => x.RollbackRevision, BindingMode.TwoWay),
                        new Button()
                            .Col(5)
                            .Command(vm, x => x.RollbackCommand)
                            .CommandParameter(vm, x => x.SelectedRelease)
                            .Content(Assets.Resources.HelmReleasesView_Rollback),
                        new CheckBox()
                            .Col(6)
                            .Margin(6, 0, 6, 0)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Content(Assets.Resources.HelmReleasesView_Wait)
                            .IsChecked(vm, x => x.WaitForActions, BindingMode.TwoWay),
                        new TextBlock()
                            .Col(7)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(vm, x => x.Status),
                        new Button()
                            .Col(8)
                            .Command(vm, x => x.RefreshCommand)
                            .Content(new FluentIcon().Icon(Icon.ArrowSync))
                            .ToolTip_Tip(Assets.Resources.HelmReleasesView_Refresh)),
                table.Row(1));
    }

    private static MultiComboBox CreateNamespaceSelector(HelmReleasesViewModel viewModel)
    {
        var template = new FuncDataTemplate<V1Namespace>((ns, _) =>
            new TextBlock().Text(ns?.Metadata?.Name ?? string.Empty));

        return new MultiComboBox()
            .Width(200)
            .MinWidth(140)
            .MaxHeight(20)
            .Margin(0, 0, 2, 0)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Classes("ClearButton")
            .ItemsSource(CompiledBinding.Create<HelmReleasesViewModel, IEnumerable>(x => x.Cluster!.Runtime.Namespaces, source: viewModel))
            .PlaceholderText(Assets.Resources.ResourceListView_SelectNamespace)
            .SelectedItems(CompiledBinding.Create<HelmReleasesViewModel, IList>(x => x.SelectedNamespaces, source: viewModel))
            .ItemTemplate(template)
            .SelectedItemTemplate(template);
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
