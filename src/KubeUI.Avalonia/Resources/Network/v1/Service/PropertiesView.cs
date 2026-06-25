using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Network.v1.Service;

public sealed class PropertiesView : ViewBase<V1Service>
{
    protected override object Build(V1Service vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Type!)
                    .Value(vm.Spec?.Type ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.ServicePropertiesView_Cluster_IP!)
                    .Value(vm.Spec?.ClusterIP ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Ports!)
                    .Value(vm.Spec?.Ports?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.ServicePropertiesView_Load_Balancer_Ingress!)
                    .Value(vm.Status?.LoadBalancer?.Ingress?.Count ?? 0),
                new ExpandableSection()
                    .Header(Assets.Resources.ServicePropertiesView_Routing!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.ServicePropertiesView_Selector!)
                                    .Value(vm.Spec?.Selector?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.ServicePropertiesView_Session_Affinity!)
                                    .Value(vm.Spec?.SessionAffinity ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.ServicePropertiesView_Internal_Traffic_Policy!)
                                    .Value(vm.Spec?.InternalTrafficPolicy ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.ServicePropertiesView_External_Traffic_Policy!)
                                    .Value(vm.Spec?.ExternalTrafficPolicy ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.ServicePropertiesView_Publish_Not_Ready_Addresses!)
                                    .Value(vm.Spec?.PublishNotReadyAddresses ?? false))),
                new ExpandableSection()
                    .Header(Assets.Resources.ServicePropertiesView_Networking!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.ServicePropertiesView_Cluster_IPs!)
                                    .Value(vm.Spec?.ClusterIPs?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.ServicePropertiesView_External_IPs!)
                                    .Value(vm.Spec?.ExternalIPs?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.ServicePropertiesView_External_Name!)
                                    .Value(vm.Spec?.ExternalName ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.ServicePropertiesView_Load_Balancer_IP!)
                                    .Value(vm.Spec?.LoadBalancerIP ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.ServicePropertiesView_Load_Balancer_Class!)
                                    .Value(vm.Spec?.LoadBalancerClass ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.ServicePropertiesView_Health_Check_Node_Port!)
                                    .Value(vm.Spec?.HealthCheckNodePort ?? 0))));
    }
}
