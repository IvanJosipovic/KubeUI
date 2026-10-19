using k8s.Models;

namespace KubeUI.Avalonia.Resources.Configuration.v1.LimitRange;

public sealed partial class V1LimitRangeConfig : ResourceConfig<V1LimitRange>
{
    public V1LimitRangeConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.ConfigurationId);
    public override int Order => 3;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
        ];
    }

    public override Control[] Properties(V1LimitRange resource) => [new PropertiesView()];
}
