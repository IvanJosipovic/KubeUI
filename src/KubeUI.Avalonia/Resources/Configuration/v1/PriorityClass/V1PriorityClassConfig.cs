using Avalonia.Controls;
using k8s.Models;

namespace KubeUI.Avalonia.Resources.Configuration.v1.PriorityClass;

public sealed partial class V1PriorityClassConfig : ResourceConfigBase<V1PriorityClass>
{
    public V1PriorityClassConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override string Category => Assets.Resources.ResourceConfig_Category_Configuration!;
    public override int Order => 6;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1PriorityClass, int>()
            {
                Key = "value",
                Name = Assets.Resources.V1PriorityClassConfig_Value!,
                Field = x => x.Value,
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<V1PriorityClass, bool?>()
            {
                Key = "global-default",
                Name = Assets.Resources.V1PriorityClassConfig_Global_Default!,
                Field = x => x.GlobalDefault ?? false,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1PriorityClass resource) => [new PropertiesView()];
}
