using k8s.Models;

namespace KubeUI.Resources.Storage;

public sealed partial class V1StorageClassConfig : ResourceConfigBase<V1StorageClass>
{
    public override string Category => "Storage";
    public override int Order => 2;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1StorageClass, string>()
            {
                Name = "Provisioner",
                Field = x => x.Provisioner,
                Width = "*",
            },
            new ResourceListColumn<V1StorageClass, string>()
            {
                Name = "Reclaim Policy",
                Field = x => x.ReclaimPolicy,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<V1StorageClass, string>()
            {
                Name = "Default", // "storageclass.kubernetes.io/is-default-class":"true"
                Field = x => x.Metadata.Annotations?.ContainsKey("storageclass.kubernetes.io/is-default-class") == true ?
                                x.Metadata.Annotations["storageclass.kubernetes.io/is-default-class"] : "false",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }
}
