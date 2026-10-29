using k8s.Models;

namespace KubeUI.Avalonia.Resources.AccessControl.v1.Role;

public sealed partial class V1RoleConfig : ResourceConfig<V1Role>
{
    public V1RoleConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override bool IsNamespaced => true;
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.AccessControlId);
    public override int Order => 2;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1Role resource) => [new PropertiesView()];
}
