using k8s.Models;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Resources.Metrics.Controls;
using KubeUI.Features.Resources.Properties;

namespace KubeUI.Features.Resources.Core.v1.Node;

public sealed class PropertiesView : ViewBase<V1Node>
{
    protected override object Build(V1Node vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(new MetricsControl { DataContext = vm },
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.NodePropertiesView_Operating_System.CurrentValue()!)
                    .Value(vm.Status?.NodeInfo?.OperatingSystem ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.NodePropertiesView_Architecture.CurrentValue()!)
                    .Value(vm.Status?.NodeInfo?.Architecture ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.NodePropertiesView_Kernel_Version.CurrentValue()!)
                    .Value(vm.Status?.NodeInfo?.KernelVersion ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.NodePropertiesView_Container_Runtime.CurrentValue()!)
                    .Value(vm.Status?.NodeInfo?.ContainerRuntimeVersion ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.NodePropertiesView_Kubelet_Version.CurrentValue()!)
                    .Value(vm.Status?.NodeInfo?.KubeletVersion ?? ""),
                ResourcePropertiesSection.Create(
                    FoundationLanguageManager.Instance.Shared_Status,
                    new PropertyItem()
                        .Key(ResourcesLanguageManager.Instance.NodePropertiesView_Addresses.CurrentValue()!)
                        .Value(vm.Status?.Addresses?.Count ?? 0),
                    new PropertyItem()
                        .Key(ResourcesLanguageManager.Instance.NodePropertiesView_Taints.CurrentValue()!)
                        .Value(vm.Spec?.Taints?.Count ?? 0),
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Conditions.CurrentValue()!)
                        .Value(vm.Status?.Conditions?.Count ?? 0)),
                ResourcePropertiesSection.Create(
                    FoundationLanguageManager.Instance.Shared_Resources,
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Capacity_Entries.CurrentValue()!)
                        .Value(vm.Status?.Capacity?.Count ?? 0),
                    new PropertyItem()
                        .Key(ResourcesLanguageManager.Instance.NodePropertiesView_Allocatable_Entries.CurrentValue()!)
                        .Value(vm.Status?.Allocatable?.Count ?? 0)));
    }
}
