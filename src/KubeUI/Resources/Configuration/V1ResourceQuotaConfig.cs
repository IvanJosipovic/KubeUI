using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads.Configuration;

[ServiceDescriptor<ResourceConfigBase<V1ResourceQuota>>(ServiceLifetime.Transient)]
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
