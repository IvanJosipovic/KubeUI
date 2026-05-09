using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.AccessControl.v1.ClusterRole.Views;

namespace KubeUI.Avalonia.Resources.AccessControl.v1.ClusterRole;

public sealed partial class V1ClusterRoleConfig : ResourceConfigBase<V1ClusterRole>
{
    public V1ClusterRoleConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override string Category => CategoryString("ResourceConfig_Category_AccessControl", "Access Control");
    public override int Order => 1;
    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1ClusterRole resource) => [new PropertiesView()];
}

