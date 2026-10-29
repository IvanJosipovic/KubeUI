using k8s.Models;

namespace KubeUI.Avalonia.Resources.AccessControl.v1.RoleBinding;

public sealed partial class V1RoleBindingConfig : ResourceConfig<V1RoleBinding>
{
    public V1RoleBindingConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override bool IsNamespaced => true;
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.AccessControlId);
    public override int Order => 4;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1RoleBinding, string>()
            {
                Key = "bindings",
                Name = Assets.Resources.V1RoleBindingConfig_Bindings!,
                Field = x => x.Subjects is { Count: > 0 } subjects ? string.Join(", ", subjects.Select(y => y.Name)) : "",
                Width = 1, WidthMode = DynamicTableViewWidthMode.Star,
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1RoleBinding resource) => [new PropertiesView()];
}
