using k8s.Models;
using Avalonia.Controls;
using KubeUI.Avalonia.Resources.Configuration.v1.Lease.Views;

namespace KubeUI.Avalonia.Resources.Configuration;

public sealed partial class V1LeaseConfig : ResourceConfigBase<V1Lease>
{
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_Configuration", "Configuration");
    public override int Order => 8;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Lease, string>()
            {
                Name = "Holder",
                Field = x => x.Spec.HolderIdentity ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1Lease resource) => [new PropertiesView()];
}

