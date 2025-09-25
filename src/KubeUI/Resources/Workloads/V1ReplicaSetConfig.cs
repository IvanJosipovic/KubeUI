using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using static KubeUI.Client.Cluster;

namespace KubeUI.Resources.Workloads;

public sealed partial class V1ReplicaSetConfig : ResourceConfigBase<V1ReplicaSet>
{
    public override bool IsNamespaced => true;
    public override string Category => "Workloads";

    public override int Order => 4;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1ReplicaSet, int>()
            {
                Name = "Desired",
                Display = x => (x.Spec.Replicas ?? 0).ToString(),
                Field = x => x.Spec.Replicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1ReplicaSet, int>()
            {
                Name = "Current",
                Display = x => x.Status.Replicas.ToString(),
                Field = x => x.Status.Replicas,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1ReplicaSet, int>()
            {
                Name = "Ready",
                Display = x => x.Status.Replicas.ToString(),
                Field = x => x.Status.ReadyReplicas ?? 0,
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
                CommandPath = nameof(RestartReplicaSetCommand),
                CommandParameterPath = Utilities.PathBuilder<ResourceListViewModel<V1Deployment>>(x => x.SelectedItem.Value),
            },
        ];
    }

    [RelayCommand(CanExecute = nameof(CanRestartReplicaSet))]
    private async Task RestartReplicaSet(V1ReplicaSet replicaSet)
    {
        try
        {
            ContentDialogSettings settings = new()
            {
                Title = Assets.Resources.ResourceListViewModel_Restart_Title,
                Content = string.Format(Assets.Resources.ResourceListViewModel_Restart_Content, replicaSet.Name()),
                PrimaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Primary,
                SecondaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Secondary,
                DefaultButton = ContentDialogButton.Secondary
            };

            var result = await _dialogService.ShowContentDialogAsync(this, settings);

            if (result == ContentDialogResult.Primary)
            {
                await Cluster.Client.AppsV1.PatchNamespacedReplicaSetAsync(new V1Patch(sRestartControllerPatch, V1Patch.PatchType.MergePatch), replicaSet.Metadata.Name, replicaSet.Metadata.NamespaceProperty);
            }
        }
        catch (Exception ex)
        {
            Utilities.HandleException(_logger, _notificationManager, ex, "Error Restarting ReplicaSet", sendNotification: true);
        }
    }

    private bool CanRestartReplicaSet(V1ReplicaSet replicaSet)
    {
        return replicaSet != null && Cluster.CanI<V1ReplicaSet>(Verb.Patch, replicaSet.Namespace());
    }
}
