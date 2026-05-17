using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.Storage.v1.StorageClass.Views;

namespace KubeUI.Avalonia.Resources.Storage.v1.StorageClass;

public sealed partial class V1StorageClassConfig : ResourceConfigBase<V1StorageClass>
{
    public V1StorageClassConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override string Category => Assets.Resources.ResourceConfig_Category_Storage!;
    public override int Order => 2;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1StorageClass, string>()
            {
                Key = "provisioner",
                Name = Assets.Resources.V1StorageClassConfig_Provisioner!,
                Field = x => x.Provisioner,
                Width = "*",
            },
            new ResourceListColumn<V1StorageClass, string>()
            {
                Key = "reclaim-policy",
                Name = Assets.Resources.V1StorageClassConfig_Reclaim_Policy!,
                Field = x => x.ReclaimPolicy,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<V1StorageClass, string>()
            {
                Key = "default",
                Name = Assets.Resources.V1StorageClassConfig_Default!, // "storageclass.kubernetes.io/is-default-class":"true"
                Field = x => x.Metadata.Annotations?.ContainsKey("storageclass.kubernetes.io/is-default-class") == true ?
                                x.Metadata.Annotations["storageclass.kubernetes.io/is-default-class"] : "false",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1StorageClass resource) => [new PropertiesView()];
}

