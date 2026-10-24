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
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Cilium.Hubble;
using SvcSystems.Avalonia.DynamicTableView;
using System.Windows.Input;
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
        if (DataContext is not FlowListViewModel vm) return;

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
        if (DataContext is not FlowListViewModel vm) return;

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
            .MinHeight(32)
            .Margin(0, 2, 0, 2)
            .Cols("Auto,Auto,*,Auto")
            .Children(
                new Button()
                    .Col(0)
                    .Margin(2, 0, 0, 0)
                    .Command(vm.ClearCommand)
                    .ToolTip_Tip(Assets.Resources.FlowListView_TooltipClear)
                    .Content(new TextBlock().Text(Assets.Resources.FlowListView_ButtonClear)),

                new Label()
                    .Col(1)
                    .Margin(2, 0, 0, 0)
                    .VerticalContentAlignment(VerticalAlignment.Center)
                    .Content(CompiledBinding.Create<FlowListViewModel, int>(x => x.FlowCount,
                        stringFormat: Assets.Resources.FlowListView_ItemsFormat)),

                new StackPanel()
                    .Col(3)
                    .MaxWidth(600)
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
                            .PlaceholderText(Assets.Resources.FlowListView_SearchWatermark)
                            .Text(CompiledBinding.Create<FlowListViewModel, string>(x => x.SearchQuery, mode: BindingMode.TwoWay)),

                        new ComboBox()
                            .Width(120)
                            .Margin(0, 0, 2, 0)
                            .ItemsSource(new[] { KubeUI.Cilium.Hubble.FlowVerdict.Forwarded, KubeUI.Cilium.Hubble.FlowVerdict.Dropped, KubeUI.Cilium.Hubble.FlowVerdict.Error })
                            .SelectedItem(CompiledBinding.Create<FlowListViewModel, KubeUI.Cilium.Hubble.FlowVerdict?>(x => x.Filter.Verdict, mode: BindingMode.TwoWay))
                            .PlaceholderText(Assets.Resources.FlowListView_FilterVerdictAll)
                            .ItemTemplate(new FuncDataTemplate<KubeUI.Cilium.Hubble.FlowVerdict>((v, _) => new TextBlock().Text(v switch
                            {
                                KubeUI.Cilium.Hubble.FlowVerdict.Forwarded => Assets.Resources.FlowVerdict_Allowed,
                                KubeUI.Cilium.Hubble.FlowVerdict.Dropped => Assets.Resources.FlowVerdict_Dropped,
                                KubeUI.Cilium.Hubble.FlowVerdict.Error => Assets.Resources.FlowVerdict_Error,
                                _ => Assets.Resources.FlowVerdict_Unknown
                            }))),

                        new ComboBox()
                            .Width(100)
                            .Margin(0, 0, 2, 0)
                            .ItemsSource(new[] { KubeUI.Cilium.Hubble.FlowDirection.Ingress, KubeUI.Cilium.Hubble.FlowDirection.Egress })
                            .SelectedItem(CompiledBinding.Create<FlowListViewModel, KubeUI.Cilium.Hubble.FlowDirection?>(x => x.Filter.Direction, mode: BindingMode.TwoWay))
                            .PlaceholderText(Assets.Resources.FlowListView_FilterDirectionAll)
                            .ItemTemplate(new FuncDataTemplate<KubeUI.Cilium.Hubble.FlowDirection>((d, _) => new TextBlock().Text(d switch
                            {
                                KubeUI.Cilium.Hubble.FlowDirection.Ingress => Assets.Resources.FlowDirection_Ingress,
                                KubeUI.Cilium.Hubble.FlowDirection.Egress => Assets.Resources.FlowDirection_Egress,
                                _ => Assets.Resources.FlowDirection_Unknown
                            }))),

                        new NumericUpDown()
                            .Width(80)
                            .Margin(0, 0, 2, 0)
                            .Minimum(1)
                            .Maximum(65535)
                            .Value(CompiledBinding.Create<FlowListViewModel, int?>(x => x.Filter.DestinationPort, mode: BindingMode.TwoWay))
                            .PlaceholderText(Assets.Resources.FlowListView_FilterPortWatermark),

                        new CheckBox()
                            .Margin(0, 0, 2, 0)
                            .Content(Assets.Resources.FlowListView_FilterErrorsOnly)
                            .IsChecked(CompiledBinding.Create<FlowListViewModel, bool>(x => x.Filter.ShowOnlyErrors, mode: BindingMode.TwoWay)),

                        new Button()
                            .Margin(0, 0, 2, 0)
                            .Content(Assets.Resources.FlowListView_ButtonResetFilter)
                            .Command(vm.ResetFilterCommand),

                        new ProgressBar()
                            .Width(20)
                            .Height(20)
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .IsIndeterminate(true)
                            .IsVisible(CompiledBinding.Create<FlowListViewModel, bool>(x => !x.IsConnected)),

                        new TextBlock()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Margin(8, 0, 0, 0)
                            .Text(CompiledBinding.Create<FlowListViewModel, string>(x => x.ConnectionStatus))
                            .Foreground(CompiledBinding.Create<FlowListViewModel, bool>(x => x.IsConnected,
                                converter: new BoolToBrushConverter(
                                    new DynamicResourceExtension("PodStatusReadyBrush"),
                                    new DynamicResourceExtension("ContainerStatusErrorBrush")))))
            );
    }

    private static ContextMenu CreateContextMenu()
    {
        return new ContextMenu()
            .Styles(
                new Style<MenuItem>()
                    .Setter(MenuItem.IsVisibleProperty, CompiledBinding.Create<MenuItemViewModel, bool>(x => x.IsVisible))
                    .Setter(HeaderedItemsControl.HeaderProperty, CompiledBinding.Create<MenuItemViewModel, string?>(x => x.Title))
                    .Setter(MenuItem.CommandProperty, CompiledBinding.Create<MenuItemViewModel, ICommand?>(x => x.Command))
                    .Setter(MenuItem.CommandParameterProperty, CompiledBinding.Create<MenuItemViewModel, object?>(x => x.CommandParameter))
                    .Setter(ItemsControl.ItemsSourceProperty, CompiledBinding.Create<MenuItemViewModel, IEnumerable?>(x => x.Items))
                    .Setter(TagProperty, CompiledBinding.Create<MenuItemViewModel, bool>(x => x.IsSeparator)),
                new Style<MenuItem>(x => x.PropertyEquals(TagProperty, true))
                    .Setter(TemplatedControl.TemplateProperty, new FuncControlTemplate((_, _) => new Separator())));
    }
}

file sealed class BoolToBrushConverter : global::Avalonia.Data.Converters.IValueConverter
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
