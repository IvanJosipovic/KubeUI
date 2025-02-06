using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads.Configuration;

[ServiceDescriptor<ResourceConfigBase<V1Lease>>(ServiceLifetime.Transient)]
public sealed partial class V1LeaseConfig : ResourceConfigBase<V1Lease>
{
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
