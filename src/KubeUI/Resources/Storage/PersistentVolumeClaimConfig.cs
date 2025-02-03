
using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Storage;

[ServiceDescriptor<ResourceConfigBase<V1PersistentVolumeClaim>>(ServiceLifetime.Transient)]
public sealed partial class PersistentVolumeClaimConfig : ResourceConfigBase<V1PersistentVolumeClaim>
{
    public override string Category => "Storage";
    public override int Order => 0;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<V1PersistentVolumeClaim, string>()
            {
                Name = "Storage Class",
                Field = x => x.Spec.StorageClassName,
                Width = "*",
            },
            new ResourceListViewDefinitionColumn<V1PersistentVolumeClaim, string>()
            {
                Name = "Size",
                Field = x => x.Spec.Resources.Requests["storage"].Value,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
            new ResourceListViewDefinitionColumn<V1PersistentVolumeClaim, string>()
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

    public override Control[]? Properties(V1PersistentVolumeClaim resource)
    {
        return null;
    }
}
