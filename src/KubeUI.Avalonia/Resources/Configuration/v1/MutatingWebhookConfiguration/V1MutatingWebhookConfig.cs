using k8s.Models;

namespace KubeUI.Avalonia.Resources.Configuration.v1.MutatingWebhookConfiguration;

public sealed partial class V1MutatingWebhookConfig : ResourceConfig<V1MutatingWebhookConfiguration>
{
    public V1MutatingWebhookConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.ConfigurationId);
    public override int Order => 9;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1MutatingWebhookConfiguration, int>()
            {
                Key = "webhooks",
                Name = Assets.Resources.V1MutatingWebhookConfig_Webhooks!,
                Field = x => x.Webhooks?.Count ?? 0,
                WidthMode = DynamicTableViewWidthMode.Header
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1MutatingWebhookConfiguration resource) => [new PropertiesView()];
}
