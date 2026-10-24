using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Xaml.Interactions.Core;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Helm.NET;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure;
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
            .Behaviors(new EventTriggerBehavior
            {
                EventName = nameof(Loaded),
                Actions = [new InvokeCommandAction { Command = vm.RefreshCommand }]
            })
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
                            .Command(vm, x => x.NewReleaseCommand)
                            .Content(new FluentIcon().Icon(Icon.AddSquare))
                            .ToolTip_Tip(Assets.Resources.HelmReleasesView_Install),
                        new Label()
                            .Col(1)
                            .Margin(2, 0, 0, 0)
                            .VerticalContentAlignment(VerticalAlignment.Center)
                            .Content(CompiledBinding.Create<HelmReleasesViewModel, int>(
                                x => x.ItemCount,
                                source: vm,
                                stringFormat: Assets.Resources.HelmReleasesView_ItemsFormat)),
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
                                CreateNamespaceSelector(vm),
                                new ToggleButton()
                                    .Margin(0, 0, 2, 0)
                                    .IsChecked(vm, x => x.IsNamespaceSelectionLinked, BindingMode.TwoWay)
                                    .ToolTip_Tip(Assets.Resources.ResourceListView_NamespaceLink)
                                    .Content(new FluentIcon().Icon(Icon.Link)),
                                new Button()
                                    .Command(vm, x => x.RefreshCommand)
                                    .Content(new FluentIcon().Icon(Icon.ArrowSync))
                                    .ToolTip_Tip(Assets.Resources.Shared_Refresh))),
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
                .Header(Assets.Resources.HelmReleasesView_Values)
                .Command(vm, x => x.ViewValuesCommand)
                .CommandParameter(release)
                .Icon(new FluentIcon().Icon(Icon.Code));
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
