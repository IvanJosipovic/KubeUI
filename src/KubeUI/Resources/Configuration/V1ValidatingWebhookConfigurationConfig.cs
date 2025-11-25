using k8s.Models;

namespace KubeUI.Resources.Configuration;

public sealed partial class V1ValidatingWebhookConfigurationConfig : ResourceConfigBase<V1ValidatingWebhookConfiguration>
{
    public override string Category => "Configuration";

    public override int Order => 10;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1ValidatingWebhookConfiguration, int>()
            {
                Name = "Webhooks",
                Display = x => x.Webhooks.Count.ToString(),
                Field = x => x.Webhooks.Count,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceMenuItem> MenuItems()
    {
        return [

        ];
    }
}
