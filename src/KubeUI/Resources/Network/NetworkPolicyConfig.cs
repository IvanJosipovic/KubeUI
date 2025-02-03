using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Network;

[ServiceDescriptor<ResourceConfigBase<V1NetworkPolicy>>(ServiceLifetime.Transient)]
public sealed partial class NetworkPolicyConfig : ResourceConfigBase<V1NetworkPolicy>
{
    public override string Category => "Network";
    public override int Order => 5;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<V1NetworkPolicy, string>()
            {
                Name = "Policy Types",
                Display = x => x.Spec.PolicyTypes.Aggregate((a,b) => a + ", " + b),
                Field = x => x.Spec.PolicyTypes.Count > 0 ? x.Spec.PolicyTypes[0] : "",
                Width = "*",
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [];
    }

    public override Control[]? Properties(V1NetworkPolicy resource)
    {
        return null;
    }
}
