using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.AccessControl.v1.ClusterRoleBinding.Views;

namespace KubeUI.Avalonia.Resources.AccessControl.v1.ClusterRoleBinding;

public sealed partial class V1ClusterRoleBindingConfig : ResourceConfigBase<V1ClusterRoleBinding>
{
    public V1ClusterRoleBindingConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override string Category => Assets.Resources.ResourceConfig_Category_AccessControl!;
    public override int Order => 3;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1ClusterRoleBinding, string>()
            {
                Key = "bindings",
                Name = Assets.Resources.V1ClusterRoleBindingConfig_Bindings!,
                Field = x => x.Subjects is { Count: > 0 } subjects ? string.Join(", ", subjects.Select(y => y.Name)) : "",
                Width = "*",
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1ClusterRoleBinding resource) => [new PropertiesView()];
}
