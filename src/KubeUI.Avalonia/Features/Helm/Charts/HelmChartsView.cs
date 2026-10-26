using System.Diagnostics.CodeAnalysis;
using Avalonia.Xaml.Interactions.Core;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Helm.NET;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;

namespace KubeUI.Avalonia.Features.Helm.Charts;

/// <summary>Repository-filtered Helm chart search with install actions.</summary>
public sealed class HelmChartsView : KubeUIView<HelmChartsViewModel>
{
    /// <inheritdoc />
    [RequiresUnreferencedCode("DynamicTableView creates columns from runtime Helm chart metadata.")]
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
        return new Grid()
            .Behaviors(new EventTriggerBehavior
            {
                EventName = nameof(Loaded),
                Actions = [new InvokeCommandAction { Command = vm.RefreshCommand }]
            })
            .Rows("Auto,*").Children(
            new Grid().Row(0).MinHeight(32).Margin(0, 2, 0, 2).Cols("Auto,*,Auto").Children(
                new Label().Margin(2, 0, 0, 0).VerticalContentAlignment(VerticalAlignment.Center)
                    .Content(CompiledBinding.Create<HelmChartsViewModel, int>(x => x.ItemCount,
                        source: vm, stringFormat: Assets.Resources.HelmChartsView_ItemsFormat)),
                new StackPanel().Col(2).MaxWidth(760)
                    .HorizontalAlignment(HorizontalAlignment.Right)
                    .Orientation(Orientation.Horizontal)
                    .Children(
                        new TextBox()
                            .Width(180)
                            .MinWidth(120)
                            .Height(32)
                            .Margin(0, 0, 2, 0)
                            .VerticalContentAlignment(VerticalAlignment.Center)
                            .PlaceholderText(Assets.Resources.HelmChartsView_SearchPlaceholder)
                            .Text(vm, x => x.SearchText, BindingMode.TwoWay),
                        new ComboBox().Name("Repository", Scope)
                            .Width(200)
                            .MinWidth(140)
                            .Height(32)
                            .Margin(0, 0, 2, 0)
                            .HorizontalAlignment(HorizontalAlignment.Stretch)
                            .VerticalContentAlignment(VerticalAlignment.Center)
                            .Classes("ClearButton")
                            .PlaceholderText(Assets.Resources.HelmReleaseEditorView_Repository)
                            .IsEnabled(vm, x => !x.IsLoading)
                            .ItemsSource(vm, x => x.Repositories)
                            .SelectedItem(vm, x => x.SelectedRepository, BindingMode.TwoWay),
                        new Button()
                            .Margin(0, 0, 2, 0)
                            .Command(vm, x => x.RefreshCommand)
                            .Content(new FluentIcon().Icon(Icon.ArrowSync))
                            .ToolTip_Tip(Assets.Resources.Shared_Refresh))),
            table.Row(1));
    }

}
