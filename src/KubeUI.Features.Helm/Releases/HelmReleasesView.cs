using System.Diagnostics.CodeAnalysis;
using Avalonia.Xaml.Interactions.Core;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Helm.NET;
using KubeUI.Controls.Layout;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Clusters.Presentation;
using Ursa.Controls;

namespace KubeUI.Features.Helm.Releases;

public sealed class HelmReleasesView : KubeUIView<HelmReleasesViewModel>
{
    [RequiresUnreferencedCode("DynamicTableView creates columns from runtime Helm release metadata.")]
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

        var root = new Grid()
            .Rows("Auto,*")
            .Children(
                new CompactToolbar
                {
                    Leading = new Button()
                        .Command(vm, x => x.NewReleaseCommand)
                        .Content(new FluentIcon().Icon(Icon.AddSquare))
                        .ToolTip_Tip(L10n.Instance.HelmReleasesView_Install.ToBinding()),
                    Count = new Label()
                        .VerticalContentAlignment(VerticalAlignment.Center)
                        .Content(CompiledBinding.Create<HelmReleasesViewModel, int>(
                            x => x.ItemCount,
                            source: vm,
                            stringFormat: L10n.Instance.HelmReleasesView_ItemsFormat.CurrentValue()!)),
                    Trailing = new StackPanel()
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
                            new Button()
                                .Command(vm, x => x.RefreshCommand)
                                .Content(new FluentIcon().Icon(Icon.ArrowSync))
                                .ToolTip_Tip(SharedL10n.Instance.Shared_Refresh.ToBinding()))
                },
                table.Row(1));
        Interaction.GetBehaviors(root).Add(new EventTriggerBehavior
        {
            EventName = nameof(Loaded),
            Actions = [new InvokeCommandAction { Command = vm.RefreshCommand }]
        });
        return root;
    }

    private static NamespaceScopePicker CreateNamespaceSelector(HelmReleasesViewModel viewModel)
    {
        return new NamespaceScopePicker()
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .BindValue(NamespaceScopePicker.NamespacesProperty, CompiledBinding.Create<HelmReleasesViewModel, IEnumerable>(x => x.Cluster!.Runtime.Namespaces, source: viewModel, mode: BindingMode.OneWay))
            .BindValue(NamespaceScopePicker.SelectedNamespacesProperty, CompiledBinding.Create<HelmReleasesViewModel, IList>(x => x.SelectedNamespaces, source: viewModel, mode: BindingMode.OneWay))
            .BindValue(NamespaceScopePicker.IsSelectionLinkedProperty, CompiledBinding.Create<HelmReleasesViewModel, bool>(x => x.IsNamespaceSelectionLinked, source: viewModel, mode: BindingMode.TwoWay));
    }

    private static IEnumerable<MenuItem> CreateContextMenuItems(HelmReleasesViewModel vm, IReadOnlyList<object> targets)
    {
        foreach (var release in targets.OfType<HelmRelease>())
        {
            yield return new MenuItem()
                .Header(L10n.Instance.HelmReleasesView_Visualize.ToBinding())
                .Command(vm, x => x.VisualizeReleaseCommand)
                .CommandParameter(release)
                .Icon(new FluentIcon().Icon(Icon.DataUsage));
            yield return new MenuItem()
                .Header(L10n.Instance.HelmReleasesView_Values.ToBinding())
                .Command(vm, x => x.ViewValuesCommand)
                .CommandParameter(release)
                .Icon(new FluentIcon().Icon(Icon.Code));
            yield return new MenuItem()
                .Header(L10n.Instance.HelmReleasesView_Upgrade.ToBinding())
                .Command(vm, x => x.UpgradeReleaseCommand)
                .CommandParameter(release)
                .Icon(new FluentIcon().Icon(Icon.Edit));
            yield return new MenuItem()
                .Header(L10n.Instance.HelmReleasesView_Rollback.ToBinding())
                .Command(vm, x => x.RollbackCommand)
                .CommandParameter(release)
                .Icon(new FluentIcon().Icon(Icon.ArrowUndo));
            yield return new MenuItem()
                .Header(L10n.Instance.HelmReleasesView_Uninstall.ToBinding())
                .Command(vm, x => x.UninstallCommand)
                .CommandParameter(release)
                .Icon(new FluentIcon().Icon(Icon.Delete));
        }
    }
}
