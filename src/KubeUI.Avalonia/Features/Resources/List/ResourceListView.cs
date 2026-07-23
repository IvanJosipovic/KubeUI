using System.Globalization;
using System.Windows.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Styling;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Avalonia.Controls.DataGridFilters;
using KubeUI.Avalonia.Converters;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Features.Resources.List.Behaviors;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Resources;
using Ursa.Controls;
using KubeUI.Avalonia.Infrastructure;

namespace KubeUI.Avalonia.Features.Resources.List;

public partial class ResourceListView : ViewBase<IResourceListViewModel>
{
    private DataGrid _grid;
    private DataGridColumnFilterFlyoutFactory? _filterFlyoutFactory;

    public ResourceListView()
    {
        DesignTimePreview.Run(InitializePreviewDataAsync);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (_grid is DataGrid grid && DataContext is IResourceListViewModel vm && vm.DataGridRuntimeState is { } state)
        {
            grid.RestoreState(state, DataGridStateSections.All, CreateStateOptions(grid));
        }
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
        vm.DataGridRuntimeState = state;
    }

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
                CreateTopBar(vm),
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
                    .ColumnDefinitionsSource(vm, x => x.ColumnDefinitions)
                    .FilteringModel(vm, x => x.FilteringModel)
                    .GridLinesVisibility(DataGridGridLinesVisibility.All)
                    .IsReadOnly(true)
                    .ItemsSource(vm, x => x.View)
                    .SearchModel(vm, x => x.SearchModel)
                    .Selection(vm, x => x.SelectionModel)
                    .SelectionMode(DataGridSelectionMode.Extended)
                    .SortingModel(vm, x => x.SortingModel)
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

    private static Grid CreateTopBar(IResourceListViewModel vm)
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
                    .Command(vm, x => x.ResourceConfig.NewResourceCommand)
                    .IsVisible(vm, x => x.ResourceConfig.ShowNewResource)
                    .ToolTip_Tip(Assets.Resources.ResourceListView_NewResource)
                    .Content(new FluentIcon().Icon(Icon.AddSquare)),
                new Label()
                    .Col(1)
                    .Margin(2, 0, 0, 0)
                    .VerticalContentAlignment(VerticalAlignment.Center)
                    .BindValue(ContentControl.ContentProperty, CompiledBinding.Create<IResourceListViewModel, int>(x => x.ItemCount,
                        source: vm,
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
                            .Text(vm, x => x.SearchQuery, BindingMode.TwoWay),
                        CreateNamespaceSelector(vm),
                        new ToggleButton()
                            .Margin(0, 0, 2, 0)
                            .IsChecked(vm, x => x.IsNamespaceSelectionLinked, BindingMode.TwoWay)
                            .IsVisible(vm, x => x.ResourceConfig.IsNamespaced)
                            .ToolTip_Tip(Assets.Resources.ResourceListView_NamespaceLink)
                            .Content(new FluentIcon().Icon(Icon.Link))));
    }

    private static MultiComboBox CreateNamespaceSelector(IResourceListViewModel vm)
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
            .IsVisible(vm, x => x.ResourceConfig.IsNamespaced)
            .ItemsSource(vm, x => x.Cluster.Runtime.Namespaces)
            .PlaceholderText(Assets.Resources.ResourceListView_SelectNamespace)
            .SelectedItems(vm, x => x.SelectedNamespaces)
            .ItemTemplate(template)
            .SelectedItemTemplate(template);
    }

    private static ContextMenu CreateContextMenu()
    {
        var contextMenu = new ContextMenu()
            .Styles(
                new Style<MenuItem>()
                    .Setter(MenuItem.IsVisibleProperty, CompiledBinding.Create<MenuItemViewModel, bool>(x => x.IsVisible))
                    .Setter(HeaderedItemsControl.HeaderProperty, CompiledBinding.Create<MenuItemViewModel, string?>(x => x.Header))
                    .Setter(MenuItem.CommandProperty, CompiledBinding.Create<MenuItemViewModel, ICommand?>(x => x.Command))
                    .Setter(MenuItem.CommandParameterProperty, CompiledBinding.Create<MenuItemViewModel, object?>(x => x.CommandParameter))
                    .Setter(ItemsControl.ItemsSourceProperty, CompiledBinding.Create<MenuItemViewModel, IEnumerable?>(x => x.Items))
                    .Setter(Control.TagProperty, CompiledBinding.Create<MenuItemViewModel, bool>(x => x.IsSeparator))
                    .Setter(MenuItem.IconProperty, new Binding { Converter = MenuItemIconConverter.Instance }),
                new Style<MenuItem>(x => x.PropertyEquals(Control.TagProperty, true))
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

            if (item.FluentIcon is { } fluentIcon)
            {
                return new FluentIcon().Icon(fluentIcon);
            }

            if (!string.IsNullOrWhiteSpace(item.IconResource))
            {
                var data = StaticResourceConverter.Instance.Convert(item.IconResource, typeof(object), parameter, culture);
                if (data != AvaloniaProperty.UnsetValue)
                {
                    return new PathIcon { Data = data as Geometry };
                }
            }

            return null;
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
            //_grid.SortingModel = vm.SortingModel;
            //_grid.FilteringModel = vm.FilteringModel;
            //_grid.SearchModel = vm.SearchModel;
            //_grid.Selection = vm.SelectionModel;

            AttachFilterFlyouts(vm);
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
