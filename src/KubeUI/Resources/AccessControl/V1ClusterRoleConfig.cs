using k8s.Models;

namespace KubeUI.Resources.AccessControl;

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
