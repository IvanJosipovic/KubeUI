using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads.Configuration;

[ServiceDescriptor<ResourceConfigBase<V1Lease>>(ServiceLifetime.Transient)]
public sealed partial class LeaseConfig : ResourceConfigBase<V1Lease>
{
    public override string Category => "Configuration";
    public override int Order => 8;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<V1Lease, string>()
            {
                Name = "Holder",
                Display = x => x.Spec.HolderIdentity ?? "",
                Field = x => x.Spec.HolderIdentity ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [

        ];
    }

    public override Control[] Properties(V1Lease resource)
    {
        return null;
    }
}
