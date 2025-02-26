using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Network;

[ServiceDescriptor<ResourceConfigBase<V1EndpointSlice>>(ServiceLifetime.Transient)]
public sealed partial class V1EndpointSliceConfig : ResourceConfigBase<V1EndpointSlice>
{
    public override string Category => "Network";
    public override int Order => 2;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            AgeColumn(),
        ];
    }
}
