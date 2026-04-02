using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.Configuration.v1.ResourceQuota.Views;

namespace KubeUI.Avalonia.Resources.Configuration.v1.ResourceQuota;

public sealed partial class V1ResourceQuotaConfig : ResourceConfigBase<V1ResourceQuota>
{
    public V1ResourceQuotaConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_Configuration", "Configuration");
    public override int Order => 2;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
        ];
    }

    public override Control[] Properties(V1ResourceQuota resource) => [new PropertiesView()];
}

