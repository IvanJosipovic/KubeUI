using System.Windows.Input;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Controls.DependencyInjection;
using KubeUI.Controls.Layout;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Clusters.Presentation;
using KubeUI.Features.Resources.Infrastructure;
using Ursa.Controls;

namespace KubeUI.Features.Resources.List;

public partial class ResourceListView : KubeUIView<IResourceListViewModel>
{
    private DynamicTableView _table = null!;
    private readonly TableStateRestoreCoordinator<DynamicTableViewState> _stateRestore = new();
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
        var table = new DynamicTableView
        {
            ContextMenuItemsFactory = viewModel.GetContextMenuItems
        }
            .GridLinesVisibility(DynamicTableViewGridLinesVisibility.All)
            .Source(viewModel.TableSource)
            .ContextMenu(MenuPresenter.CreateContextMenu());
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
        => SelectionKeyBindingFactory.Create(key, command, () => GetSelectedItems(viewModel));

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

        _stateRestore.Restore(
            _table,
            vm,
            vm.TableViewRuntimeState,
            () => ReferenceEquals(DataContext, vm)
                && VisualRoot is not null
                && ReferenceEquals(_table.Source, vm.TableSource),
            _table.RestoreState,
            vm.RestoreSelectionState);
    }

    private static CompactToolbar CreateTopBar()
    {
        return new CompactToolbar
        {
            Leading = new Button()
                .Flyout(CreateNewResourceFlyout())
                .IsVisible(CompiledBinding.Create<IResourceListViewModel, bool>(x => x.ResourceConfig.ShowNewResource))
                .ToolTip_Tip(ResourcesLanguageManager.Instance.ResourceListView_NewResource.ToBinding())
                .Content(new FluentIcon().Icon(Icon.AddSquare)),
            Middle = new Label()
                .Content(CompiledBinding.Create<IResourceListViewModel, int>(x => x.ItemCount,
                    stringFormat: ResourcesLanguageManager.Instance.ResourceListView_ItemsFormat.CurrentValue()!)),
            Trailing = CompactToolbar.Group(
                    new TextBox()
                        .Width(200)
                        .MinWidth(120)
                        .Background(Brushes.Transparent)
                        .PlaceholderText(ResourcesLanguageManager.Instance.ResourceListView_SearchWatermark.ToBinding())
                        .Text(CompiledBinding.Create<IResourceListViewModel, string>(x => x.SearchQuery, mode: BindingMode.TwoWay)),
                    CreateNamespaceSelector())
                .MaxWidth(456)
        }.Row(0);
    }

    private static NamespaceScopePicker CreateNamespaceSelector()
    {
        return new NamespaceScopePicker()
            .IsVisible(CompiledBinding.Create<IResourceListViewModel, bool>(x => x.ResourceConfig.IsNamespaced))
            .BindValue(NamespaceScopePicker.NamespacesProperty, CompiledBinding.Create<IResourceListViewModel, IEnumerable>(x => x.Cluster.Runtime.Namespaces, mode: BindingMode.OneWay))
            .BindValue(NamespaceScopePicker.SelectedNamespacesProperty, CompiledBinding.Create<IResourceListViewModel, IList>(x => x.SelectedNamespaces, mode: BindingMode.OneWay))
            .BindValue(NamespaceScopePicker.IsSelectionLinkedProperty, CompiledBinding.Create<IResourceListViewModel, bool>(x => x.IsNamespaceSelectionLinked, mode: BindingMode.TwoWay));
    }

    private async Task InitializePreviewDataAsync()
    {
        DataContext = await DesignTimePreview.CreateClusterBoundViewModelAsync<ResourceListViewModel<V1Pod>, V1Pod>();
    }

    internal static MenuFlyout CreateNewResourceFlyout()
    {
        MenuFlyout flyout = new();
        flyout.Items.Add(new MenuItem()
            .Header(ResourcesLanguageManager.Instance.ResourceListView_NewResourceYaml.CurrentValue()!)
            .Icon(new FluentIcon().Icon(Icon.DocumentEdit))
            .Command(CompiledBinding.Create<IResourceListViewModel, ICommand?>(x => x.ResourceConfig.NewResourceCommand)));
        flyout.Items.Add(new MenuItem()
            .Header(ResourcesLanguageManager.Instance.ResourceListView_NewResourceUiEditor.CurrentValue()!)
            .Icon(new FluentIcon().Icon(Icon.Edit))
            .Command(CompiledBinding.Create<IResourceListViewModel, ICommand?>(x => x.ResourceConfig.NewResourceUiEditorCommand)));
        return flyout;
    }

}
