using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Storage;

[ServiceDescriptor<ResourceConfigBase<V1StorageClass>>(ServiceLifetime.Transient)]
public sealed partial class StorageClassConfig : ResourceConfigBase<V1StorageClass>
{
    public override string Category => "Storage";
    public override int Order => 2;
    public override bool ShowNamespaces => false;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListViewDefinitionColumn<V1StorageClass, string>()
            {
                Name = "Provisioner",
                Field = x => x.Provisioner,
                Width = "*",
            },
            new ResourceListViewDefinitionColumn<V1StorageClass, string>()
            {
                Name = "Reclaim Policy",
                Field = x => x.ReclaimPolicy,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListViewDefinitionColumn<V1StorageClass, string>()
            {
                Name = "Default", // "storageclass.kubernetes.io/is-default-class":"true"
                Field = x => x.Metadata.Annotations?.ContainsKey("storageclass.kubernetes.io/is-default-class") == true ?
                                x.Metadata.Annotations["storageclass.kubernetes.io/is-default-class"] : "false",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [];
    }

    public override Control[]? Properties(V1StorageClass resource)
    {
        return null;
    }
}
