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
                Field = x => x.Spec?.PolicyTypes is { Count: > 0 } policyTypes ? string.Join(", ", policyTypes) : "",
                Width = "*",
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1NetworkPolicy resource) => [new PropertiesView()];
}

