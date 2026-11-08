using k8s.Models;

namespace KubeUI.Features.Resources.Storage.v1.PersistentVolume;

public sealed partial class V1PersistentVolumeConfig : ResourceConfig<V1PersistentVolume>
{
    public V1PersistentVolumeConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.StorageId);
    public override int Order => 1;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1PersistentVolume, string>()
            {
                Key = "storage-class",
                Name = L10n.Instance.V1PersistentVolumeConfig_Storage_Class.CurrentValue()!,
                Field = x => x.Spec.StorageClassName,
                Width = 1, WidthMode = DynamicTableViewWidthMode.Star,
            },
            new ResourceListColumn<V1PersistentVolume, decimal>()
            {
                Key = "size",
                Name = L10n.Instance.V1PersistentVolumeConfig_Size.CurrentValue()!,
                Display = x => x.Spec.Capacity["storage"]?.CanonicalizeString(ResourceQuantity.SuffixFormat.BinarySI) ?? "",
                Field = x => x.Spec.Capacity["storage"]?.ToDecimal() ?? 0,
                WidthMode = DynamicTableViewWidthMode.Cells
            },
            new ResourceListColumn<V1PersistentVolume, string>()
            {
                Key = "claim",
                Name = L10n.Instance.V1PersistentVolumeConfig_Claim.CurrentValue()!,
                Field = x => x.Spec.ClaimRef.Name,
                Width = 1, WidthMode = DynamicTableViewWidthMode.Star,
            },
            AgeColumn(),
            new ResourceListColumn<V1PersistentVolume, string>()
            {
                Key = "status",
                Name = L10n.Instance.V1PersistentVolumeConfig_Status.CurrentValue()!,
                Field = x => x.Status.Phase,
                WidthMode = DynamicTableViewWidthMode.Cells
            },
        ];
    }

    public override Control[] Properties(V1PersistentVolume resource) => [new PropertiesView()];
}
