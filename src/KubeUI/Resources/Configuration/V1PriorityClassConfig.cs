using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads.Configuration;

[ServiceDescriptor<ResourceConfigBase<V1PriorityClass>>(ServiceLifetime.Transient)]
public sealed partial class V1PriorityClassConfig : ResourceConfigBase<V1PriorityClass>
{
    public override string Category => "Configuration";
    public override bool ShowNamespaces => false;
    public override int Order => 6;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1PriorityClass, int>()
            {
                Name = "Value",
                Display = x => x.Value.ToString(),
                Field = x => x.Value,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<V1PriorityClass, bool?>()
            {
                Name = "Global Default",
                Display = x => (x.GlobalDefault ?? false).ToString(),
                Field = x => x.GlobalDefault ?? false,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }
}
