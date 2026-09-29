using System.Windows.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.VisualTree;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using Ursa.Controls;

namespace KubeUI.Avalonia.Features.Resources.List;

public partial class ResourceListView : ViewBase<IResourceListViewModel>
{
    private DynamicTableView _table = null!;
    private DynamicTableView? _restoredTable;
    private IResourceListViewModel? _restoredViewModel;
    private readonly List<KeyBinding> _actionKeyBindings = [];

    public ResourceListView()
    {
        DesignTimePreview.Run(InitializePreviewDataAsync);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        TryRestoreState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SaveState();
        base.OnDetachedFromVisualTree(e);
    }

    protected override object Build(IResourceListViewModel vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        var table = CreateTable(vm);

        return new Grid()
            .Rows("Auto,*")
            .Children(
                CreateTopBar(),
                table
                    .Name("PART_Grid", Scope)
                    .Ref(out _table)
                    .Row(1)
                    .Styles(vm.ResourceConfig.ListStyle()));
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is not IResourceListViewModel vm)
            return;
        _table.Source = vm.TableSource;
        ConfigureActionBindings(_table, vm);
        _table.ContextMenuItemsFactory = vm.GetContextMenuItems;
        if (VisualRoot is not null)
            TryRestoreState();
    }

    private void SaveState()
    {
        if (_table.Source is not null && DataContext is IResourceListViewModel vm)
        {
            vm.TableViewRuntimeState = _table.CaptureState();
            vm.CaptureSelectionState();
        }
    }

    private DynamicTableView CreateTable(IResourceListViewModel viewModel)
    {
        DynamicTableView table = new()
        {
            GridLinesVisibility = DynamicTableViewGridLinesVisibility.All,
            ContextMenuItemsFactory = viewModel.GetContextMenuItems,
            Source = viewModel.TableSource,
            ContextMenu = CreateContextMenu()
        };
        ConfigureActionBindings(table, viewModel);
        table.DoubleTapped += TableOnDoubleTapped;
        return table;
    }

    private void ConfigureActionBindings(
        DynamicTableView table,
        IResourceListViewModel viewModel)
    {
        foreach (var binding in _actionKeyBindings)
            table.KeyBindings.Remove(binding);
        _actionKeyBindings.Clear();

        var viewBinding = CreateSelectionKeyBinding(Key.Enter, viewModel, viewModel.ResourceConfig.ViewCommand);
        var deleteBinding = CreateSelectionKeyBinding(Key.Delete, viewModel, viewModel.ResourceConfig.DeleteCommand);
        table.KeyBindings.Add(viewBinding);
        table.KeyBindings.Add(deleteBinding);
        _actionKeyBindings.Add(viewBinding);
        _actionKeyBindings.Add(deleteBinding);
    }

    private static KeyBinding CreateSelectionKeyBinding(
        Key key,
        IResourceListViewModel viewModel,
        ICommand command)
    {
        return new KeyBinding
        {
            Gesture = new KeyGesture(key),
            Command = new RelayCommand(
                () => ExecuteForSelection(viewModel, command),
                () => command.CanExecute(GetSelectedItems(viewModel)))
        };
    }

    private static void ExecuteForSelection(IResourceListViewModel viewModel, ICommand command)
    {
        var selectedItems = GetSelectedItems(viewModel);
        if (command.CanExecute(selectedItems))
            command.Execute(selectedItems);
    }

    private static IList GetSelectedItems(IResourceListViewModel viewModel)
        => viewModel.SelectionModel.SelectedItems.OfType<object>().ToList();

    private void TableOnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not IResourceListViewModel viewModel || FindRow(e.Source)?.DataContext is not { } clickedItem)
            return;
        var selectedItems = GetSelectedItems(viewModel);
        if (selectedItems.Count == 0)
            selectedItems.Add(clickedItem);
        ICommand command = viewModel.ResourceConfig.ViewCommand;
        if (command.CanExecute(selectedItems))
            command.Execute(selectedItems);
    }

    private static TableViewRow? FindRow(object? source)
        => source is Visual visual ? visual.GetSelfAndVisualAncestors().OfType<TableViewRow>().FirstOrDefault() : null;

    private void TryRestoreState()
    {
        if (DataContext is not IResourceListViewModel vm)
            return;

        var restoreTableState = !ReferenceEquals(_restoredTable, _table) || !ReferenceEquals(_restoredViewModel, vm);
        var state = restoreTableState ? vm.TableViewRuntimeState : null;
        if (restoreTableState)
        {
            _restoredTable = _table;
            _restoredViewModel = vm;
        }

        if (state is null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (ReferenceEquals(DataContext, vm) && VisualRoot is not null && ReferenceEquals(_table.Source, vm.TableSource))
                    vm.RestoreSelectionState();
            }, DispatcherPriority.Background);
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (ReferenceEquals(DataContext, vm) && VisualRoot is not null && ReferenceEquals(_table.Source, vm.TableSource))
            {
                _table.RestoreState(state);
                vm.RestoreSelectionState();
            }
        }, DispatcherPriority.Background);
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
                            .Text(CompiledBinding.Create<IResourceListViewModel, string>(x => x.SearchQuery, mode: BindingMode.TwoWay)),
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
        FuncDataTemplate<V1Namespace> template = new((ns, _) => new TextBlock().Text(ns?.Metadata?.Name ?? string.Empty));
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
        return new ContextMenu()
            .Styles(
                new Style<MenuItem>()
                    .Setter(IsVisibleProperty, CompiledBinding.Create<MenuItemViewModel, bool>(x => x.IsVisible))
                    .Setter(HeaderedItemsControl.HeaderProperty, CompiledBinding.Create<MenuItemViewModel, string?>(x => x.Title))
                    .Setter(MenuItem.CommandProperty, CompiledBinding.Create<MenuItemViewModel, ICommand?>(x => x.Command))
                    .Setter(MenuItem.CommandParameterProperty, CompiledBinding.Create<MenuItemViewModel, object?>(x => x.CommandParameter))
                    .Setter(ItemsControl.ItemsSourceProperty, CompiledBinding.Create<MenuItemViewModel, IEnumerable?>(x => x.Items))
                    .Setter(TagProperty, CompiledBinding.Create<MenuItemViewModel, bool>(x => x.IsSeparator))
                    .Setter(MenuItem.IconProperty,
                        CompiledBinding.Create<MenuItemViewModel, MenuItemViewModel>(x => x, converter: ResourceMenuItemIconConverter.Instance)),
                new Style<MenuItem>(x => x.PropertyEquals(TagProperty, true))
                    .Setter(TemplatedControl.TemplateProperty, new FuncControlTemplate((_, _) => new Separator())));
    }

    private async Task InitializePreviewDataAsync()
    {
        DataContext = await DesignTimePreview.CreateClusterBoundViewModelAsync<ResourceListViewModel<V1Pod>, V1Pod>();
    }
}
