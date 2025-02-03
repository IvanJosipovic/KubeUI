
using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Storage;

[ServiceDescriptor<ResourceConfigBase<V1PersistentVolume>>(ServiceLifetime.Transient)]
public sealed partial class PersistentVolumeConfig : ResourceConfigBase<V1PersistentVolume>
{
    public override string Category => "Storage";
    public override int Order => 1;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListViewDefinitionColumn<V1PersistentVolume, string>()
            {
                Name = "Storage Class",
                Field = x => x.Spec.StorageClassName,
                Width = "*",
            },
            new ResourceListViewDefinitionColumn<V1PersistentVolume, string>()
            {
                Name = "Size",
                Field = x => x.Spec.Capacity["storage"].Value,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListViewDefinitionColumn<V1PersistentVolume, string>()
            {
                Name = "Claim",
                Field = x => x.Spec.ClaimRef.Name,
                Width = "*",
            },
            AgeColumn(),
            new ResourceListViewDefinitionColumn<V1PersistentVolume, string>()
            {
                Name = "Status",
                Field = x => x.Status.Phase,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [];
    }

    public override Control[]? Properties(V1PersistentVolume resource)
    {
        return null;
    }
}
