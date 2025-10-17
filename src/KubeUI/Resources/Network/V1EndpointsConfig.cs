using k8s.Models;

namespace KubeUI.Resources.Network;

public sealed partial class V1EndpointsConfig : ResourceConfigBase<V1Endpoints>
{
    public override bool IsNamespaced => true;
    public override string Category => "Network";
    public override int Order => 1;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Endpoints, int>()
            {
                Name = "Endpoints",
                Display = x => x.Subsets != null
                    ? x.Subsets
                        .Where(x => x.Addresses != null && x.Ports != null)
                        .SelectMany(y => y.Ports.Select(z => y.Addresses[0].Ip + ":" + z.Port))
                        .DefaultIfEmpty("")
                        .Aggregate((a, b) => a + ", " + b)
                    : "",
                Field = x => (x.Subsets?[0].Ports?[0].Port) ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];;
    }
}
