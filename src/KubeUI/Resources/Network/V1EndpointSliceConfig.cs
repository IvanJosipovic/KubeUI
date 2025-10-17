using k8s.Models;

namespace KubeUI.Resources.Network;

public sealed partial class V1EndpointSliceConfig : ResourceConfigBase<V1EndpointSlice>
{
    public override bool IsNamespaced => true;
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
