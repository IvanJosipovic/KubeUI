using k8s.Models;

namespace KubeUI.Resources.Configuration;

public sealed partial class V1LeaseConfig : ResourceConfigBase<V1Lease>
{
    public override bool IsNamespaced => true;
    public override string Category => "Configuration";
    public override int Order => 8;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Lease, string>()
            {
                Name = "Holder",
                Display = x => x.Spec.HolderIdentity ?? "",
                Field = x => x.Spec.HolderIdentity ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }
}
