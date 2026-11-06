using KubeUI.Controls.Presentation;

namespace KubeUI.Features.Cilium.Hubble;

/// <summary>Displays Hubble details for an aggregated service-map node.</summary>
public sealed class ServiceMapNodePropertiesView : KubeUIView<ServiceMapNodePropertiesViewModel>
{
    protected override object Build(ServiceMapNodePropertiesViewModel viewModel)
    {
        var node = viewModel.Node;
        if (node is null)
            return new TextBlock().Text(L10n.Instance.ServiceMapNodePropertiesView_NoNode.CurrentValue()!);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(L10n.Instance.ServiceMapNodePropertiesView_Id.CurrentValue()!)
                    .Value(node.Id),
                new PropertyItem()
                    .Key(L10n.Instance.ServiceMapNodePropertiesView_Name.CurrentValue()!)
                    .Value(node.Name),
                new PropertyItem()
                    .Key(L10n.Instance.ServiceMapNodePropertiesView_Namespace.CurrentValue()!)
                    .Value(string.IsNullOrEmpty(node.Namespace)
                        ? L10n.Instance.ServiceMapGraphControl_External.CurrentValue()!
                        : node.Namespace),
                new PropertyItem()
                    .Key(L10n.Instance.ServiceMapNodePropertiesView_WorkloadType.CurrentValue()!)
                    .Value(node.WorkloadType ?? string.Empty),
                new PropertyItem()
                    .Key(L10n.Instance.ServiceMapNodePropertiesView_IngressFlows.CurrentValue()!)
                    .Value(node.IngressFlowCount),
                new PropertyItem()
                    .Key(L10n.Instance.ServiceMapNodePropertiesView_EgressFlows.CurrentValue()!)
                    .Value(node.EgressFlowCount),
                new PropertyItem()
                    .Key(L10n.Instance.ServiceMapNodePropertiesView_AllowedFlows.CurrentValue()!)
                    .Value(node.AllowedCount),
                new PropertyItem()
                    .Key(L10n.Instance.ServiceMapNodePropertiesView_DeniedFlows.CurrentValue()!)
                    .Value(node.DeniedCount),
                new PropertyItem()
                    .Key(L10n.Instance.ServiceMapNodePropertiesView_AccessPoints.CurrentValue()!)
                    .Value(viewModel.AccessPoints));
    }
}
