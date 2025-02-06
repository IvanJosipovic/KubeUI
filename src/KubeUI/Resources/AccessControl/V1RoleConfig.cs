using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.AccessControl;

[ServiceDescriptor<ResourceConfigBase<V1Role>>(ServiceLifetime.Transient)]
public sealed partial class V1RoleConfig : ResourceConfigBase<V1Role>
{
    public override string Category => "Access Control";
    public override int Order => 2;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            AgeColumn(),
        ];
    }
}
