using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.Network.v1.EndpointSlice.Views;

namespace KubeUI.Avalonia.Resources.Network.v1.EndpointSlice;

public sealed partial class V1EndpointSliceConfig : ResourceConfigBase<V1EndpointSlice>
{
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_Network", "Network");
    public override int Order => 2;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1EndpointSlice resource) => [new PropertiesView()];
}

