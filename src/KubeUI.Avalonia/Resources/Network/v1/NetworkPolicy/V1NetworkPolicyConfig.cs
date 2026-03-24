using k8s.Models;

namespace KubeUI.Avalonia.Resources.Network;

public sealed partial class V1NetworkPolicyConfig : ResourceConfigBase<V1NetworkPolicy>
{
    public override bool IsNamespaced => true;
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

