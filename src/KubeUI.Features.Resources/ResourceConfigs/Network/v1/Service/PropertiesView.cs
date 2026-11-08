using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Network.v1.Service;

public sealed class PropertiesView : ViewBase<V1Service>
{
    protected override object Build(V1Service vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Type.CurrentValue()!)
                    .Value(vm.Spec?.Type ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.ServicePropertiesView_Cluster_IP.CurrentValue()!)
                    .Value(vm.Spec?.ClusterIP ?? ""),
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Ports.CurrentValue()!)
                    .Value(vm.Spec?.Ports?.Count ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.ServicePropertiesView_Load_Balancer_Ingress.CurrentValue()!)
                    .Value(vm.Status?.LoadBalancer?.Ingress?.Count ?? 0),
                new ExpandableSection()
                    .Header(L10n.Instance.ServicePropertiesView_Routing.ToBinding())
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(L10n.Instance.ServicePropertiesView_Selector.CurrentValue()!)
                                    .Value(vm.Spec?.Selector?.Count ?? 0),
                                new PropertyItem()
                                    .Key(L10n.Instance.ServicePropertiesView_Session_Affinity.CurrentValue()!)
                                    .Value(vm.Spec?.SessionAffinity ?? ""),
                                new PropertyItem()
                                    .Key(L10n.Instance.ServicePropertiesView_Internal_Traffic_Policy.CurrentValue()!)
                                    .Value(vm.Spec?.InternalTrafficPolicy ?? ""),
                                new PropertyItem()
                                    .Key(L10n.Instance.ServicePropertiesView_External_Traffic_Policy.CurrentValue()!)
                                    .Value(vm.Spec?.ExternalTrafficPolicy ?? ""),
                                new PropertyItem()
                                    .Key(L10n.Instance.ServicePropertiesView_Publish_Not_Ready_Addresses.CurrentValue()!)
                                    .Value(vm.Spec?.PublishNotReadyAddresses ?? false))),
                new ExpandableSection()
                    .Header(L10n.Instance.ServicePropertiesView_Networking.ToBinding())
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(L10n.Instance.ServicePropertiesView_Cluster_IPs.CurrentValue()!)
                                    .Value(vm.Spec?.ClusterIPs?.Count ?? 0),
                                new PropertyItem()
                                    .Key(L10n.Instance.ServicePropertiesView_External_IPs.CurrentValue()!)
                                    .Value(vm.Spec?.ExternalIPs?.Count ?? 0),
                                new PropertyItem()
                                    .Key(L10n.Instance.ServicePropertiesView_External_Name.CurrentValue()!)
                                    .Value(vm.Spec?.ExternalName ?? ""),
                                new PropertyItem()
                                    .Key(L10n.Instance.ServicePropertiesView_Load_Balancer_IP.CurrentValue()!)
                                    .Value(vm.Spec?.LoadBalancerIP ?? ""),
                                new PropertyItem()
                                    .Key(L10n.Instance.ServicePropertiesView_Load_Balancer_Class.CurrentValue()!)
                                    .Value(vm.Spec?.LoadBalancerClass ?? ""),
                                new PropertyItem()
                                    .Key(L10n.Instance.ServicePropertiesView_Health_Check_Node_Port.CurrentValue()!)
                                    .Value(vm.Spec?.HealthCheckNodePort ?? 0))));
    }
}
