using k8s.Models;

namespace KubeUI.Avalonia.Resources.AccessControl.v1.ClusterRole;

public sealed partial class V1ClusterRoleConfig : ResourceConfig<V1ClusterRole>
{
    public V1ClusterRoleConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.AccessControlId);
    public override int Order => 1;
    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1ClusterRole resource) => [new PropertiesView()];
}
