using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.Storage.v1.PersistentVolume.Views;

namespace KubeUI.Avalonia.Resources.Storage;

public sealed partial class V1PersistentVolumeConfig : ResourceConfigBase<V1PersistentVolume>
{
    public override string Category => CategoryString("ResourceConfig_Category_Storage", "Storage");
    public override int Order => 1;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1PersistentVolume, string>()
            {
                Name = "Storage Class",
                Field = x => x.Spec.StorageClassName,
                Width = "*",
            },
            new ResourceListColumn<V1PersistentVolume, decimal>()
            {
                Name = "Size",
                Display = x => x.Spec.Capacity["storage"]?.CanonicalizeString(ResourceQuantity.SuffixFormat.BinarySI) ?? "",
                Field = x => x.Spec.Capacity["storage"]?.ToDecimal() ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<V1PersistentVolume, string>()
            {
                Name = "Claim",
                Field = x => x.Spec.ClaimRef.Name,
                Width = "*",
            },
            AgeColumn(),
            new ResourceListColumn<V1PersistentVolume, string>()
            {
                Name = "Status",
                Field = x => x.Status.Phase,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
        ];
    }

    public override Control[] Properties(V1PersistentVolume resource) => [new PropertiesView()];
}

