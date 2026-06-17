using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Core.v1.Node;

public sealed class PropertiesView : ViewBase<V1Node>
{
    protected override object Build(V1Node vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.NodePropertiesView_Operating_System!)
                    .Value(vm.Status?.NodeInfo?.OperatingSystem ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.NodePropertiesView_Architecture!)
                    .Value(vm.Status?.NodeInfo?.Architecture ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.NodePropertiesView_Kernel_Version!)
                    .Value(vm.Status?.NodeInfo?.KernelVersion ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.NodePropertiesView_Container_Runtime!)
                    .Value(vm.Status?.NodeInfo?.ContainerRuntimeVersion ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.NodePropertiesView_Kubelet_Version!)
                    .Value(vm.Status?.NodeInfo?.KubeletVersion ?? ""),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Status!)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.NodePropertiesView_Addresses!)
                                    .Value(vm.Status?.Addresses?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.NodePropertiesView_Taints!)
                                    .Value(vm.Spec?.Taints?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Conditions!)
                                    .Value(vm.Status?.Conditions?.Count ?? 0))),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Resources!)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Capacity_Entries!)
                                    .Value(vm.Status?.Capacity?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.NodePropertiesView_Allocatable_Entries!)
                                    .Value(vm.Status?.Allocatable?.Count ?? 0))));
    }
}
