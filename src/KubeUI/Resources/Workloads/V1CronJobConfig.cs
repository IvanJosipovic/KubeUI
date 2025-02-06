using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads;

[ServiceDescriptor<ResourceConfigBase<V1CronJob>>(ServiceLifetime.Transient)]
public sealed partial class V1CronJobConfig : ResourceConfigBase<V1CronJob>
{
    public override string Category => "Workloads";
    public override int Order => 6;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1CronJob, string>()
            {
                Name = "Schedule",
                Display = x => x.Spec.Schedule,
                Field = x => x.Spec.Schedule,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1CronJob, bool>()
            {
                Name = "Suspend",
                Display = x => (x.Spec.Suspend ?? false).ToString(),
                Field = x => x.Spec.Suspend ?? false,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1CronJob, int>()
            {
                Name = "Active",
                Display = x => x.Status.Active?.Count.ToString() ?? "",
                Field = x => x.Status.Active?.Count ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1CronJob, DateTime?>()
            {
                Name = "Last Schedule",
                Display = x => x.Status.LastScheduleTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                Field = x => x.Status.LastScheduleTime,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }
}
