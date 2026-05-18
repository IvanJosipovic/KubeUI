using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.Configuration.v1.LimitRange.Views;

namespace KubeUI.Avalonia.Resources.Configuration.v1.LimitRange;

public sealed partial class V1LimitRangeConfig : ResourceConfigBase<V1LimitRange>
{
    public V1LimitRangeConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => Assets.Resources.ResourceConfig_Category_Configuration!;
    public override int Order => 3;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
        ];
    }

    public override Control[] Properties(V1LimitRange resource) => [new PropertiesView()];
}

