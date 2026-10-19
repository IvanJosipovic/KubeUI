using k8s.Models;

namespace KubeUI.Avalonia.Resources.Configuration.v1.Lease;

public sealed partial class V1LeaseConfig : ResourceConfig<V1Lease>
{
    public V1LeaseConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.ConfigurationId);
    public override int Order => 8;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Lease, string>()
            {
                Key = "holder",
                Name = Assets.Resources.V1LeaseConfig_Holder!,
                Field = x => x.Spec.HolderIdentity ?? "",
                WidthMode = DynamicTableViewWidthMode.Cells
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1Lease resource) => [new PropertiesView()];
}
