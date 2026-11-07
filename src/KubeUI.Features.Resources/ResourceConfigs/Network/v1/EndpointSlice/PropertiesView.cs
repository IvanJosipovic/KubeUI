using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Network.v1.EndpointSlice;

public sealed class PropertiesView : ViewBase<V1EndpointSlice>
{
    protected override object Build(V1EndpointSlice vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.EndpointSlicePropertiesView_Address_Type.CurrentValue()!)
                    .Value(vm.AddressType ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.EndpointSlicePropertiesView_Endpoints.CurrentValue()!)
                    .Value(vm.Endpoints?.Count ?? 0),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Ports.CurrentValue()!)
                    .Value(vm.Ports?.Count ?? 0));
    }
}
