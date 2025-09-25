using k8s.Models;

namespace KubeUI.Resources.Workloads.Configuration;

public sealed partial class V1LimitRangeConfig : ResourceConfigBase<V1LimitRange>
{
    public override bool IsNamespaced => true;
    public override string Category => "Configuration";
    public override int Order => 3;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
        ];
    }
}
