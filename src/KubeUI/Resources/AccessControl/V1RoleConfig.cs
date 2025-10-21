using k8s.Models;

namespace KubeUI.Resources.AccessControl;

public sealed partial class V1RoleConfig : ResourceConfigBase<V1Role>
{
    public override bool IsNamespaced => true;
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
