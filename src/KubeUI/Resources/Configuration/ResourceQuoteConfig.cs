using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads.Configuration;

[ServiceDescriptor<ResourceConfigBase<V1LimitRange>>(ServiceLifetime.Transient)]
public sealed partial class LimitRangeConfig : ResourceConfigBase<V1LimitRange>
{
    public override string Category => "Configuration";
    public override int Order => 3;

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

    public override Control[] Properties(V1LimitRange resource)
    {
        return null;
    }
}
