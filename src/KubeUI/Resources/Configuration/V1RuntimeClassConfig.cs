using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads.Configuration;

[ServiceDescriptor<ResourceConfigBase<V1RuntimeClass>>(ServiceLifetime.Transient)]
public sealed partial class V1RuntimeClassConfig : ResourceConfigBase<V1RuntimeClass>
{
    public override string Category => "Configuration";
    public override int Order => 7;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1RuntimeClass, string>()
            {
                Name = "Handler",
                Display = x => x.Handler,
                Field = x => x.Handler,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }
}
