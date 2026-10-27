using k8s.Models;

namespace KubeUI.Avalonia.Resources.Configuration.v1.ValidatingWebhookConfiguration;

public sealed partial class V1ValidatingWebhookConfigurationConfig : ResourceConfig<V1ValidatingWebhookConfiguration>
{
    public V1ValidatingWebhookConfigurationConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.ConfigurationId);

    public override int Order => 10;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1ValidatingWebhookConfiguration, int>()
            {
                Key = "webhooks",
                Name = Assets.Resources.V1ValidatingWebhookConfigurationConfig_Webhooks!,
                Field = x => x.Webhooks.Count,
                WidthMode = DynamicTableViewWidthMode.Header
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1ValidatingWebhookConfiguration resource) => [new PropertiesView()];

}
