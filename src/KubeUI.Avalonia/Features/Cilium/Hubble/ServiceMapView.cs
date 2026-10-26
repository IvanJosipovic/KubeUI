using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Declarative;
using Avalonia.Markup.Xaml.MarkupExtensions;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Avalonia.Features.Cilium.Hubble.Controls;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Cilium.Hubble;
using SvcSystems.Avalonia.DynamicTableView;
using Westermo.GraphX.Common.Enums;
using UrsaControls = Ursa.Controls;

namespace KubeUI.Avalonia.Features.Cilium.Hubble;

public sealed partial class ServiceMapView : ViewBase<ServiceMapViewModel>
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
                                .Text($"{timeWindow.TotalMinutes} {Assets.Resources.ServiceMapView_TimeWindowMinutes}"))),
                        new ToggleButton()
                            .Width(32)
                            .Height(32)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .IsChecked(CompiledBinding.Create<ServiceMapViewModel, bool>(x => x.AggregateByNamespace, mode: BindingMode.TwoWay))
                            .ToolTip_Tip(Assets.Resources.ServiceMapView_TooltipAggregate)
                            .Content(new FluentIcon().Icon(Icon.DataUsage)),
                        new ToggleButton()
                            .Width(32)
                            .Height(32)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .IsChecked(CompiledBinding.Create<ServiceMapViewModel, bool>(x => x.ShowOnlyErrors, mode: BindingMode.TwoWay))
                            .ToolTip_Tip(Assets.Resources.ServiceMapView_TooltipErrorsOnly)
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
                        CreateNamespaceSelector(vm),
                        new ToggleButton()
                            .Margin(0, 0, 2, 0)
                            .IsChecked(CompiledBinding.Create<ServiceMapViewModel, bool>(x => x.IsNamespaceSelectionLinked, source: vm, mode: BindingMode.TwoWay))
                            .ToolTip_Tip(Assets.Resources.ResourceListView_NamespaceLink)
                            .Content(new FluentIcon().Icon(Icon.Link))));
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
            .ToolTip_Tip(Assets.Resources.ServiceMapView_LayoutAlgorithm);
    }

    private static string GetLayoutAlgorithmName(LayoutAlgorithmTypeEnum algorithm)
    {
        return algorithm switch
        {
            LayoutAlgorithmTypeEnum.BoundedFR => Assets.Resources.ServiceMapView_Layout_BoundedFR!,
            LayoutAlgorithmTypeEnum.Circular => Assets.Resources.ServiceMapView_Layout_Circular!,
            LayoutAlgorithmTypeEnum.CompoundFDP => Assets.Resources.ServiceMapView_Layout_CompoundFDP!,
            LayoutAlgorithmTypeEnum.EfficientSugiyama => Assets.Resources.ServiceMapView_Layout_EfficientSugiyama!,
            LayoutAlgorithmTypeEnum.FR => Assets.Resources.ServiceMapView_Layout_FR!,
            LayoutAlgorithmTypeEnum.ISOM => Assets.Resources.ServiceMapView_Layout_ISOM!,
            LayoutAlgorithmTypeEnum.KK => Assets.Resources.ServiceMapView_Layout_KK!,
            LayoutAlgorithmTypeEnum.LinLog => Assets.Resources.ServiceMapView_Layout_LinLog!,
            LayoutAlgorithmTypeEnum.SimpleRandom => Assets.Resources.ServiceMapView_Layout_Random!,
            LayoutAlgorithmTypeEnum.Sugiyama => Assets.Resources.ServiceMapView_Layout_Sugiyama!,
            LayoutAlgorithmTypeEnum.Tree => Assets.Resources.ServiceMapView_Layout_Tree!,
            _ => algorithm.ToString()
        };
    }

    private static UrsaControls.MultiComboBox CreateNamespaceSelector(ServiceMapViewModel viewModel)
    {
        var template = new FuncDataTemplate<V1Namespace>((ns, _) =>
            new TextBlock().Text(ns?.Metadata?.Name ?? string.Empty));

        return new UrsaControls.MultiComboBox()
            .Width(200)
            .MinWidth(140)
            .MaxHeight(20)
            .Margin(0, 0, 2, 0)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Classes("ClearButton")
            .ItemsSource(CompiledBinding.Create<ServiceMapViewModel, IEnumerable>(x => x.Cluster!.Runtime.Namespaces, source: viewModel))
            .PlaceholderText(Assets.Resources.ResourceListView_SelectNamespace)
            .SelectedItems(CompiledBinding.Create<ServiceMapViewModel, IList>(x => x.SelectedNamespaces, source: viewModel))
            .ItemTemplate(template)
            .SelectedItemTemplate(template);
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
