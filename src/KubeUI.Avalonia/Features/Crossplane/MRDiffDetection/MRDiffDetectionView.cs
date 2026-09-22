using System.Collections;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Controls.Selection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Avalonia.Controls.DataGridFilters;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;

public sealed class MRDiffDetectionView : ViewBase<MRDiffDetectionViewModel>
{
    private DataGridColumnFilterFlyoutFactory? _filterFlyoutFactory;

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is not MRDiffDetectionViewModel vm)
        {
            return;
        }

        _filterFlyoutFactory ??= GetServiceProvider().GetRequiredService<DataGridColumnFilterFlyoutFactory>();
        AttachFilterFlyouts(vm.ColumnDefinitions, _filterFlyoutFactory, vm.FilteringModel);
    }

    internal static void AttachFilterFlyouts(
        IEnumerable<DataGridColumnDefinition> columns,
        DataGridColumnFilterFlyoutFactory factory,
        IFilteringModel filteringModel)
    {
        foreach (var column in columns)
        {
            if (column.Tag is IResourceListColumn resourceColumn)
            {
                column.FilterFlyout = factory.Create(resourceColumn, column, filteringModel);
            }
        }
    }

    protected override object Build(MRDiffDetectionViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new Grid()
            .Rows("Auto,*")
            .Children(
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(6)
                    .Children(
                        new ComboBox()
                            .Width(260)
                            .PlaceholderText(Assets.Resources.MRDiffDetectionView_SelectProvider)
                            .ItemsSource(vm, x => x.Providers)
                            .SelectedItem(vm, x => x.SelectedProvider, BindingMode.TwoWay)
                            .ItemTemplate(new FuncDataTemplate<CrossplaneProviderOption>((provider, _) => new TextBlock().Text(provider?.Name ?? string.Empty))),
                        new Button()
                            .Command(vm, x => x.ClearCommand)
                            .ToolTip_Tip(Assets.Resources.MRDiffDetectionView_Clear)
                            .Content(new FluentIcon().Icon(Icon.Broom))),
                new DataGrid
                    {
                        SortingAdapterFactory = vm.SortingAdapterFactory,
                        FilteringAdapterFactory = vm.FilteringAdapterFactory,
                        SearchAdapterFactory = vm.SearchAdapterFactory
                    }
                    .Row(1)
                    .CanUserReorderColumns(true)
                    .CanUserResizeColumns(true)
                    .CanUserSortColumns(true)
                    .GridLinesVisibility(DataGridGridLinesVisibility.All)
                    .IsReadOnly(true)
                    .ColumnDefinitionsSource(CompiledBinding.Create<MRDiffDetectionViewModel, IList<DataGridColumnDefinition>>(x => x.ColumnDefinitions))
                    .ItemsSource(CompiledBinding.Create<MRDiffDetectionViewModel, IList>(x => x.View))
                    .Selection(CompiledBinding.Create<MRDiffDetectionViewModel, ISelectionModel>(x => x.SelectionModel))
                    .SelectionMode(DataGridSelectionMode.Extended)
                    .FilteringModel(CompiledBinding.Create<MRDiffDetectionViewModel, IFilteringModel>(x => x.FilteringModel))
                    .SearchModel(CompiledBinding.Create<MRDiffDetectionViewModel, ISearchModel>(x => x.SearchModel))
                    .SortingModel(CompiledBinding.Create<MRDiffDetectionViewModel, ISortingModel>(x => x.SortingModel))
                    .ContextMenu(new ContextMenu())
                    .Behaviors([new MRDiffDetectionContextMenuBehavior()])
                    );
    }

    private static IServiceProvider GetServiceProvider()
    {
        if (Application.Current is IServiceProviderHost host)
        {
            return host.Services;
        }

        throw new InvalidOperationException("Unable to resolve services from the current application host.");
    }
}
