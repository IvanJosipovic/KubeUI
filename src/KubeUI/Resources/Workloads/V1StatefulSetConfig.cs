using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using static KubeUI.Client.Cluster;

namespace KubeUI.Resources.Workloads;

public sealed partial class V1StatefulSetConfig : ResourceConfigBase<V1StatefulSet>
{
    public override bool IsNamespaced => true;
    public override string Category => "Workloads";

    public override int Order => 3;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1StatefulSet, int>()
            {
                Name = "Replicas",
                Display = x => x.Status.Replicas.ToString(),
                Field = x => x.Status.Replicas,
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
                CommandPath = nameof(RestartStatefulSetCommand),
                CommandParameterPath = "SelectedItem.Value"
            },
        ];
    }

    [RelayCommand(CanExecute = nameof(CanRestartStatefulSet))]
    private async Task RestartStatefulSet(V1StatefulSet statefulSet)
    {
        try
        {
            ContentDialogSettings settings = new()
            {
                Title = Assets.Resources.ResourceListViewModel_Restart_Title,
                Content = string.Format(Assets.Resources.ResourceListViewModel_Restart_Content, statefulSet.Name()),
                PrimaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Primary,
                SecondaryButtonText = Assets.Resources.ResourceListViewModel_Restart_Secondary,
                DefaultButton = ContentDialogButton.Secondary
            };

            var result = await _dialogService.ShowContentDialogAsync(this, settings);

            if (result == ContentDialogResult.Primary)
            {
                await Cluster.Client.AppsV1.PatchNamespacedStatefulSetAsync(new V1Patch(sRestartControllerPatch, V1Patch.PatchType.MergePatch), statefulSet.Metadata.Name, statefulSet.Metadata.NamespaceProperty);
            }
        }
        catch (Exception ex)
        {
            Utilities.HandleException(_logger, _notificationManager, ex, "Error Restarting StatefulSet", sendNotification: true);
        }
    }

    private bool CanRestartStatefulSet(V1StatefulSet statefulSet)
    {
        return statefulSet != null && Cluster.CanI<V1StatefulSet>(Verb.Patch, statefulSet.Namespace());
    }
}
