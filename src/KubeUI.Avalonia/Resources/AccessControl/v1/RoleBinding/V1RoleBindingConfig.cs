using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.AccessControl.v1.RoleBinding.Views;

namespace KubeUI.Avalonia.Resources.AccessControl;

public sealed partial class V1RoleBindingConfig : ResourceConfigBase<V1RoleBinding>
{
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_AccessControl", "Access Control");
    public override int Order => 4;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1RoleBinding, string>()
            {
                Name = "Bindings",
                Field = x => x.Subjects.Select(y => y.Name).Aggregate((a,b) => a + ", " + b),
                Width = "*",
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1RoleBinding resource) => [new PropertiesView()];
}

