using k8s.Models;

namespace KubeUI.Avalonia.Resources.Network.v1.NetworkPolicy;

public sealed partial class V1NetworkPolicyConfig : ResourceConfig<V1NetworkPolicy>
{
    public V1NetworkPolicyConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.NetworkId);
    public override int Order => 5;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1NetworkPolicy, string>()
            {
                Key = "policy-types",
                Name = Assets.Resources.V1NetworkPolicyConfig_Policy_Types!,
                Field = x => x.Spec?.PolicyTypes is { Count: > 0 } policyTypes ? string.Join(", ", policyTypes) : "",
                Width = 1, WidthMode = DynamicTableViewWidthMode.Star,
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1NetworkPolicy resource) => [new PropertiesView()];
}
