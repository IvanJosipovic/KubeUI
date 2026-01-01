using k8s.Models;

namespace KubeUI.Resources.Network;

public sealed partial class V1IngressClassConfig : ResourceConfigBase<V1IngressClass>
{
    public override string Category => "Network";
    public override int Order => 4;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1IngressClass, string>()
            {
                Name = "Controller",
                Field = x => x.Spec.Controller,
                Width = "*",
            },
            new ResourceListColumn<V1IngressClass, string>()
            {
                Name = "API Group",
                Field = x => x.Spec.Parameters != null ? x.Spec.Parameters.ApiGroup : "",
                Width = "*",
            },
            new ResourceListColumn<V1IngressClass, string>()
            {
                Name = "Scope",
                Field = x => x.Spec.Parameters != null ? x.Spec.Parameters.Scope : "",
                Width = "*",
            },
            new ResourceListColumn<V1IngressClass, string>()
            {
                Name = "Kind",
                Field = x => x.Spec.Parameters != null ? x.Spec.Parameters.Kind : "",
                Width = "*",
            },
            AgeColumn(),
        ];
    }
}
