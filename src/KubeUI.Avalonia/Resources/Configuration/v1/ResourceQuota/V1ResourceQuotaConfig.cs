using k8s.Models;

namespace KubeUI.Avalonia.Resources.Configuration.v1.ResourceQuota;

public sealed partial class V1ResourceQuotaConfig : ResourceConfig<V1ResourceQuota>
{
    public V1ResourceQuotaConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.ConfigurationId);
    public override int Order => 2;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
        ];
    }

    public override Control[] Properties(V1ResourceQuota resource) => [new PropertiesView()];
}
