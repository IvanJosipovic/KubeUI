using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Network;

[ServiceDescriptor<ResourceConfigBase<V1Endpoints>>(ServiceLifetime.Transient)]
public sealed partial class V1EndpointsConfig : ResourceConfigBase<V1Endpoints>
{
    public override string Category => "Network";
    public override int Order => 1;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<V1Endpoints, int>()
            {
                Name = "Endpoints",
                Display = x => x.Subsets != null ? x.Subsets.SelectMany(y => y.Ports.Select(z => y.Addresses[0].Ip + ":" + z.Port)).Aggregate((a,b) => a + ", " + b) : "",
                Field = x => x.Subsets != null ? x.Subsets[0].Ports[0].Port : 0,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [];
    }

    public override Control[]? Properties(V1Endpoints resource)
    {
        return null;
    }
}
