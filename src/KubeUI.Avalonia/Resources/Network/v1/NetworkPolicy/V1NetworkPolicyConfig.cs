using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.Network.v1.NetworkPolicy.Views;

namespace KubeUI.Avalonia.Resources.Network.v1.NetworkPolicy;

public sealed partial class V1NetworkPolicyConfig : ResourceConfigBase<V1NetworkPolicy>
{
    public V1NetworkPolicyConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => Assets.Resources.ResourceConfig_Category_Network!;
    public override int Order => 5;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1NetworkPolicy, string>()
            {
                Key = "policy-types",
                Name = Assets.Resources.V1NetworkPolicyConfig_Policy_Types!,
                Display = x => x.Spec.PolicyTypes.Aggregate((a,b) => a + ", " + b),
                Field = x => x.Spec.PolicyTypes.Count > 0 ? x.Spec.PolicyTypes[0] : "",
                Width = "*",
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1NetworkPolicy resource) => [new PropertiesView()];
}

