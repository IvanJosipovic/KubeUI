using k8s.Models;

namespace KubeUI.Resources.Core.v1;

public sealed partial class V1NamespaceConfig : ResourceConfigBase<V1Namespace>
{
    public override int Order => 6;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1Namespace, string>()
            {
                Name = "Labels",
                Field = x => x.Metadata.Labels?.Select(x => x.Key + "=" + x.Value).Aggregate((x,y) => x + ", " + y) ?? "",
                Width = "2*"
            },
            new ResourceListColumn<V1Namespace, string>()
            {
                Name = "Status",
                Field = x => x.Status?.Phase ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }
}
