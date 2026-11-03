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
                    .Key(Assets.Resources.EndpointSlicePropertiesView_Address_Type!)
                    .Value(vm.AddressType ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.EndpointSlicePropertiesView_Endpoints!)
                    .Value(vm.Endpoints?.Count ?? 0),
                new PropertyItem()
                    .Key(SharedAssets.Resources.Shared_Ports!)
                    .Value(vm.Ports?.Count ?? 0));
    }
}
