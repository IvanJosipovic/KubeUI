using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s.Models;
using KubeUI.Client;
using Scrutor;
using static KubeUI.Client.Cluster;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1Deployment>>(ServiceLifetime.Transient)]
public sealed partial class V1DeploymentConfig : ResourceConfigBase<V1Deployment>, IInitializeCluster
{
    private ICluster _cluster;
    private IFactory _factory;

    public override string Category => "Workloads";

    public override int Order => 1;

    public V1DeploymentConfig(IFactory factory)
    {
        _factory = factory;
    }

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1Deployment, int>()
                {
                    Name = "Pods",
                    Display = x => $"{x.Status?.AvailableReplicas.GetValueOrDefault()}/{x.Spec?.Replicas}",
                    Field = x => x.Status.AvailableReplicas.GetValueOrDefault(),
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Deployment, int>()
                {
                    Name = "Replicas",
                    Display = x => x.Spec.Replicas.GetValueOrDefault().ToString(),
                    Field = x => x.Spec.Replicas.GetValueOrDefault(),
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Deployment, string>()
                {
                    Name = "Available",
                    Field = x => x.Status.Conditions == null ? "" : x.Status.Conditions.FirstOrDefault(x => x.Type == "Available")?.Status ?? "",
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                AgeColumn(),
            ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [
                new()
                {
                    Header = "Restart",
                    IconResource = "arrow_sync_regular",
                    CommandPath = nameof(ResourceListViewModel<V1Deployment>.RestartDeploymentCommand),
                    CommandParameterPath = "SelectedItem.Value"
                },
        ];
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
    }

    public override Control[]? Properties(V1Deployment resource)
    {
        return null;
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
                await Cluster.Client.AppsV1.PatchNamespacedDeploymentAsync(new V1Patch(s_restartControllerPatch, V1Patch.PatchType.MergePatch), deployment.Metadata.Name, deployment.Metadata.NamespaceProperty);
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
