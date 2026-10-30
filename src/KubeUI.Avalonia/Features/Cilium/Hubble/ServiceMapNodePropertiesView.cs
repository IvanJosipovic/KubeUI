using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Infrastructure.Presentation;

namespace KubeUI.Avalonia.Features.Cilium.Hubble;

/// <summary>Displays Hubble details for an aggregated service-map node.</summary>
public sealed class ServiceMapNodePropertiesView : KubeUIView<ServiceMapNodePropertiesViewModel>
{
    protected override object Build(ServiceMapNodePropertiesViewModel viewModel)
    {
        var node = viewModel.Node;
        if (node is null)
            return new TextBlock().Text(Assets.Resources.ServiceMapNodePropertiesView_NoNode!);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.ServiceMapNodePropertiesView_Id!)
                    .Value(node.Id),
                new PropertyItem()
                    .Key(Assets.Resources.ServiceMapNodePropertiesView_Name!)
                    .Value(node.Name),
                new PropertyItem()
                    .Key(Assets.Resources.ServiceMapNodePropertiesView_Namespace!)
                    .Value(string.IsNullOrEmpty(node.Namespace)
                        ? Assets.Resources.ServiceMapGraphControl_External!
                        : node.Namespace),
                new PropertyItem()
                    .Key(Assets.Resources.ServiceMapNodePropertiesView_WorkloadType!)
                    .Value(node.WorkloadType ?? string.Empty),
                new PropertyItem()
                    .Key(Assets.Resources.ServiceMapNodePropertiesView_IngressFlows!)
                    .Value(node.IngressFlowCount),
                new PropertyItem()
                    .Key(Assets.Resources.ServiceMapNodePropertiesView_EgressFlows!)
                    .Value(node.EgressFlowCount),
                new PropertyItem()
                    .Key(Assets.Resources.ServiceMapNodePropertiesView_AllowedFlows!)
                    .Value(node.AllowedCount),
                new PropertyItem()
                    .Key(Assets.Resources.ServiceMapNodePropertiesView_DeniedFlows!)
                    .Value(node.DeniedCount),
                new PropertyItem()
                    .Key(Assets.Resources.ServiceMapNodePropertiesView_AccessPoints!)
                    .Value(viewModel.AccessPoints));
    }
}
