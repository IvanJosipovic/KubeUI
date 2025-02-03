using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads.Configuration;

[ServiceDescriptor<ResourceConfigBase<V1MutatingWebhookConfiguration>>(ServiceLifetime.Transient)]
public sealed partial class MutatingWebhookConfig : ResourceConfigBase<V1MutatingWebhookConfiguration>
{
    public override string Category => "Configuration";
    public override bool ShowNamespaces => false;
    public override int Order => 9;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListViewDefinitionColumn<V1MutatingWebhookConfiguration, int>()
            {
                Name = "Webhooks",
                Display = x => x.Webhooks?.Count.ToString() ?? "",
                Field = x => x.Webhooks?.Count ?? 0,
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

    public override Control[] Properties(V1MutatingWebhookConfiguration resource)
    {
        return null;
    }
}
