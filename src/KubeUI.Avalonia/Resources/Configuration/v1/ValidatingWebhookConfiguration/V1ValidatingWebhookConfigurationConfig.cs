using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.Configuration.v1.ValidatingWebhookConfiguration.Views;

namespace KubeUI.Avalonia.Resources.Configuration.v1.ValidatingWebhookConfiguration;

public sealed partial class V1ValidatingWebhookConfigurationConfig : ResourceConfigBase<V1ValidatingWebhookConfiguration>
{
    public V1ValidatingWebhookConfigurationConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override string Category => CategoryString("ResourceConfig_Category_Configuration", "Configuration");

    public override int Order => 10;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1ValidatingWebhookConfiguration, int>()
            {
                Name = "Webhooks",
                Field = x => x.Webhooks.Count,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1ValidatingWebhookConfiguration resource) => [new PropertiesView()];

}

