using System.Collections;
using System.Collections.Generic;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Helm.NET;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure;
using Ursa.Controls;

namespace KubeUI.Avalonia.Features.Helm.Charts;

/// <summary>Repository-filtered Helm chart search with install actions.</summary>
public sealed class HelmChartsView : ViewBase<HelmChartsViewModel>
{
    /// <inheritdoc />
    protected override object Build(HelmChartsViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        DynamicTableView table = new()
        {
            GridLinesVisibility = DynamicTableViewGridLinesVisibility.All,
            Source = vm.TableSource,
            ContextMenu = new ContextMenu(),
            ContextMenuItemsFactory = targets => targets.OfType<HelmChartSearchResult>().Select(chart =>
                new MenuItem()
                    .Header(Assets.Resources.HelmChartsView_Install)
                    .Command(vm, x => x.InstallChartCommand)
                    .CommandParameter(chart)
                    .Icon(new FluentIcon().Icon(Icon.AddSquare)))
        };
        return new Grid().Rows("Auto,*").Children(
            new Grid().Row(0).MinHeight(32).Margin(0, 2, 0, 2).Cols("Auto,Auto,*,Auto").Children(
                new Label().Margin(2, 0, 0, 0).VerticalContentAlignment(VerticalAlignment.Center)
                    .Content(CompiledBinding.Create<HelmChartsViewModel, int>(x => x.ItemCount,
                        source: vm, stringFormat: Assets.Resources.HelmChartsView_ItemsFormat)),
                new StackPanel().Col(3).MaxWidth(700)
                    .HorizontalAlignment(HorizontalAlignment.Right)
                    .Orientation(Orientation.Horizontal)
                    .Children(
                        new TextBox()
                            .Width(200)
                            .MinWidth(120)
                            .Margin(0, 0, 2, 0)
                            .HorizontalAlignment(HorizontalAlignment.Stretch)
                            .VerticalAlignment(VerticalAlignment.Stretch)
                            .VerticalContentAlignment(VerticalAlignment.Center)
                            .Background(Brushes.Transparent)
                            .PlaceholderText(Assets.Resources.ResourceListView_SearchWatermark)
                            .Text(vm, x => x.SearchText, BindingMode.TwoWay),
                        CreateRepositorySelector(vm),
                        CreateNamespaceSelector(vm),
                        new ToggleButton()
                            .Margin(0, 0, 2, 0)
                            .IsChecked(vm, x => x.IsNamespaceSelectionLinked, BindingMode.TwoWay)
                            .ToolTip_Tip(Assets.Resources.ResourceListView_NamespaceLink)
                            .Content(new FluentIcon().Icon(Icon.Link)),
                        new Button()
                            .Margin(0, 0, 2, 0)
                            .Command(vm, x => x.RefreshCommand)
                            .Content(new FluentIcon().Icon(Icon.ArrowSync))
                            .ToolTip_Tip(Assets.Resources.Shared_Refresh))),
            table.Row(1));
    }

    private static ComboBox CreateRepositorySelector(HelmChartsViewModel viewModel)
    {
        return new ComboBox()
            .Width(200)
            .MinWidth(140)
            .Height(32)
            .Margin(0, 0, 2, 0)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalContentAlignment(VerticalAlignment.Center)
            .Classes("ClearButton")
            .PlaceholderText(Assets.Resources.HelmChartsView_Repository)
            .IsEnabled(viewModel, x => !x.IsLoading)
            .ItemsSource(viewModel, x => x.Repositories)
            .SelectedItem(viewModel, x => x.SelectedRepository, BindingMode.TwoWay);
    }

    private static MultiComboBox CreateNamespaceSelector(HelmChartsViewModel viewModel)
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
            .ItemsSource(CompiledBinding.Create<HelmChartsViewModel, IEnumerable>(x => x.Cluster!.Runtime.Namespaces, source: viewModel))
            .PlaceholderText(Assets.Resources.ResourceListView_SelectNamespace)
            .SelectedItems(CompiledBinding.Create<HelmChartsViewModel, IList>(x => x.SelectedNamespaces, source: viewModel))
            .ItemTemplate(template)
            .SelectedItemTemplate(template);
    }
}
