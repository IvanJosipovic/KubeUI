using k8s.Models;

namespace KubeUI.Resources.Storage;

public sealed partial class V1PersistentVolumeClaimConfig : ResourceConfigBase<V1PersistentVolumeClaim>
{
    public override string Category => "Storage";
    public override int Order => 0;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1PersistentVolumeClaim, string>()
            {
                Name = "Storage Class",
                Field = x => x.Spec.StorageClassName,
                Width = "*",
            },
            new ResourceListColumn<V1PersistentVolumeClaim, string>()
            {
                Name = "Size",
                Field = x => x.Spec.Resources.Requests["storage"].Value,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
            new ResourceListColumn<V1PersistentVolumeClaim, string>()
            {
                Name = "Status",
                Field = x => x.Status.Phase,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
        ];
    }
}
