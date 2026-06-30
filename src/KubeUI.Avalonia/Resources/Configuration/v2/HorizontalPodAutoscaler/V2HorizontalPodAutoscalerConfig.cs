using Avalonia.Controls;
using k8s.Models;

namespace KubeUI.Avalonia.Resources.Configuration.v2.HorizontalPodAutoscaler;

public sealed partial class V2HorizontalPodAutoscalerConfig : ResourceConfigBase<V2HorizontalPodAutoscaler>
{
    public V2HorizontalPodAutoscalerConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => Assets.Resources.ResourceConfig_Category_Configuration!;
    public override int Order => 4;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V2HorizontalPodAutoscaler, int>()
            {
                Key = "min-pods",
                Name = Assets.Resources.V2HorizontalPodAutoscalerConfig_Min_Pods!,
                Field = x => x.Spec.MinReplicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V2HorizontalPodAutoscaler, int>()
            {
                Key = "max-pods",
                Name = Assets.Resources.V2HorizontalPodAutoscalerConfig_Max_Pods!,
                Field = x => x.Spec.MaxReplicas,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V2HorizontalPodAutoscaler, int>()
            {
                Key = "replica",
                Name = Assets.Resources.V2HorizontalPodAutoscalerConfig_Replica!,
                Field = x => x.Status.CurrentReplicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
            new ResourceListColumn<V2HorizontalPodAutoscaler, string>()
            {
                Key = "conditions",
                Name = Assets.Resources.V2HorizontalPodAutoscalerConfig_Conditions!,
                Field = x => x.Status.Conditions?.FirstOrDefault(y => y.Status == "True")?.Type ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
        ];
    }

    public override Control[] Properties(V2HorizontalPodAutoscaler resource) => [new PropertiesView()];
}
