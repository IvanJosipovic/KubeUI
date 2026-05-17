using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.Network.v1.IngressClass.Views;

namespace KubeUI.Avalonia.Resources.Network.v1.IngressClass;

public sealed partial class V1IngressClassConfig : ResourceConfigBase<V1IngressClass>
{
    public V1IngressClassConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override string Category => CategoryString("ResourceConfig_Category_Network", "Network");
    public override int Order => 4;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1IngressClass, string>()
            {
                Key = "controller",
                Name = Assets.Resources.V1IngressClassConfig_Controller!,
                Field = x => x.Spec.Controller,
                Width = "*",
            },
            new ResourceListColumn<V1IngressClass, string>()
            {
                Key = "api-group",
                Name = Assets.Resources.V1IngressClassConfig_API_Group!,
                Field = x => x.Spec?.Parameters?.ApiGroup ?? "",
                Width = "*",
            },
            new ResourceListColumn<V1IngressClass, string>()
            {
                Key = "scope",
                Name = Assets.Resources.V1IngressClassConfig_Scope!,
                Field = x => x.Spec.Parameters?.Scope ?? "",
                Width = "*",
            },
            new ResourceListColumn<V1IngressClass, string>()
            {
                Key = "kind",
                Name = Assets.Resources.V1IngressClassConfig_Kind!,
                Field = x => x.Spec.Parameters?.Kind ?? "",
                Width = "*",
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1IngressClass resource) => [new PropertiesView()];
}

