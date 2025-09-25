using k8s.Models;

namespace KubeUI.Resources.AccessControl;

public sealed partial class V1ServiceAccountConfig : ResourceConfigBase<V1ServiceAccount>
{
    public override bool IsNamespaced => true;
    public override string Category => "Access Control";
    public override int Order => 0;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            AgeColumn(),
        ];
    }
}
