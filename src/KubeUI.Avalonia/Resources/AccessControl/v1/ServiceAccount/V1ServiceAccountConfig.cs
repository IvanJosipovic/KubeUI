using k8s.Models;

namespace KubeUI.Avalonia.Resources.AccessControl.v1.ServiceAccount;

public sealed partial class V1ServiceAccountConfig : ResourceConfig<V1ServiceAccount>
{
    public V1ServiceAccountConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override bool IsNamespaced => true;
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.AccessControlId);
    public override int Order => 0;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1ServiceAccount resource) => [new PropertiesView()];
}
