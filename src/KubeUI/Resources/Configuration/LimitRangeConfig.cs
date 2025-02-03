using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads.Configuration;

[ServiceDescriptor<ResourceConfigBase<V1ResourceQuota>>(ServiceLifetime.Transient)]
public sealed partial class ResourceQuotaConfig : ResourceConfigBase<V1ResourceQuota>
{
    public override string Category => "Configuration";
    public override int Order => 2;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [

        ];
    }

    public override Control[] Properties(V1ResourceQuota resource)
    {
        return null;
    }
}
