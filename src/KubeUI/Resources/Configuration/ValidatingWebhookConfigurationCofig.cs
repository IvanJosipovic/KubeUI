using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads.Configuration;

[ServiceDescriptor<ResourceConfigBase<V1ValidatingWebhookConfiguration>>(ServiceLifetime.Transient)]
public sealed partial class ValidatingWebhookConfigurationConfig : ResourceConfigBase<V1ValidatingWebhookConfiguration>
{
    public override string Category => "Configuration";

    public override bool ShowNamespaces => false;
    public override int Order => 10;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListViewDefinitionColumn<V1ValidatingWebhookConfiguration, int>()
            {
                Name = "Webhooks",
                Display = x => x.Webhooks.Count.ToString(),
                Field = x => x.Webhooks.Count,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [

        ];
    }

    public override Control[] Properties(V1ValidatingWebhookConfiguration resource)
    {
        return null;
    }
}
