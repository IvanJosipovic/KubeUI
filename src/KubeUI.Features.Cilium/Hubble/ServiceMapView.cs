using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Declarative;
using Avalonia.Markup.Xaml.MarkupExtensions;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Cilium.Hubble;
using KubeUI.Features.Cilium.Hubble.Controls;
using KubeUI.Features.Clusters.Presentation;
using SvcSystems.Avalonia.DynamicTableView;
using Westermo.GraphX.Common.Enums;

namespace KubeUI.Features.Cilium.Hubble;

public sealed partial class ServiceMapView : KubeUIView<ServiceMapViewModel>
{
    protected override object Build(ServiceMapViewModel vm)
    {
        return new Grid()
            .Rows("Auto,Auto,*")
            .Children(
                CreateTopBar(vm).Row(0),
                CreateSelectedFlows().Row(1),
                new Grid()
                    .Margin(6)
                    .Children(
                        CreateGraphViewer(vm),
                        new TextBlock()
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(CompiledBinding.Create<ServiceMapViewModel, string>(x => x.EmptyStateMessage))
                            .IsVisible(CompiledBinding.Create<ServiceMapViewModel, bool>(x => !x.HasFlows)))
                    .Row(2));
    }

    private static Grid CreateTopBar(ServiceMapViewModel vm)
    {
        return new Grid()
            .Row(0)
            .MinHeight(32)
            .Margin(0, 2, 0, 2)
            .Cols("Auto,Auto,*,Auto")
            .Children(
                new StackPanel()
                    .Col(0)
                    .Orientation(Orientation.Horizontal)
                    .Spacing(6)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Children(
                        new ComboBox()
                            .Width(140)
                            .Height(32)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .ItemsSource(new[] { TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(60) })
                            .SelectedItem(CompiledBinding.Create<ServiceMapViewModel, TimeSpan>(x => x.TimeWindow, mode: BindingMode.TwoWay))
                            .ItemTemplate(new FuncDataTemplate<TimeSpan>((timeWindow, _) => new TextBlock()
                                .Text($"{timeWindow.TotalMinutes} {CiliumLanguageManager.Instance.ServiceMapView_TimeWindowMinutes.CurrentValue()!}"))),
                        new ToggleButton()
                            .Width(32)
                            .Height(32)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .IsChecked(CompiledBinding.Create<ServiceMapViewModel, bool>(x => x.AggregateByNamespace, mode: BindingMode.TwoWay))
                            .ToolTip_Tip(CiliumLanguageManager.Instance.ServiceMapView_TooltipAggregate.ToBinding())
                            .Content(new FluentIcon().Icon(Icon.DataUsage)),
                        new ToggleButton()
                            .Width(32)
                            .Height(32)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .IsChecked(CompiledBinding.Create<ServiceMapViewModel, bool>(x => x.ShowOnlyErrors, mode: BindingMode.TwoWay))
                            .ToolTip_Tip(CiliumLanguageManager.Instance.ServiceMapView_TooltipErrorsOnly.ToBinding())
                            .Content(new FluentIcon().Icon(Icon.Warning))),
                new StackPanel()
                    .Col(1)
                    .Orientation(Orientation.Horizontal)
                    .Spacing(12)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Margin(8, 0, 0, 0)
                    .Children(
                        new TextBlock()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(CompiledBinding.Create<ServiceMapViewModel, string>(x => x.ConnectionStatus)),
                        new TextBlock()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(CompiledBinding.Create<ServiceMapViewModel, string>(x => x.GraphSummary))),
                new StackPanel()
                    .Col(3)
                    .MaxWidth(620)
                    .HorizontalAlignment(HorizontalAlignment.Right)
                    .Orientation(Orientation.Horizontal)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Children(
                        CreateLayoutSelector(vm),
                        new NamespaceScopePicker()
                            .BindValue(NamespaceScopePicker.NamespacesProperty,
                                CompiledBinding.Create<ServiceMapViewModel, IEnumerable>(x => x.Cluster!.Runtime.Namespaces, source: vm))
                            .BindValue(NamespaceScopePicker.SelectedNamespacesProperty,
                                CompiledBinding.Create<ServiceMapViewModel, IList>(x => x.SelectedNamespaces, source: vm, mode: BindingMode.OneWay))
                            .BindValue(NamespaceScopePicker.IsSelectionLinkedProperty,
                                CompiledBinding.Create<ServiceMapViewModel, bool>(x => x.IsNamespaceSelectionLinked, source: vm, mode: BindingMode.TwoWay))));
    }

    private static ComboBox CreateLayoutSelector(ServiceMapViewModel viewModel)
    {
        var algorithms = Enum.GetValues<LayoutAlgorithmTypeEnum>()
            .Where(static algorithm => algorithm != LayoutAlgorithmTypeEnum.Custom)
            .ToArray();
        return new ComboBox()
            .Width(148)
            .Height(32)
            .ItemsSource(algorithms)
            .SelectedItem(CompiledBinding.Create<ServiceMapViewModel, LayoutAlgorithmTypeEnum>(
                x => x.LayoutAlgorithm,
                source: viewModel,
                mode: BindingMode.TwoWay))
            .ItemTemplate(new FuncDataTemplate<LayoutAlgorithmTypeEnum>((algorithm, _) =>
                new TextBlock().Text(GetLayoutAlgorithmName(algorithm))))
            .ToolTip_Tip(CiliumLanguageManager.Instance.ServiceMapView_LayoutAlgorithm.CurrentValue()!);
    }

    private static string GetLayoutAlgorithmName(LayoutAlgorithmTypeEnum algorithm)
    {
        return algorithm switch
        {
            LayoutAlgorithmTypeEnum.BoundedFR => CiliumLanguageManager.Instance.ServiceMapView_Layout_BoundedFR.CurrentValue()!,
            LayoutAlgorithmTypeEnum.Circular => CiliumLanguageManager.Instance.ServiceMapView_Layout_Circular.CurrentValue()!,
            LayoutAlgorithmTypeEnum.CompoundFDP => CiliumLanguageManager.Instance.ServiceMapView_Layout_CompoundFDP.CurrentValue()!,
            LayoutAlgorithmTypeEnum.EfficientSugiyama => CiliumLanguageManager.Instance.ServiceMapView_Layout_EfficientSugiyama.CurrentValue()!,
            LayoutAlgorithmTypeEnum.FR => CiliumLanguageManager.Instance.ServiceMapView_Layout_FR.CurrentValue()!,
            LayoutAlgorithmTypeEnum.ISOM => CiliumLanguageManager.Instance.ServiceMapView_Layout_ISOM.CurrentValue()!,
            LayoutAlgorithmTypeEnum.KK => CiliumLanguageManager.Instance.ServiceMapView_Layout_KK.CurrentValue()!,
            LayoutAlgorithmTypeEnum.LinLog => CiliumLanguageManager.Instance.ServiceMapView_Layout_LinLog.CurrentValue()!,
            LayoutAlgorithmTypeEnum.SimpleRandom => CiliumLanguageManager.Instance.ServiceMapView_Layout_Random.CurrentValue()!,
            LayoutAlgorithmTypeEnum.Sugiyama => CiliumLanguageManager.Instance.ServiceMapView_Layout_Sugiyama.CurrentValue()!,
            LayoutAlgorithmTypeEnum.Tree => CiliumLanguageManager.Instance.ServiceMapView_Layout_Tree.CurrentValue()!,
            _ => algorithm.ToString()
        };
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Graph control ownership is transferred to the visual tree via the fluent builder.")]
    private static ServiceMapGraphControl CreateGraphViewer(ServiceMapViewModel vm)
    {
        return new ServiceMapGraphControl()
            .BindValue(
                ServiceMapGraphControl.GraphProperty,
                CompiledBinding.Create<ServiceMapViewModel, ServiceMapGraph?>(x => x.Graph, source: vm))
            .BindValue(
                ServiceMapGraphControl.LayoutAlgorithmProperty,
                CompiledBinding.Create<ServiceMapViewModel, LayoutAlgorithmTypeEnum>(x => x.LayoutAlgorithm, source: vm))
            .BindValue(
                ServiceMapGraphControl.ViewNodeCommandProperty,
                CompiledBinding.Create<ServiceMapViewModel, System.Windows.Input.ICommand>(x => x.ViewNodeCommand, source: vm));
    }

    private static ScrollViewer CreateSelectedFlows()
    {
        return new ScrollViewer()
            .MaxHeight(72)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
            .Content(new StackPanel()
                .Children(
                    new ItemsControl()
                        .ItemsSource(CompiledBinding.Create<ServiceMapViewModel, IEnumerable<FlowViewModel>>(x => x.SelectedNodeFlows))
                        .ItemTemplate(new FuncDataTemplate<FlowViewModel>((flow, _) => new TextBlock()
                            .Margin(8, 2)
                            .Text($"{flow.Timestamp.ToLocalTime():HH:mm:ss} {flow.Verdict}: {flow.SourcePod} \u2192 {flow.DestinationPod}")))));
    }

}
