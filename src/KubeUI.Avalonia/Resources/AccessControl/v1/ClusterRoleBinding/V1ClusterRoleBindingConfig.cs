using k8s.Models;

namespace KubeUI.Avalonia.Resources.AccessControl.v1.ClusterRoleBinding;

public sealed partial class V1ClusterRoleBindingConfig : ResourceConfig<V1ClusterRoleBinding>
{
    public V1ClusterRoleBindingConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.AccessControlId);
    public override int Order => 3;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1ClusterRoleBinding, string>()
            {
                Key = "bindings",
                Name = Assets.Resources.V1ClusterRoleBindingConfig_Bindings!,
                Field = x => x.Subjects is { Count: > 0 } subjects ? string.Join(", ", subjects.Select(y => y.Name)) : "",
                Width = 1, WidthMode = DynamicTableViewWidthMode.Star,
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1ClusterRoleBinding resource) => [new PropertiesView()];
}
