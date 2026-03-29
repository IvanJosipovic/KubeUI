using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.Network.v1.Ingress.Views;

namespace KubeUI.Avalonia.Resources.Network;

public sealed partial class V1IngressConfig : ResourceConfigBase<V1Ingress>
{
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_Network", "Network");
    public override int Order => 3;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Ingress, string>()
            {
                Name = "Load Balancers",
                Display = x => x.Status.LoadBalancer.Ingress.Select(x => x.Ip).Aggregate((a,b) => a + ", " + b),
                Field = x => x.Status.LoadBalancer.Ingress.Count > 0 ? x.Status.LoadBalancer.Ingress[0].Ip : "",
                Width = "*",
            },
            new ResourceListColumn<V1Ingress, string>()
            {
                Name = "Rules",
                Display = x => x.Spec.Rules.Select(z => $"http://{z.Host}{z.Http.Paths[0].Path}").Aggregate((a,b) => a + ", " + b),
                Field = x => x.Spec.Rules.Count > 0 ? x.Spec.Rules[0].Host : "",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1Ingress resource) => [new PropertiesView()];
}

