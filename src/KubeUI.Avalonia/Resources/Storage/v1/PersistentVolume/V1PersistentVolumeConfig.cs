using k8s.Models;

namespace KubeUI.Avalonia.Resources.Storage.v1.PersistentVolume;

public sealed partial class V1PersistentVolumeConfig : ResourceConfigBase<V1PersistentVolume>
{
    public V1PersistentVolumeConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override string Category => Assets.Resources.ResourceConfig_Category_Storage!;
    public override int Order => 1;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1PersistentVolume, string>()
            {
                Key = "storage-class",
                Name = Assets.Resources.V1PersistentVolumeConfig_Storage_Class!,
                Field = x => x.Spec.StorageClassName,
                Width = 1, WidthMode = DynamicTableViewWidthMode.Star,
            },
            new ResourceListColumn<V1PersistentVolume, decimal>()
            {
                Key = "size",
                Name = Assets.Resources.V1PersistentVolumeConfig_Size!,
                Display = x => x.Spec.Capacity["storage"]?.CanonicalizeString(ResourceQuantity.SuffixFormat.BinarySI) ?? "",
                Field = x => x.Spec.Capacity["storage"]?.ToDecimal() ?? 0,
                WidthMode = DynamicTableViewWidthMode.Cells
            },
            new ResourceListColumn<V1PersistentVolume, string>()
            {
                Key = "claim",
                Name = Assets.Resources.V1PersistentVolumeConfig_Claim!,
                Field = x => x.Spec.ClaimRef.Name,
                Width = 1, WidthMode = DynamicTableViewWidthMode.Star,
            },
            AgeColumn(),
            new ResourceListColumn<V1PersistentVolume, string>()
            {
                Key = "status",
                Name = Assets.Resources.V1PersistentVolumeConfig_Status!,
                Field = x => x.Status.Phase,
                WidthMode = DynamicTableViewWidthMode.Cells
            },
        ];
    }

    public override Control[] Properties(V1PersistentVolume resource) => [new PropertiesView()];
}
