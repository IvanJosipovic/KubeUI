using k8s.Models;

namespace KubeUI.Features.Resources.Configuration.v1.RuntimeClass;

public sealed partial class V1RuntimeClassConfig : ResourceConfig<V1RuntimeClass>
{
    public V1RuntimeClassConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.ConfigurationId);
    public override int Order => 7;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1RuntimeClass, string>()
            {
                Key = "handler",
                Name = L10n.Instance.V1RuntimeClassConfig_Handler.CurrentValue()!,
                Field = x => x.Handler,
                WidthMode = DynamicTableViewWidthMode.Cells
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1RuntimeClass resource) => [new PropertiesView()];
}
