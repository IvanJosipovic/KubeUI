using k8s.Models;

namespace KubeUI.Resources.Workloads;

public sealed partial class V1JobConfig : ResourceConfigBase<V1Job>
{
    public override bool IsNamespaced => true;
    public override string Category => "Workloads";
    public override int Order => 5;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Job, int>()
            {
                Name = "Completions",
                Display = x => $"{x.Status.Succeeded ?? 0}/{x.Spec.Completions ?? 0}",
                Field = x => x.Spec.Completions ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
            new ResourceListColumn<V1Job, string>()
            {
                Name = "Conditions",
                Field = x => x.Status?.Conditions?.FirstOrDefault(y => y.Status == "True")?.Type ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            ];
    }
}
