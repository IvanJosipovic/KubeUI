using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Declarative;
using Avalonia.Markup.Xaml.MarkupExtensions;
using KubeUI.Avalonia.Features.Cilium.Hubble.Controls;
using KubeUI.Cilium.Hubble;

namespace KubeUI.Avalonia.Features.Cilium.Hubble;

public sealed partial class ServiceMapView : ViewBase<ServiceMapViewModel>
{
    protected override object Build(ServiceMapViewModel vm)
    {
        return new Grid()
            .Rows("Auto,*")
            .Children(
                CreateTopBar().Row(0),
                new Grid()
                    .Cols("*,320")
                    .Children(
                        new Border()
                            .Margin(6)
                            .BorderThickness(1)
                            .BorderBrush(new DynamicResourceExtension("SystemBaseMediumBrush"))
                            .Child(
                                new Grid()
                                    .Children(
                                        new ScrollViewer()
                                            .HorizontalScrollBarVisibility(ScrollBarVisibility.Auto)
                                            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
                                            .Content(new ServiceMapGraphControl()
                                                .Bind(
                                                    ServiceMapGraphControl.GraphProperty,
                                                    CompiledBinding.Create<ServiceMapViewModel, ServiceMapGraph?>(x => x.Graph))),
                                        new TextBlock()
                                            .HorizontalAlignment(HorizontalAlignment.Center)
                                            .VerticalAlignment(VerticalAlignment.Center)
                                            .Text(Assets.Resources.ServiceMapView_NoFlows)
                                            .IsVisible(CompiledBinding.Create<ServiceMapViewModel, bool>(x => !x.HasFlows))))
                            .Col(0),
                        CreateNodePanel(vm).Col(1))
                    .Row(1));
    }

    private static Border CreateTopBar()
    {
        var graphTextBinding = CompiledBinding.Create<ServiceMapViewModel, string>(x => GraphText(x));
        return new Border()
            .Classes("toolbar")
            .MinHeight(40)
            .Child(
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(8)
                    .Margin(8, 0)
                    .Children(
                        new ComboBox()
                            .Width(140)
                            .ItemsSource(new[] { TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(60) })
                            .SelectedItem(CompiledBinding.Create<ServiceMapViewModel, TimeSpan>(x => x.TimeWindow, mode: BindingMode.TwoWay))
                            .ItemTemplate(new FuncDataTemplate<TimeSpan>((timeWindow, _) => new TextBlock()
                                .Text($"{timeWindow.TotalMinutes} {Assets.Resources.ServiceMapView_TimeWindowMinutes}"))),
                        new CheckBox()
                            .Content(Assets.Resources.ServiceMapView_AggregateByNamespace)
                            .IsChecked(CompiledBinding.Create<ServiceMapViewModel, bool>(x => x.AggregateByNamespace, mode: BindingMode.TwoWay)),
                        new CheckBox()
                            .Content(Assets.Resources.ServiceMapView_ShowErrorsOnly)
                            .IsChecked(CompiledBinding.Create<ServiceMapViewModel, bool>(x => x.ShowOnlyErrors, mode: BindingMode.TwoWay)),
                        new TextBlock()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Margin(12, 0, 0, 0)
                            .Text(CompiledBinding.Create<ServiceMapViewModel, string>(x => x.ConnectionStatus)),
                        new TextBlock()
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Margin(8, 0, 0, 0)
                            .Text(graphTextBinding)));
    }

    private static Border CreateNodePanel(ServiceMapViewModel vm)
    {
        return new Border()
            .Margin(6)
            .Padding(10)
            .BorderThickness(1)
            .BorderBrush(new DynamicResourceExtension("SystemBaseMediumBrush"))
            .Child(
                new StackPanel()
                    .Spacing(8)
                    .Children(
                        new TextBlock()
                            .FontWeight(FontWeight.SemiBold)
                            .Text(Assets.Resources.ServiceMapView_NodeDetails),
                        new ComboBox()
                            .ItemsSource(CompiledBinding.Create<ServiceMapViewModel, IEnumerable<ServiceMapNode>>(x => x.Nodes))
                            .SelectedItem(CompiledBinding.Create<ServiceMapViewModel, ServiceMapNode?>(x => x.SelectedNode, mode: BindingMode.TwoWay))
                            .ItemTemplate(new FuncDataTemplate<ServiceMapNode>((node, _) => new TextBlock()
                                .Text($"{node.Namespace}/{node.Name}"))),
                        new Button()
                            .Content(Assets.Resources.ServiceMapView_ShowFlowsForNode)
                            .Command(vm.ShowFlowsForNodeCommand)
                            .CommandParameter(CompiledBinding.Create<ServiceMapViewModel, ServiceMapNode?>(x => x.SelectedNode)),
                        new TextBlock()
                            .FontWeight(FontWeight.SemiBold)
                            .Text(Assets.Resources.ServiceMapView_SelectedNodeFlows),
                        new ScrollViewer()
                            .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
                            .Content(new ItemsControl()
                                .ItemsSource(CompiledBinding.Create<ServiceMapViewModel, IEnumerable<FlowViewModel>>(x => x.SelectedNodeFlows))
                                .ItemTemplate(new FuncDataTemplate<FlowViewModel>((flow, _) => new TextBlock()
                                    .Margin(0, 3)
                                    .Text($"{flow.Timestamp.ToLocalTime():HH:mm:ss} {flow.Verdict}: {flow.SourcePod} → {flow.DestinationPod}"))))));
    }

    private static string GraphText(ServiceMapViewModel vm)
    {
        var nodeCount = vm.Graph?.NodeCount ?? 0;
        var edgeCount = vm.Graph?.EdgeCount ?? 0;
        return string.Format(
            System.Globalization.CultureInfo.CurrentCulture,
            Assets.Resources.ServiceMapView_GraphCountsFormat,
            nodeCount,
            edgeCount);
    }
}
