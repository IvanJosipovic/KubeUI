using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.Configuration.v1.Lease.Views;

namespace KubeUI.Avalonia.Resources.Configuration.v1.Lease;

public sealed partial class V1LeaseConfig : ResourceConfigBase<V1Lease>
{
    public V1LeaseConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => Assets.Resources.ResourceConfig_Category_Configuration!;
    public override int Order => 8;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Lease, string>()
            {
                Key = "holder",
                Name = Assets.Resources.V1LeaseConfig_Holder!,
                Field = x => x.Spec.HolderIdentity ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1Lease resource) => [new PropertiesView()];
}

