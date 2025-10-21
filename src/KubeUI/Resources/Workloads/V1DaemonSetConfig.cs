using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using static KubeUI.Client.Cluster;

namespace KubeUI.Resources.Workloads;

public sealed partial class V1DaemonSetConfig : ResourceConfigBase<V1DaemonSet>
{
    public override bool IsNamespaced => true;
    public override string Category => "Workloads";

    public override int Order => 2;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1DaemonSet, int>()
            {
                Name = "Pods",
                Display = x => x.Status.NumberReady.ToString(),
                Field = x => x.Status.NumberReady,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1DaemonSet, string>()
            {
                Name = "Node Selector",
                Display = x => x.Spec.Selector.MatchLabels.Select(z => z.Key + "=" + z.Value).Aggregate((x,y) => x + ", " + y),
                Field = x => x.Spec.Selector.MatchLabels.Select(z => z.Key + "=" + z.Value).Aggregate((x,y) => x + ", " + y),
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
                CommandPath = nameof(RestartDaemonSetCommand),
                CommandParameterPath = Utilities.PathBuilder<ResourceListViewModel<V1DaemonSet>>(x => x.SelectedItem.Value)
            },
        ];
    }

    [RelayCommand(CanExecute = nameof(CanRestartDaemonSet))]
    private async Task RestartDaemonSet(V1DaemonSet daemonSet)
    {
        try
        {
            ContentDialogSettings settings = new()
            {
                Title = Assets.Resources.ResourceListViewModel_Restart_Title,
                Content = string.Format(Assets.Resources.ResourceListViewModel_Restart_Content, daemonSet.Name()),
                PrimaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Primary,
                SecondaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Secondary,
                DefaultButton = ContentDialogButton.Secondary
            };

            var result = await _dialogService.ShowContentDialogAsync(this, settings);

            if (result == ContentDialogResult.Primary)
            {
                await Cluster.Client.AppsV1.PatchNamespacedDaemonSetAsync(new V1Patch(sRestartControllerPatch, V1Patch.PatchType.MergePatch), daemonSet.Metadata.Name, daemonSet.Metadata.NamespaceProperty);
            }
        }
        catch (Exception ex)
        {
            Utilities.HandleException(_logger, _notificationManager, ex, "Error Restarting DaemonSet", sendNotification: true);
        }
    }

    private bool CanRestartDaemonSet(V1DaemonSet daemonSet)
    {
        return daemonSet != null && Cluster.CanI<V1DaemonSet>(Verb.Patch, daemonSet.Namespace());
    }
}
