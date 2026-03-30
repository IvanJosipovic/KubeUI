using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.Configuration.v2.HorizontalPodAutoscaler.Views;

namespace KubeUI.Avalonia.Resources.Configuration.v2.HorizontalPodAutoscaler;

public sealed partial class V2HorizontalPodAutoscalerConfig : ResourceConfigBase<V2HorizontalPodAutoscaler>
{
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_Configuration", "Configuration");
    public override int Order => 4;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V2HorizontalPodAutoscaler, int>()
            {
                Name = "Min Pods",
                Field = x => x.Spec.MinReplicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V2HorizontalPodAutoscaler, int>()
            {
                Name = "Max Pods",
                Field = x => x.Spec.MaxReplicas,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V2HorizontalPodAutoscaler, int>()
            {
                Name = "Replica",
                Field = x => x.Status.CurrentReplicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
            new ResourceListColumn<V2HorizontalPodAutoscaler, string>()
            {
                Name = "Conditions",
                Field = x => x.Status.Conditions?.FirstOrDefault(y => y.Status == "True")?.Type ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
        ];
    }

    public override Control[] Properties(V2HorizontalPodAutoscaler resource) => [new PropertiesView()];
}

