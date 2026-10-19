using k8s.Models;

namespace KubeUI.Avalonia.Resources.Network.v1.EndpointSlice;

public sealed partial class V1EndpointSliceConfig : ResourceConfig<V1EndpointSlice>
{
    public V1EndpointSliceConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.NetworkId);
    public override int Order => 2;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1EndpointSlice resource) => [new PropertiesView()];
}
