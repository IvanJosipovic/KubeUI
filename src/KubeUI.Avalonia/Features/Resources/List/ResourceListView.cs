using System.Globalization;
using System.Windows.Input;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Threading;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Avalonia.Controls.DataGridFilters;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Features.Resources.List.Behaviors;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Resources;
using Ursa.Controls;

namespace KubeUI.Avalonia.Features.Resources.List;

public partial class ResourceListView : ViewBase<IResourceListViewModel>
{
    private const DataGridStateSections RestoredStateSections =
        DataGridStateSections.All & ~DataGridStateSections.Searching & ~DataGridStateSections.Scroll;

    private DataGrid _grid;
    private DataGrid? _stateRestoredGrid;
    private IResourceListViewModel? _stateRestoredViewModel;
    private DataGridColumnFilterFlyoutFactory? _filterFlyoutFactory;

    public ResourceListView()
    {
        DesignTimePreview.Run(InitializePreviewDataAsync);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        TryScheduleStateRestore();
    }

    private void TryScheduleStateRestore()
    {
        if (_grid is not DataGrid grid ||
            DataContext is not IResourceListViewModel vm ||
            vm.DataGridRuntimeState is not { } state ||
            (ReferenceEquals(_stateRestoredGrid, grid) && ReferenceEquals(_stateRestoredViewModel, vm)))
        {
            return;
        }

        // Mark only pairs with state to restore. The first attachment normally
        // has no snapshot; it must remain eligible after the first detach.
        _stateRestoredGrid = grid;
        _stateRestoredViewModel = vm;

        Dispatcher.UIThread.Post(
            static state =>
            {
                var (view, grid, vm, dataGridState) = ((ResourceListView View, DataGrid Grid, IResourceListViewModel ViewModel, DataGridState State))state!;
                if (view._grid == grid &&
                    ReferenceEquals(view.DataContext, vm) &&
                    view.VisualRoot is not null)
                {
                    grid.RestoreState(
                        PrepareStateForRestore(grid, dataGridState),
                        RestoredStateSections,
                        CreateStateOptions(grid));
                }
            },
            (this, grid, vm, state),
            DispatcherPriority.Loaded);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SaveDataGridState();
        base.OnDetachedFromVisualTree(e);
    }

    private void SaveDataGridState()
    {
        if (_grid is not DataGrid grid || DataContext is not IResourceListViewModel vm)
        {
            return;
        }

        var state = grid.CaptureState(DataGridStateSections.All, CreateStateOptions(grid));
        vm.DataGridRuntimeState = RemoveNamespaceScopeFilter(state);
    }

    private static DataGridState RemoveNamespaceScopeFilter(DataGridState state)
    {
        if (state.Filtering?.Descriptors is not { } descriptors)
        {
            return state;
        }

        var filteredDescriptors = descriptors
            .Where(descriptor => !IsNamespaceScopeFilter(descriptor))
            .ToArray();

        if (filteredDescriptors.Length == descriptors.Count)
        {
            return state;
        }

        state.Filtering = new DataGridFilteringState
        {
            Descriptors = filteredDescriptors,
            OwnsViewFilter = state.Filtering.OwnsViewFilter
        };

        return state;
    }

    private static DataGridState PrepareStateForRestore(DataGrid grid, DataGridState state)
    {
        var sanitizedState = RemoveNamespaceScopeFilter(state);
        IReadOnlyList<FilteringDescriptor> currentNamespaceDescriptors = grid.FilteringModel.Descriptors
            .Where(IsNamespaceScopeFilter)
            .ToArray();

        if (currentNamespaceDescriptors.Count == 0 || sanitizedState.Filtering is not { } filteringState)
        {
            return sanitizedState;
        }

        filteringState.Descriptors = filteringState.Descriptors
            .Concat(currentNamespaceDescriptors)
            .ToArray();
        return sanitizedState;
    }

    private static bool IsNamespaceScopeFilter(FilteringDescriptor descriptor)
        => string.Equals(
            descriptor.ColumnId?.ToString(),
            ResourceListViewModel<V1Pod>.NamespaceScopeFilterId,
            StringComparison.Ordinal);

    private static DataGridStateOptions CreateStateOptions(DataGrid grid) => new()
    {
        ColumnKeySelector = column => column.ColumnKey?.ToString(),
        ColumnKeyResolver = key => grid.Columns.FirstOrDefault(column =>
            string.Equals(column.ColumnKey?.ToString(), key?.ToString(), StringComparison.Ordinal))
    };

    protected override object Build(IResourceListViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        var grid = new Grid()
            .Rows("Auto,*")
            .Children(
                CreateTopBar(),
                new DataGrid
                {
                    ReferenceIndexResolver = vm.ReferenceIndexResolver,
                    SortingAdapterFactory = vm.SortingAdapterFactory,
                    FilteringAdapterFactory = vm.FilteringAdapterFactory,
                    SearchAdapterFactory = vm.SearchAdapterFactory,
                }
                    .Name("PART_Grid")
                    .Ref(out _grid)
                    .Row(1)
                    .CanUserReorderColumns(true)
                    .CanUserResizeColumns(true)
                    .CanUserSortColumns(true)
                    .ColumnDefinitionsSource(CompiledBinding.Create<IResourceListViewModel, IList<DataGridColumnDefinition>>(x => x.ColumnDefinitions))
                    .FilteringModel(CompiledBinding.Create<IResourceListViewModel, IFilteringModel>(x => x.FilteringModel))
                    .GridLinesVisibility(DataGridGridLinesVisibility.All)
                    .IsReadOnly(true)
                    .ItemsSource(CompiledBinding.Create<IResourceListViewModel, IList>(x => x.View))
                    .SearchModel(CompiledBinding.Create<IResourceListViewModel, ISearchModel>(x => x.SearchModel))
                    .Selection(CompiledBinding.Create<IResourceListViewModel, ISelectionModel>(x => x.SelectionModel))
                    .SelectionMode(DataGridSelectionMode.Extended)
                    .SortingModel(CompiledBinding.Create<IResourceListViewModel, ISortingModel>(x => x.SortingModel))
                    .UseLogicalScrollable(true)
                    .ContextMenu(CreateContextMenu())
                    .RowHeightEstimator(new DefaultRowHeightEstimator())
                    .Behaviors([
                        new ResourceListDoubleTapBehavior(),
                        new ResourceListContextMenuBehavior()
                        ])
                    .KeyBindings(
                        new KeyBinding
                        {
                            Command = vm.ResourceConfig.ViewCommand,
                            CommandParameter = vm.SelectionModel.SelectedItems,
                            Gesture = new KeyGesture(Key.Enter)
                        },
                        new KeyBinding
                        {
                            Command = vm.ResourceConfig.DeleteCommand,
                            CommandParameter = vm.SelectionModel.SelectedItems,
                            Gesture = new KeyGesture(Key.Delete)
                        }))
                    .Styles(vm.ResourceConfig.ListStyle());


        Scope.Register("PART_Grid", _grid); //todo why is this needed

        return grid;
    }

    private static Grid CreateTopBar()
    {
        return new Grid()
            .Row(0)
            .MinHeight(32)
            .Margin(0, 2, 0, 2)
            .Cols("Auto,Auto,*,Auto")
            .Children(
                new Button()
                    .Col(0)
                    .Margin(2, 0, 0, 0)
                    .Command(CompiledBinding.Create<IResourceListViewModel, ICommand?>(x => x.ResourceConfig.NewResourceCommand))
                    .IsVisible(CompiledBinding.Create<IResourceListViewModel, bool>(x => x.ResourceConfig.ShowNewResource))
                    .ToolTip_Tip(Assets.Resources.ResourceListView_NewResource)
                    .Content(new FluentIcon().Icon(Icon.AddSquare)),
                new Label()
                    .Col(1)
                    .Margin(2, 0, 0, 0)
                    .VerticalContentAlignment(VerticalAlignment.Center)
                    .Content(CompiledBinding.Create<IResourceListViewModel, int>(x => x.ItemCount,
                        stringFormat: Assets.Resources.ResourceListView_ItemsFormat)),
                new StackPanel()
                    .Col(3)
                    .MaxWidth(456)
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
                            .Text(CompiledBinding.Create<IResourceListViewModel, string?>(x => x.SearchQuery, mode: BindingMode.TwoWay)),
                        CreateNamespaceSelector(),
                        new ToggleButton()
                            .Margin(0, 0, 2, 0)
                            .IsChecked(CompiledBinding.Create<IResourceListViewModel, bool>(x => x.IsNamespaceSelectionLinked, mode: BindingMode.TwoWay))
                            .IsVisible(CompiledBinding.Create<IResourceListViewModel, bool>(x => x.ResourceConfig.IsNamespaced))
                            .ToolTip_Tip(Assets.Resources.ResourceListView_NamespaceLink)
                            .Content(new FluentIcon().Icon(Icon.Link))));
    }

    private static MultiComboBox CreateNamespaceSelector()
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
            .IsVisible(CompiledBinding.Create<IResourceListViewModel, bool>(x => x.ResourceConfig.IsNamespaced))
            .ItemsSource(CompiledBinding.Create<IResourceListViewModel, IEnumerable>(x => x.Cluster.Runtime.Namespaces))
            .PlaceholderText(Assets.Resources.ResourceListView_SelectNamespace)
            .SelectedItems(CompiledBinding.Create<IResourceListViewModel, IList>(x => x.SelectedNamespaces))
            .ItemTemplate(template)
            .SelectedItemTemplate(template);
    }

    private static ContextMenu CreateContextMenu()
    {
        var contextMenu = new ContextMenu()
            .Styles(
                new Style<MenuItem>()
                    .Setter(IsVisibleProperty, CompiledBinding.Create<MenuItemViewModel, bool>(x => x.IsVisible))
                    .Setter(HeaderedItemsControl.HeaderProperty, CompiledBinding.Create<MenuItemViewModel, string?>(x => x.Title))
                    .Setter(MenuItem.CommandProperty, CompiledBinding.Create<MenuItemViewModel, ICommand?>(x => x.Command))
                    .Setter(MenuItem.CommandParameterProperty, CompiledBinding.Create<MenuItemViewModel, object?>(x => x.CommandParameter))
                    .Setter(ItemsControl.ItemsSourceProperty, CompiledBinding.Create<MenuItemViewModel, IEnumerable?>(x => x.Items))
                    .Setter(TagProperty, CompiledBinding.Create<MenuItemViewModel, bool>(x => x.IsSeparator))
                    .Setter(MenuItem.IconProperty,
                        CompiledBinding.Create<MenuItemViewModel, MenuItemViewModel>(
                            x => x,
                            converter: MenuItemIconConverter.Instance)),
                new Style<MenuItem>(x => x.PropertyEquals(TagProperty, true))
                    .Setter(TemplatedControl.TemplateProperty, new FuncControlTemplate((_, _) => new Separator())));

        return contextMenu;
    }

    private sealed class MenuItemIconConverter : IValueConverter
    {
        public static readonly MenuItemIconConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not MenuItemViewModel item)
            {
                return null;
            }

            return ResourceActionPresenter.CreateIcon(item);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    private async Task InitializePreviewDataAsync()
    {
        DataContext = await DesignTimePreview.CreateClusterBoundViewModelAsync<ResourceListViewModel<V1Pod>, V1Pod>();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is IResourceListViewModel vm)
        {
            _grid.ReferenceIndexResolver = vm.ReferenceIndexResolver;
            _grid.SortingAdapterFactory = vm.SortingAdapterFactory;
            _grid.FilteringAdapterFactory = vm.FilteringAdapterFactory;
            _grid.SearchAdapterFactory = vm.SearchAdapterFactory;

            if (_grid.KeyBindings.Count >= 2)
            {
                _grid.KeyBindings[0].Command = vm.ResourceConfig.ViewCommand;
                _grid.KeyBindings[0].CommandParameter = vm.SelectionModel.SelectedItems;
                _grid.KeyBindings[1].Command = vm.ResourceConfig.DeleteCommand;
                _grid.KeyBindings[1].CommandParameter = vm.SelectionModel.SelectedItems;
            }

            AttachFilterFlyouts(vm);
            if (VisualRoot is not null)
            {
                TryScheduleStateRestore();
            }
        }

    }

    private static IResourceListColumn? GetResourceListColumn(DataGridColumnDefinition columnDefinition)
    {
        return columnDefinition.Tag as IResourceListColumn;
    }

    private void AttachFilterFlyouts(IResourceListViewModel vm)
    {
        _filterFlyoutFactory ??= GetServiceProvider().GetRequiredService<DataGridColumnFilterFlyoutFactory>();

        foreach (var column in vm.ColumnDefinitions)
        {
            if (GetResourceListColumn(column) is not IResourceListColumn resourceColumn)
            {
                continue;
            }

            column.FilterFlyout = _filterFlyoutFactory.Create(resourceColumn, column, vm.FilteringModel);
        }
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
