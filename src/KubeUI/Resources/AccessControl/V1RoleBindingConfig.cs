using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.AccessControl;

[ServiceDescriptor<ResourceConfigBase<V1RoleBinding>>(ServiceLifetime.Transient)]
public sealed partial class V1RoleBindingConfig : ResourceConfigBase<V1RoleBinding>
{
    public override string Category => "Access Control";
    public override int Order => 4;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<V1RoleBinding, string>()
            {
                Name = "Bindings",
                Field = x => x.Subjects.Select(y => y.Name).Aggregate((a,b) => a + ", " + b),
                Width = "*",
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [

        ];
    }

    public override Control[] Properties(V1RoleBinding resource)
    {
        return null;
    }
}
