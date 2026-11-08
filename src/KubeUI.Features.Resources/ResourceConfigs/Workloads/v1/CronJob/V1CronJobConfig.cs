using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Features.Resources.Common;
using KubeUI.Features.Resources.Infrastructure;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Resources.Workloads.v1.CronJob;

public sealed partial class V1CronJobConfig : ResourceConfig<V1CronJob>
{
    private readonly TimeProvider _timeProvider;
    private const int KubernetesNameMaxLength = 63;
    private const string ManualInstantiateAnnotation = "cronjob.kubernetes.io/instantiate";

    private static readonly AuthorizationRequest[] s_startAuthorizationRequests =
    [
        new(GroupApiVersionKind.From<V1Job>(), Verb.Create, null),
    ];

    public V1CronJobConfig(IServiceProvider serviceProvider, TimeProvider timeProvider)
        : base(serviceProvider)
    {
        _timeProvider = timeProvider;
    }
    public override bool IsNamespaced => true;
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.WorkloadsId);
    public override int Order => 6;

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1CronJob, string>()
            {
                Key = "schedule",
                Name = L10n.Instance.V1CronJobConfig_Schedule.CurrentValue()!,
                Field = x => x.Spec.Schedule,
                WidthMode = DynamicTableViewWidthMode.Header
            },
            new ResourceListColumn<V1CronJob, bool>()
            {
                Key = "suspend",
                Name = L10n.Instance.V1CronJobConfig_Suspend.CurrentValue()!,
                Field = x => x.Spec.Suspend ?? false,
                WidthMode = DynamicTableViewWidthMode.Header
            },
            new ResourceListColumn<V1CronJob, int>()
            {
                Key = "active",
                Name = L10n.Instance.V1CronJobConfig_Active.CurrentValue()!,
                Field = x => x.Status?.Active?.Count ?? 0,
                WidthMode = DynamicTableViewWidthMode.Header
            },
            new ResourceListColumn<V1CronJob, DateTime?>()
            {
                Key = "last-schedule",
                Name = L10n.Instance.V1CronJobConfig_Last_Schedule.CurrentValue()!,
                Display = x => x.Status?.LastScheduleTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                Field = x => x.Status.LastScheduleTime,
                WidthMode = DynamicTableViewWidthMode.Header
            },
            AgeColumn(),
        ];
    }

    /// <summary>Requests permissions to create jobs and read pod logs for cron job actions.</summary>
    public override IEnumerable<AuthorizationRequest> AuthorizationRequests()
    {
        return base.AuthorizationRequests()
            .Concat(s_startAuthorizationRequests)
            .Append(new AuthorizationRequest(GroupApiVersionKind.From<V1Pod>(), Verb.Get, "log"));
    }

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1CronJob>? selectedItems)
    {
        return [
            CreatePodLogsMenuItem(selectedItems),
            new()
            {
                Title = L10n.Instance.V1CronJobConfig_Start.CurrentValue()!,
                FluentIcon = Icon.Play,
                Command = StartCommand,
                CommandParameter = selectedItems?.ToList(),
            },
            new()
            {
                Title = L10n.Instance.V1CronJobConfig_Suspend.CurrentValue()!,
                FluentIcon = Icon.Pause,
                Command = SuspendCommand,
                CommandParameter = selectedItems?.ToList(),
            },
            new()
            {
                Title = L10n.Instance.V1CronJobConfig_Resume.CurrentValue()!,
                FluentIcon = Icon.ArrowRight,
                Command = ResumeCommand,
                CommandParameter = selectedItems?.ToList(),
            },
        ];
    }

    [RelayCommand(CanExecute = nameof(CanSuspend))]
    private Task Suspend(IList items) => SetSuspendedAsync(items, true);

    [RelayCommand(CanExecute = nameof(CanResume))]
    private Task Resume(IList items) => SetSuspendedAsync(items, false);

    private async Task SetSuspendedAsync(IList items, bool suspended)
    {
        var cronJobs = GetSuspendCandidates(items, suspended);
        if (cronJobs.Count == 0)
        {
            return;
        }

        var result = await _dialogService.ShowContentDialogAsync(this, new ContentDialogSettings
        {
            Title = suspended ? L10n.Instance.V1CronJobConfig_SuspendTitle.CurrentValue()! : L10n.Instance.V1CronJobConfig_ResumeTitle.CurrentValue()!,
            Content = string.Format(suspended ? L10n.Instance.V1CronJobConfig_SuspendContent.CurrentValue()! : L10n.Instance.V1CronJobConfig_ResumeContent.CurrentValue()!, cronJobs.Count),
            PrimaryButtonText = L10n.Instance.ResourceListView_Delete_Primary.CurrentValue()!,
            SecondaryButtonText = SharedL10n.Instance.Shared_Cancel.CurrentValue()!,
            DefaultButton = FAContentDialogButton.Secondary,
        });
        if (result != FAContentDialogResult.Primary)
        {
            return;
        }

        cronJobs = GetSuspendCandidates(cronJobs, suspended);
        var failures = new List<Exception>();
        var patch = new V1Patch(suspended ? "{\"spec\":{\"suspend\":true}}" : "{\"spec\":{\"suspend\":false}}", V1Patch.PatchType.MergePatch);
        foreach (var cronJob in cronJobs)
        {
            try
            {
                await Cluster.Runtime.Client!.BatchV1.PatchNamespacedCronJobAsync(patch, cronJob.Name(), cronJob.Namespace());
            }
            catch (Exception ex)
            {
                failures.Add(ex);
                ResourceUtilities.HandleException(_logger, _notificationManager, ex, string.Format(suspended ? L10n.Instance.V1CronJobConfig_SuspendError.CurrentValue()! : L10n.Instance.V1CronJobConfig_ResumeError.CurrentValue()!, cronJob.Namespace(), cronJob.Name()), sendNotification: true);
            }
        }

        if (failures.Count > 0)
        {
            _logger.LogError(new AggregateException(failures), "Error changing CronJob suspension state");
        }
    }

    private bool CanSuspend(IList? items) => CanChangeSuspendState(items, suspended: true);

    private bool CanResume(IList? items) => CanChangeSuspendState(items, suspended: false);

    private bool CanChangeSuspendState(IList? items, bool suspended)
        => GetSuspendCandidates(items, suspended).Count > 0;

    private List<V1CronJob> GetSuspendCandidates(IList? items, bool suspended)
    {
        if (items is not { Count: > 0 } || Cluster is null)
        {
            return [];
        }

        var candidates = new List<V1CronJob>(items.Count);
        foreach (var item in items)
        {
            if (item is not V1CronJob cronJob || (cronJob.Spec?.Suspend ?? false) == suspended)
            {
                continue;
            }

            var name = cronJob.Name();
            var @namespace = cronJob.Namespace();
            if (!string.IsNullOrWhiteSpace(name)
                && !string.IsNullOrWhiteSpace(@namespace)
                && Cluster.Runtime.Permissions.CanI<V1CronJob>(Verb.Patch, @namespace))
            {
                candidates.Add(cronJob);
            }
        }

        return candidates;
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task Start(IList items)
    {
        var exceptions = new List<Exception>();
        var timestamp = _timeProvider.GetUtcNow();

        foreach (var cronJob in items.Cast<V1CronJob>().ToList())
        {
            try
            {
                var job = CreateJobFromCronJob(cronJob, timestamp);
                await Cluster.Runtime.AddOrUpdateResource(job).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                ResourceUtilities.HandleException(_logger, _notificationManager, ex, L10n.Instance.V1CronJobConfig_Start_Error.CurrentValue()!, sendNotification: true);
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
            if (!Cluster.Runtime.Permissions.CanI<V1Job>(Verb.Create, item.Key))
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

        var uid = cronJob.Uid();
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
        var suffix = $"-manual-{timestamp:yyyyMMddHHmmssfffffff}";
        var baseName = cronJob.Name();
        var maxBaseLength = KubernetesNameMaxLength - suffix.Length;

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

    public override Control[] Properties(V1CronJob resource) => [new PropertiesView()];
}
