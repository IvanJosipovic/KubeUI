using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Core.v1.Node;

public sealed class PropertiesView : ViewBase<ResourcePropertiesViewModel<V1Node>>
{
    protected override object Build(ResourcePropertiesViewModel<V1Node> vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        var resource = vm.Object ?? throw new InvalidOperationException("Node properties require a resource.");

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.NodePropertiesView_Operating_System!)
                    .BindValue(vm, static resource => resource?.Status?.NodeInfo?.OperatingSystem ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.NodePropertiesView_Architecture!)
                    .BindValue(vm, static resource => resource?.Status?.NodeInfo?.Architecture ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.NodePropertiesView_Kernel_Version!)
                    .BindValue(vm, static resource => resource?.Status?.NodeInfo?.KernelVersion ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.NodePropertiesView_Container_Runtime!)
                    .BindValue(vm, static resource => resource?.Status?.NodeInfo?.ContainerRuntimeVersion ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.NodePropertiesView_Kubelet_Version!)
                    .BindValue(vm, static resource => resource?.Status?.NodeInfo?.KubeletVersion ?? ""),
                new MetricsControl { DataContext = resource },
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Status!)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.NodePropertiesView_Addresses!)
                                    .BindValue(vm, static resource => resource?.Status?.Addresses?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.NodePropertiesView_Taints!)
                                    .BindValue(vm, static resource => resource?.Spec?.Taints?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Conditions!)
                                    .BindValue(vm, static resource => resource?.Status?.Conditions?.Count ?? 0))),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Resources!)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Capacity_Entries!)
                                    .BindValue(vm, static resource => resource?.Status?.Capacity?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.NodePropertiesView_Allocatable_Entries!)
                                    .BindValue(vm, static resource => resource?.Status?.Allocatable?.Count ?? 0))));
    }
}
