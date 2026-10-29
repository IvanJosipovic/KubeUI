using k8s.Models;

namespace KubeUI.Avalonia.Resources.Configuration.v1.ConfigMap;

public sealed partial class V1ConfigMapConfig : ResourceConfig<V1ConfigMap>
{
    public V1ConfigMapConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.ConfigurationId);
    public override int Order => 0;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1ConfigMap, string>()
            {
                Key = "keys",
                Name = Assets.Resources.V1ConfigMapConfig_Keys!,
                Field = x => x.Data is { Count: > 0 } data ? string.Join(", ", data.Keys) : "",
                WidthMode = DynamicTableViewWidthMode.Header
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1ConfigMap resource) => [new PropertiesView()];
}
