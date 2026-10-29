using System.Collections;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Markup.Declarative;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Cilium.Hubble;
using SvcSystems.Avalonia.DynamicTableView;
using UrsaControls = Ursa.Controls;

namespace KubeUI.Avalonia.Features.Cilium.Hubble;

public sealed partial class FlowListView : ViewBase<FlowListViewModel>
{
    private DynamicTableView _table = null!;
    private DynamicTableView? _restoredTable;
    private FlowListViewModel? _restoredViewModel;

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

    protected override object Build(FlowListViewModel vm)
    {
        var table = CreateTable(vm);

        return new Grid()
            .Rows("Auto,*")
            .Children(
                CreateTopBar(vm),
                table
                    .Name("PART_Grid", Scope)
                    .Ref(out _table)
                    .Row(1));
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is not FlowListViewModel vm)
            return;

        _table.Source = vm.TableSource;
        _table.ContextMenuItemsFactory = vm.GetContextMenuItems;

        if (VisualRoot is not null)
            TryRestoreState();
    }

    private void SaveState()
    {
        if (_table.Source is not null && DataContext is FlowListViewModel vm)
        {
            vm.TableViewRuntimeState = _table.CaptureState();
            vm.CaptureSelectionState();
        }
    }

    private DynamicTableView CreateTable(FlowListViewModel viewModel)
    {
        var table = new DynamicTableView
        {
            GridLinesVisibility = DynamicTableViewGridLinesVisibility.All,
            ContextMenuItemsFactory = viewModel.GetContextMenuItems,
            Source = viewModel.TableSource,
            ContextMenu = CreateContextMenu()
        };
        return table;
    }

    private void TryRestoreState()
    {
        if (DataContext is not FlowListViewModel vm)
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

    private static Grid CreateTopBar(FlowListViewModel vm)
    {
        return new Grid()
            .Row(0)
            .Classes("toolbar")
            .Margin(0, 2, 0, 2)
            .MinHeight(32)
            .Cols("Auto,Auto,*,Auto")
            .Children(
                new Button()
                    .Col(0)
                    .Margin(2, 0, 0, 0)
                    .Command(vm.ClearCommand)
                    .ToolTip_Tip(Assets.Resources.FlowListView_TooltipClear)
                    .Content(new FluentIcon().Icon(Icon.Delete)),
                new Label()
                    .Col(1)
                    .Margin(2, 0, 0, 0)
                    .VerticalContentAlignment(VerticalAlignment.Center)
                    .Content(CompiledBinding.Create<FlowListViewModel, int>(x => x.FlowCount,
                        stringFormat: Assets.Resources.FlowListView_ItemsFormat)),
                new TextBlock()
                    .Col(2)
                    .Margin(8, 0)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Text(CompiledBinding.Create<FlowListViewModel, string>(x => x.ConnectionStatus))
                    .Foreground(CompiledBinding.Create<FlowListViewModel, bool>(x => x.IsConnected,
                        converter: new BoolToBrushConverter(
                            new DynamicResourceExtension("PodStatusReadyBrush"),
                            new DynamicResourceExtension("ContainerStatusErrorBrush")))),
                new StackPanel()
                    .Col(3)
                    .MaxWidth(940)
                    .HorizontalAlignment(HorizontalAlignment.Right)
                    .Orientation(Orientation.Horizontal)
                    .Children(
                        new TextBox()
                            .Width(200)
                            .MinWidth(120)
                            .Margin(0, 0, 2, 0)
                            .VerticalAlignment(VerticalAlignment.Stretch)
                            .VerticalContentAlignment(VerticalAlignment.Center)
                            .Background(Brushes.Transparent)
                            .PlaceholderText(Assets.Resources.FlowListView_SearchWatermark)
                            .Text(CompiledBinding.Create<FlowListViewModel, string>(x => x.SearchQuery, mode: BindingMode.TwoWay)),
                        CreateNamespaceSelector(vm),
                        new ToggleButton()
                            .Margin(0, 0, 2, 0)
                            .IsChecked(CompiledBinding.Create<FlowListViewModel, bool>(x => x.IsNamespaceSelectionLinked, source: vm, mode: BindingMode.TwoWay))
                            .ToolTip_Tip(Assets.Resources.ResourceListView_NamespaceLink)
                            .Content(new FluentIcon().Icon(Icon.Link))));
    }

    private static UrsaControls.MultiComboBox CreateNamespaceSelector(FlowListViewModel viewModel)
    {
        var template = new FuncDataTemplate<V1Namespace>((ns, _) =>
            new TextBlock().Text(ns?.Metadata?.Name ?? string.Empty));

        return new UrsaControls.MultiComboBox()
            .Width(190)
            .MinWidth(140)
            .MaxHeight(20)
            .Margin(0, 0, 2, 0)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Classes("ClearButton")
            .ItemsSource(CompiledBinding.Create<FlowListViewModel, IEnumerable>(x => x.Cluster!.Runtime.Namespaces, source: viewModel))
            .PlaceholderText(Assets.Resources.ResourceListView_SelectNamespace)
            .SelectedItems(CompiledBinding.Create<FlowListViewModel, IList>(x => x.SelectedNamespaces, source: viewModel))
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
                    .Setter(TagProperty, CompiledBinding.Create<MenuItemViewModel, bool>(x => x.IsSeparator)),
                new Style<MenuItem>(x => x.PropertyEquals(TagProperty, true))
                    .Setter(TemplatedControl.TemplateProperty, new FuncControlTemplate((_, _) => new Separator())));
    }
}

file sealed class BoolToBrushConverter : IValueConverter
{
    private readonly object _trueValue;
    private readonly object _falseValue;

    public BoolToBrushConverter(object trueValue, object falseValue)
    {
        _trueValue = trueValue;
        _falseValue = falseValue;
    }

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        var selected = value is true ? _trueValue : _falseValue;
        return selected is DynamicResourceExtension dre
            ? (Application.Current?.TryGetResource(dre.ResourceKey, Application.Current.ActualThemeVariant, out var r) == true ? r : selected)
            : selected;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}
