
using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.AccessControl;

[ServiceDescriptor<ResourceConfigBase<V1ClusterRoleBinding>>(ServiceLifetime.Transient)]
public sealed partial class V1ClusterRoleBindingConfig : ResourceConfigBase<V1ClusterRoleBinding>
{
    public override string Category => "Access Control";
    public override bool ShowNamespaces => false;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListViewDefinitionColumn<V1ClusterRoleBinding, string>()
            {
                Name = "Bindings",
                Field = x => x.Subjects == null || x.Subjects.Count == 0 ? "" : x.Subjects.Select(y => y.Name).Aggregate((a, b) => a + ", " + b),
                Width = "*",
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [];
    }

    public override Control[] Properties(V1ClusterRoleBinding resource)
    {
        return null;
    }
}