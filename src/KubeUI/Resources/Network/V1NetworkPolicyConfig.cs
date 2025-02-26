using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Network;

[ServiceDescriptor<ResourceConfigBase<V1NetworkPolicy>>(ServiceLifetime.Transient)]
public sealed partial class V1NetworkPolicyConfig : ResourceConfigBase<V1NetworkPolicy>
{
    public override string Category => "Network";
    public override int Order => 5;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1NetworkPolicy, string>()
            {
                Name = "Policy Types",
                Display = x => x.Spec.PolicyTypes.Aggregate((a,b) => a + ", " + b),
                Field = x => x.Spec.PolicyTypes.Count > 0 ? x.Spec.PolicyTypes[0] : "",
                Width = "*",
            },
            AgeColumn(),
        ];
    }
}
