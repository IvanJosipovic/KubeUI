using k8s.Models;
using Avalonia.Controls;
using KubeUI.Avalonia.Resources.Configuration.v1.MutatingWebhookConfiguration.Views;

namespace KubeUI.Avalonia.Resources.Configuration;

public sealed partial class V1MutatingWebhookConfig : ResourceConfigBase<V1MutatingWebhookConfiguration>
{
    public override string Category => CategoryString("ResourceConfig_Category_Configuration", "Configuration");
    public override int Order => 9;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1MutatingWebhookConfiguration, int>()
            {
                Name = "Webhooks",
                Field = x => x.Webhooks?.Count ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1MutatingWebhookConfiguration resource) => [new PropertiesView()];
}

