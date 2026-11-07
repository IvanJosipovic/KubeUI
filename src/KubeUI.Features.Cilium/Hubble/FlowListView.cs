using System.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Declarative;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Controls.DependencyInjection;
using KubeUI.Controls.Layout;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Cilium.Hubble;
using KubeUI.Features.Clusters.Presentation;
using SvcSystems.Avalonia.DynamicTableView;

namespace KubeUI.Features.Cilium.Hubble;

public sealed partial class FlowListView : KubeUIView<FlowListViewModel>
{
    private DynamicTableView _table = null!;
    private readonly TableStateRestoreCoordinator<DynamicTableViewState> _stateRestore = new();

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
        return new DynamicTableView
        {
            ContextMenuItemsFactory = viewModel.GetContextMenuItems
        }
            .GridLinesVisibility(DynamicTableViewGridLinesVisibility.All)
            .Source(viewModel.TableSource)
            .ContextMenu(MenuPresenter.CreateContextMenu());
    }

    private void TryRestoreState()
    {
        if (DataContext is not FlowListViewModel vm)
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

    private static CompactToolbar CreateTopBar(FlowListViewModel vm)
    {
        return new CompactToolbar
        {
            Leading = new Button()
                .Command(vm.ClearCommand)
                .ToolTip_Tip(CiliumLanguageManager.Instance.FlowListView_TooltipClear.ToBinding())
                .Content(new FluentIcon().Icon(Icon.Delete)),
            Count = new StackPanel()
                .Orientation(Orientation.Horizontal)
                .Children(
                    new Label()
                        .VerticalContentAlignment(VerticalAlignment.Center)
                        .Content(CompiledBinding.Create<FlowListViewModel, int>(x => x.FlowCount,
                             stringFormat: CiliumLanguageManager.Instance.FlowListView_ItemsFormat.CurrentValue()!)),
                    new TextBlock()
                        .MaxWidth(260)
                        .Margin(8, 0, 0, 0)
                        .VerticalAlignment(VerticalAlignment.Center)
                        .TextWrapping(TextWrapping.Wrap)
                        .Text(CompiledBinding.Create<FlowListViewModel, string>(x => x.ConnectionStatus))
                        .Foreground(CompiledBinding.Create<FlowListViewModel, bool>(x => x.IsConnected,
                            converter: new BoolToBrushConverter(
                            new DynamicResourceExtension("SemanticSuccessBrush"),
                            new DynamicResourceExtension("SemanticErrorBrush"))))),
            Trailing = new StackPanel()
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
                            .PlaceholderText(CiliumLanguageManager.Instance.FlowListView_SearchWatermark.ToBinding())
                            .Text(CompiledBinding.Create<FlowListViewModel, string>(x => x.SearchQuery, mode: BindingMode.TwoWay)),
                        new NamespaceScopePicker { PickerWidth = 190 }
                            .BindValue(NamespaceScopePicker.NamespacesProperty,
                                CompiledBinding.Create<FlowListViewModel, IEnumerable>(x => x.Cluster!.Runtime.Namespaces, source: vm))
                            .BindValue(NamespaceScopePicker.SelectedNamespacesProperty,
                                CompiledBinding.Create<FlowListViewModel, IList>(x => x.SelectedNamespaces, source: vm, mode: BindingMode.OneWay))
                            .BindValue(NamespaceScopePicker.IsSelectionLinkedProperty,
                                CompiledBinding.Create<FlowListViewModel, bool>(x => x.IsNamespaceSelectionLinked, source: vm, mode: BindingMode.TwoWay)))
        }.Row(0);
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
