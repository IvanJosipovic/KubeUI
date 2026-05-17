using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.AccessControl.v1.RoleBinding.Views;

namespace KubeUI.Avalonia.Resources.AccessControl.v1.RoleBinding;

public sealed partial class V1RoleBindingConfig : ResourceConfigBase<V1RoleBinding>
{
    public V1RoleBindingConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override bool IsNamespaced => true;
    public override string Category => Assets.Resources.ResourceConfig_Category_AccessControl!;
    public override int Order => 4;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1RoleBinding, string>()
            {
                Key = "bindings",
                Name = Assets.Resources.V1RoleBindingConfig_Bindings!,
                Field = x => x.Subjects is { Count: > 0 } subjects ? string.Join(", ", subjects.Select(y => y.Name)) : "",
                Width = "*",
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1RoleBinding resource) => [new PropertiesView()];
}
