using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Controls.Docking;
using KubeUI.Features.Resources.Common;
using KubeUI.Features.Resources.Infrastructure;
using KubeUI.Features.Resources.Workloads;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Resources.Workloads.v1.Deployment;

public sealed partial class V1DeploymentConfig : ScalableWorkloadConfig<V1Deployment>
{
    public V1DeploymentConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.WorkloadsId);

    public override int Order => 1;

    protected override int GetDesiredReplicas(V1Deployment workload) => workload.Spec?.Replicas ?? 0;

    protected override Task PatchReplicasAsync(V1Deployment workload, int replicas)
        => Cluster.Runtime.Client!.AppsV1.PatchNamespacedDeploymentScaleAsync(
            CreateScalePatch(replicas), workload.Name(), workload.Namespace());

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Deployment, int>()
            {
                Key = "pods",
                Name = Assets.Resources.V1DeploymentConfig_Pods!,
                Display = x => $"{x.Status?.AvailableReplicas ?? 0}/{x.Spec?.Replicas ?? 0}",
                Field = x => x.Status?.AvailableReplicas ?? 0,
                WidthMode = DynamicTableViewWidthMode.Header
            },
            new ResourceListColumn<V1Deployment, int>()
            {
                Key = "replicas",
                Name = Assets.Resources.V1DeploymentConfig_Replicas!,
                Field = x => x.Spec.Replicas ?? 0,
                WidthMode = DynamicTableViewWidthMode.Header
            },
            new ResourceListColumn<V1Deployment, string>()
            {
                Key = "available",
                Name = Assets.Resources.V1DeploymentConfig_Available!,
                Field = x => x.Status?.Conditions?.FirstOrDefault(x => x.Type == "Available")?.Status ?? "",
                WidthMode = DynamicTableViewWidthMode.Header
            },
            AgeColumn(),
        ];
    }

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1Deployment>? selectedItems)
    {
        return [
            CreatePodLogsMenuItem(selectedItems),
            CreateScaleMenuItem(selectedItems),
            new()
            {
                Title = Assets.Resources.V1DeploymentConfig_MenuItem_Restart,
                FluentIcon = Icon.ArrowSync,
                Command = RestartCommand,
                CommandParameter = selectedItems?.ToList()
            },
            new()
            {
                Title = Assets.Resources.V1DeploymentConfig_Pause,
                FluentIcon = Icon.Pause,
                Command = PauseCommand,
                CommandParameter = selectedItems?.ToList(),
            },
            new()
            {
                Title = Assets.Resources.V1DeploymentConfig_Resume,
                FluentIcon = Icon.ArrowRight,
                Command = ResumeCommand,
                CommandParameter = selectedItems?.ToList(),
            },
            new()
            {
                Title = Assets.Resources.V1DeploymentConfig_RolloutHistory,
                FluentIcon = Icon.History,
                Command = OpenRolloutHistoryCommand,
                CommandParameter = selectedItems?.ToList(),
            },
        ];
    }

    [RelayCommand(CanExecute = nameof(CanOpenRolloutHistory))]
    private async Task OpenRolloutHistory(IList items)
    {
        var deployment = items.Cast<V1Deployment>().Single();
        var viewModel = ServiceProvider.GetRequiredService<DeploymentRolloutHistoryViewModel>();
        viewModel.Initialize(Cluster, deployment);
        await viewModel.LoadAsync();
        _factory.AddToDocuments(viewModel);
    }

    private bool CanOpenRolloutHistory(IList? items)
        => items is { Count: 1 }
            && items[0] is V1Deployment deployment
            && !string.IsNullOrWhiteSpace(deployment.Name())
            && !string.IsNullOrWhiteSpace(deployment.Namespace())
            && Cluster.Runtime.Permissions.CanI<V1Deployment>(Verb.Get, deployment.Namespace())
            && Cluster.Runtime.Permissions.CanI<V1ReplicaSet>(Verb.List, deployment.Namespace());

    [RelayCommand(CanExecute = nameof(CanPause))]
    private Task Pause(IList items) => SetPausedAsync(items, true);

    [RelayCommand(CanExecute = nameof(CanResume))]
    private Task Resume(IList items) => SetPausedAsync(items, false);

    private async Task SetPausedAsync(IList items, bool paused)
    {
        var deployments = GetPauseCandidates(items, paused);
        if (deployments.Count == 0)
        {
            return;
        }

        var result = await _dialogService.ShowContentDialogAsync(this, new ContentDialogSettings
        {
            Title = string.Format(paused ? Assets.Resources.V1DeploymentConfig_PauseTitle : Assets.Resources.V1DeploymentConfig_ResumeTitle, deployments.Count),
            Content = paused ? Assets.Resources.V1DeploymentConfig_PauseContent : Assets.Resources.V1DeploymentConfig_ResumeContent,
            PrimaryButtonText = paused ? Assets.Resources.V1DeploymentConfig_Pause : Assets.Resources.V1DeploymentConfig_Resume,
            SecondaryButtonText = SharedAssets.Resources.Shared_Cancel,
            DefaultButton = FAContentDialogButton.Secondary,
        });
        if (result != FAContentDialogResult.Primary)
        {
            return;
        }

        deployments = GetPauseCandidates(deployments, paused);
        var patch = new V1Patch(paused ? "{\"spec\":{\"paused\":true}}" : "{\"spec\":{\"paused\":false}}", V1Patch.PatchType.MergePatch);
        foreach (var deployment in deployments)
        {
            try
            {
                await Cluster.Runtime.Client!.AppsV1.PatchNamespacedDeploymentAsync(patch, deployment.Name(), deployment.Namespace());
            }
            catch (Exception ex)
            {
                ResourceUtilities.HandleException(_logger, _notificationManager, ex, string.Format(paused ? Assets.Resources.V1DeploymentConfig_PauseError : Assets.Resources.V1DeploymentConfig_ResumeError, deployment.Namespace(), deployment.Name()), sendNotification: true);
            }
        }
    }

    private bool CanPause(IList? items) => CanChangePauseState(items, paused: true);

    private bool CanResume(IList? items) => CanChangePauseState(items, paused: false);

    private bool CanChangePauseState(IList? items, bool paused)
        => GetPauseCandidates(items, paused).Count > 0;

    private List<V1Deployment> GetPauseCandidates(IList? items, bool paused)
    {
        if (items is not { Count: > 0 } || Cluster is null)
        {
            return [];
        }

        var candidates = new List<V1Deployment>(items.Count);
        foreach (var item in items)
        {
            if (item is not V1Deployment deployment || (deployment.Spec?.Paused ?? false) == paused)
            {
                continue;
            }

            var name = deployment.Name();
            var @namespace = deployment.Namespace();
            if (!string.IsNullOrWhiteSpace(name)
                && !string.IsNullOrWhiteSpace(@namespace)
                && Cluster.Runtime.Permissions.CanI<V1Deployment>(Verb.Patch, @namespace))
            {
                candidates.Add(deployment);
            }
        }

        return candidates;
    }

    /// <summary>Requests permission to read pod logs for deployment workloads.</summary>
    public override IEnumerable<AuthorizationRequest> AuthorizationRequests()
    {
        return base.AuthorizationRequests().Append(
            new AuthorizationRequest(GroupApiVersionKind.From<V1Pod>(), Verb.Get, "log"))
            .Append(new AuthorizationRequest(GroupApiVersionKind.From<V1Deployment>(), Verb.Get, null))
            .Append(new AuthorizationRequest(GroupApiVersionKind.From<V1ReplicaSet>(), Verb.List, null));
    }

    public override Control[] Properties(V1Deployment resource) => [new PropertiesView()];
}
