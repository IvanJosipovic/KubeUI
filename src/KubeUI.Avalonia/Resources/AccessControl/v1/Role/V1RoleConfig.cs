using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.AccessControl.v1.Role.Views;

namespace KubeUI.Avalonia.Resources.AccessControl.v1.Role;

public sealed partial class V1RoleConfig : ResourceConfigBase<V1Role>
{
    public V1RoleConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_AccessControl", "Access Control");
    public override int Order => 2;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1Role resource) => [new PropertiesView()];
}

