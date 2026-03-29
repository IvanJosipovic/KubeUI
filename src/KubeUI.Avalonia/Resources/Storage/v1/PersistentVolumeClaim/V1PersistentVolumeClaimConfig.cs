using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.Storage.v1.PersistentVolumeClaim.Views;

namespace KubeUI.Avalonia.Resources.Storage;

public sealed partial class V1PersistentVolumeClaimConfig : ResourceConfigBase<V1PersistentVolumeClaim>
{
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_Storage", "Storage");
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
            new ResourceListColumn<V1PersistentVolumeClaim, decimal>()
            {
                Name = "Size",
                Display = x => x.Spec.Resources?.Requests["storage"]?.CanonicalizeString(ResourceQuantity.SuffixFormat.BinarySI) ?? "",
                Field = x => x.Spec.Resources?.Requests["storage"]?.ToDecimal() ?? 0,
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

    public override Control[] Properties(V1PersistentVolumeClaim resource) => [new PropertiesView()];
}

