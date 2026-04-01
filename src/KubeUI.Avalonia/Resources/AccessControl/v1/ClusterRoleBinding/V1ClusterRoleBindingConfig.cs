using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.AccessControl.v1.ClusterRoleBinding.Views;

namespace KubeUI.Avalonia.Resources.AccessControl.v1.ClusterRoleBinding;

public sealed partial class V1ClusterRoleBindingConfig : ResourceConfigBase<V1ClusterRoleBinding>
{
    public override string Category => CategoryString("ResourceConfig_Category_AccessControl", "Access Control");
    public override int Order => 3;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1ClusterRoleBinding, string>()
            {
                Name = "Bindings",
                Field = x => x.Subjects == null || x.Subjects.Count == 0 ? "" : x.Subjects.Select(y => y.Name).Aggregate((a, b) => a + ", " + b),
                Width = "*",
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1ClusterRoleBinding resource) => [new PropertiesView()];
}

