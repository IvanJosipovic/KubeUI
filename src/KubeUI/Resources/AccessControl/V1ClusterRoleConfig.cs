using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.AccessControl;

[ServiceDescriptor<ResourceConfigBase<V1ClusterRole>>(ServiceLifetime.Transient)]
public sealed partial class V1ClusterRoleConfig : ResourceConfigBase<V1ClusterRole>
{
    public override string Category => "Access Control";
    public override int Order => 1;
    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            AgeColumn(),
        ];
    }
}
