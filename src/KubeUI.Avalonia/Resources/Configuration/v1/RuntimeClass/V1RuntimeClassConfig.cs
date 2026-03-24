using k8s.Models;

namespace KubeUI.Avalonia.Resources.Configuration;

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
                Field = x => x.Handler,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }
}

