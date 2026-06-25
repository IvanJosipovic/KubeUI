using Avalonia.Controls;
using k8s.Models;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Job;

public sealed partial class V1JobConfig : ResourceConfigBase<V1Job>
{
    public V1JobConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => Assets.Resources.ResourceConfig_Category_Workloads!;
    public override int Order => 5;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Job, int>()
            {
                Key = "completions",
                Name = Assets.Resources.V1JobConfig_Completions!,
                Display = x => $"{x.Status.Succeeded ?? 0}/{x.Spec.Completions ?? 0}",
                Field = x => x.Spec.Completions ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
            new ResourceListColumn<V1Job, string>()
            {
                Key = "conditions",
                Name = Assets.Resources.V1JobConfig_Conditions!,
                Field = x => x.Status?.Conditions?.FirstOrDefault(y => y.Status == "True")?.Type ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
        ];
    }

    public override Control[] Properties(V1Job resource) => [new PropertiesView()];
}
