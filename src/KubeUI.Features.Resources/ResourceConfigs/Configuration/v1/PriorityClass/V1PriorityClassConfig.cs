using k8s.Models;

namespace KubeUI.Features.Resources.Configuration.v1.PriorityClass;

public sealed partial class V1PriorityClassConfig : ResourceConfig<V1PriorityClass>
{
    public V1PriorityClassConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.ConfigurationId);
    public override int Order => 6;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1PriorityClass, int?>()
            {
                Key = "value",
                Name = L10n.Instance.V1PriorityClassConfig_Value.CurrentValue()!,
                Field = x => x.Value,
                WidthMode = DynamicTableViewWidthMode.Cells
            },
            new ResourceListColumn<V1PriorityClass, bool?>()
            {
                Key = "global-default",
                Name = L10n.Instance.V1PriorityClassConfig_Global_Default.CurrentValue()!,
                Field = x => x.GlobalDefault ?? false,
                WidthMode = DynamicTableViewWidthMode.Header
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1PriorityClass resource) => [new PropertiesView()];
}
