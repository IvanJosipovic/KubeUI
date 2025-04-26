using k8s.Models;

namespace KubeUI.Resources.Workloads.Configuration;

public sealed partial class V1ResourceQuotaConfig : ResourceConfigBase<V1ResourceQuota>
{
    public override string Category => "Configuration";
    public override int Order => 2;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
        ];
    }
}
