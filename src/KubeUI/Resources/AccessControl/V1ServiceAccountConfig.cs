using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.AccessControl;

[ServiceDescriptor<ResourceConfigBase<V1ServiceAccount>>(ServiceLifetime.Transient)]
public sealed partial class V1ServiceAccountConfig : ResourceConfigBase<V1ServiceAccount>
{
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
