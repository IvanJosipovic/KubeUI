using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.AccessControl.v1.ServiceAccount.Views;

namespace KubeUI.Avalonia.Resources.AccessControl;

public sealed partial class V1ServiceAccountConfig : ResourceConfigBase<V1ServiceAccount>
{
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_AccessControl", "Access Control");
    public override int Order => 0;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1ServiceAccount resource) => [new PropertiesView()];
}

