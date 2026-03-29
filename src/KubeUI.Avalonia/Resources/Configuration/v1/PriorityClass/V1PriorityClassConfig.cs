using k8s.Models;

namespace KubeUI.Avalonia.Resources.Configuration;

public sealed partial class V1PriorityClassConfig : ResourceConfigBase<V1PriorityClass>
{
    public override string Category => "Configuration";
    public override int Order => 6;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1PriorityClass, int>()
            {
                Name = "Value",
                Field = x => x.Value,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<V1PriorityClass, bool?>()
            {
                Name = "Global Default",
                Field = x => x.GlobalDefault ?? false,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }
}

