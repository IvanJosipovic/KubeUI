using System.Collections;
using Avalonia.Controls;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Resources.Workloads.v1.CronJob.Views;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.CronJob;

public sealed partial class V1CronJobConfig : ResourceConfigBase<V1CronJob>
{
    private const int KubernetesNameMaxLength = 63;
    private const string ManualInstantiateAnnotation = "cronjob.kubernetes.io/instantiate";

    private static readonly AuthorizationRequest[] s_startAuthorizationRequests =
    [
        new(typeof(V1Job), Verb.Create, null),
    ];

    public V1CronJobConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_Workloads", "Workloads");
    public override int Order => 6;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1CronJob, string>()
            {
                Name = "Schedule",
                Field = x => x.Spec.Schedule,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1CronJob, bool>()
            {
                Name = "Suspend",
                Field = x => x.Spec.Suspend ?? false,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1CronJob, int>()
            {
                Name = "Active",
                Field = x => x.Status?.Active?.Count ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1CronJob, DateTime?>()
            {
                Name = "Last Schedule",
                Display = x => x.Status?.LastScheduleTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                Field = x => x.Status.LastScheduleTime,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override IEnumerable<AuthorizationRequest> AuthorizationRequests()
    {
        return base.AuthorizationRequests().Concat(s_startAuthorizationRequests);
    }

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1CronJob>? selectedItems)
    {
        return [
            new()
            {
                Header = GetResourceString("V1CronJobConfig_Start", "Start"),
                FluentIcon = Icon.Play,
                Command = StartCommand,
                CommandParameter = selectedItems?.ToList(),
            },
        ];
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task Start(IList items)
    {
        var exceptions = new List<Exception>();
        DateTimeOffset timestamp = DateTimeOffset.UtcNow;

        foreach (V1CronJob cronJob in items.Cast<V1CronJob>().ToList())
        {
            try
            {
                V1Job job = CreateJobFromCronJob(cronJob, timestamp);
                await Cluster.AddOrUpdateResource(job).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                Utilities.HandleException(_logger, _notificationManager, ex, GetResourceString("V1CronJobConfig_Start_Error", "Error Starting CronJob"), sendNotification: true);
            }
        }

        if (exceptions.Count > 0)
        {
            _logger.LogError(new AggregateException(exceptions), "Error Starting CronJobs");
        }
    }

    private bool CanStart(IList? items)
    {
        if (items == null || items.Count == 0)
        {
            return false;
        }

        foreach (var item in items.Cast<V1CronJob>().ToList().GroupBy(x => x.Namespace()))
        {
            if (!Cluster.CanI<V1Job>(Verb.Create, item.Key))
            {
                return false;
            }
        }

        return true;
    }

    internal static V1Job CreateJobFromCronJob(V1CronJob cronJob, DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(cronJob);

        IDictionary<string, string> annotations = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ManualInstantiateAnnotation] = "manual",
        };

        if (cronJob.Spec.JobTemplate.Metadata?.Annotations != null)
        {
            foreach (var (key, value) in cronJob.Spec.JobTemplate.Metadata.Annotations)
            {
                annotations[key] = value;
            }
        }

        V1ObjectMeta metadata = new()
        {
            Name = BuildManualJobName(cronJob, timestamp),
            NamespaceProperty = cronJob.Namespace(),
            Labels = CopyDictionary(cronJob.Spec.JobTemplate.Metadata?.Labels),
            Annotations = annotations,
        };

        string? uid = cronJob.Uid();
        if (!string.IsNullOrWhiteSpace(uid))
        {
            metadata.OwnerReferences =
            [
                new()
                {
                    ApiVersion = V1CronJob.KubeGroup + "/" + V1CronJob.KubeApiVersion,
                    Kind = V1CronJob.KubeKind,
                    Name = cronJob.Name(),
                    Uid = uid,
                    Controller = true,
                },
            ];
        }

        return new V1Job
        {
            ApiVersion = V1Job.KubeGroup + "/" + V1Job.KubeApiVersion,
            Kind = V1Job.KubeKind,
            Metadata = metadata,
            Spec = cronJob.Spec.JobTemplate.Spec,
        };
    }

    private static string BuildManualJobName(V1CronJob cronJob, DateTimeOffset timestamp)
    {
        string suffix = $"-manual-{timestamp:yyyyMMddHHmmssfffffff}";
        string baseName = cronJob.Name();
        int maxBaseLength = KubernetesNameMaxLength - suffix.Length;

        if (baseName.Length > maxBaseLength)
        {
            baseName = baseName[..maxBaseLength].TrimEnd('-');
        }

        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "cronjob";
        }

        return baseName + suffix;
    }

    private static Dictionary<string, string> CopyDictionary(IDictionary<string, string>? source)
    {
        return source == null
            ? []
            : new Dictionary<string, string>(source, StringComparer.Ordinal);
    }

    private static string GetResourceString(string resourceKey, string fallback)
    {
        return ResourceStrings.GetString(resourceKey) ?? fallback;
    }

    public override Control[] Properties(V1CronJob resource) => [new PropertiesView()];
}

