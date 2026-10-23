using k8s.Models;

namespace KubeUI.Avalonia.Resources.Storage.v1.PersistentVolumeClaim;

public sealed partial class V1PersistentVolumeClaimConfig : ResourceConfig<V1PersistentVolumeClaim>
{
    public V1PersistentVolumeClaimConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.StorageId);
    public override int Order => 0;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1PersistentVolumeClaim, string>()
            {
                Key = "storage-class",
                Name = Assets.Resources.V1PersistentVolumeClaimConfig_Storage_Class!,
                Field = x => x.Spec.StorageClassName,
                Width = 1, WidthMode = DynamicTableViewWidthMode.Star,
            },
            new ResourceListColumn<V1PersistentVolumeClaim, decimal>()
            {
                Key = "size",
                Name = Assets.Resources.V1PersistentVolumeClaimConfig_Size!,
                Display = x => x.Spec.Resources?.Requests["storage"]?.CanonicalizeString(ResourceQuantity.SuffixFormat.BinarySI) ?? "",
                Field = x => x.Spec.Resources?.Requests["storage"]?.ToDecimal() ?? 0,
                WidthMode = DynamicTableViewWidthMode.Cells
            },
            AgeColumn(),
            new ResourceListColumn<V1PersistentVolumeClaim, string>()
            {
                Key = "status",
                Name = Assets.Resources.V1PersistentVolumeClaimConfig_Status!,
                Field = x => x.Status.Phase,
                WidthMode = DynamicTableViewWidthMode.Cells
            },
        ];
    }

    public override Control[] Properties(V1PersistentVolumeClaim resource) => [new PropertiesView()];
}
