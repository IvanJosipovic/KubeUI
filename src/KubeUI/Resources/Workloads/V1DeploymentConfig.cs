using Avalonia.Controls.Notifications;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using KubeUI.Client;
using Scrutor;
using static KubeUI.Client.Cluster;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1Deployment>>(ServiceLifetime.Transient)]
public sealed partial class V1DeploymentConfig : ResourceConfigBase<V1Deployment>
{
    private readonly ILogger<V1DaemonSetConfig> _logger;
    private readonly IDialogService _dialogService;
    private readonly INotificationManager _notificationManager;
    private readonly IFactory _factory;

    public override string Category => "Workloads";

    public override int Order => 1;

    public V1DeploymentConfig(ILogger<V1DaemonSetConfig> logger, IDialogService dialogService, INotificationManager notificationManager, IFactory factory)
    {
        _logger = logger;
        _dialogService = dialogService;
        _notificationManager = notificationManager;
        _factory = factory;
    }

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Deployment, int>()
            {
                Name = "Pods",
                Display = x => $"{x.Status?.AvailableReplicas.GetValueOrDefault()}/{x.Spec?.Replicas}",
                Field = x => x.Status.AvailableReplicas.GetValueOrDefault(),
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1Deployment, int>()
            {
                Name = "Replicas",
                Display = x => x.Spec.Replicas.GetValueOrDefault().ToString(),
                Field = x => x.Spec.Replicas.GetValueOrDefault(),
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1Deployment, string>()
            {
                Name = "Available",
                Field = x => x.Status.Conditions == null ? "" : x.Status.Conditions.FirstOrDefault(x => x.Type == "Available")?.Status ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceMenuItem> MenuItems()
    {
        return [
            new()
            {
                Header = "Restart",
                IconResource = "arrow_sync_regular",
                CommandPath = nameof(RestartDeploymentCommand),
                CommandParameterPath = Utilities.PathBuilder<ResourceListViewModel<V1Deployment>>(x => x.SelectedItem.Value)
            },
        ];
    }

    [RelayCommand(CanExecute = nameof(CanRestartDeployment))]
    private async Task RestartDeployment(V1Deployment deployment)
    {
        try
        {
            ContentDialogSettings settings = new()
            {
                Title = Assets.Resources.ResourceListViewModel_Restart_Title,
                Content = string.Format(Assets.Resources.ResourceListViewModel_Restart_Content, deployment.Name()),
                PrimaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Primary,
                SecondaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Secondary,
                DefaultButton = ContentDialogButton.Secondary
            };

            var result = await _dialogService.ShowContentDialogAsync(this, settings);

            if (result == ContentDialogResult.Primary)
            {
                await Cluster.Client.AppsV1.PatchNamespacedDeploymentAsync(new V1Patch(sRestartControllerPatch, V1Patch.PatchType.MergePatch), deployment.Metadata.Name, deployment.Metadata.NamespaceProperty);
            }
        }
        catch (Exception ex)
        {
            Utilities.HandleException(_logger, _notificationManager, ex, "Error Restarting Deployment", sendNotification: true);
        }
    }

    private bool CanRestartDeployment(V1Deployment deployment)
    {
        return deployment != null && Cluster.CanI<V1Deployment>(Verb.Patch, deployment.Namespace());
    }
}
