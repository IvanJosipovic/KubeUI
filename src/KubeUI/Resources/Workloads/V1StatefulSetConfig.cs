using Avalonia.Controls.Notifications;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.ViewModels;
using Scrutor;
using static KubeUI.Client.Cluster;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1StatefulSet>>(ServiceLifetime.Transient)]
public sealed partial class V1StatefulSetConfig : ResourceConfigBase<V1StatefulSet>, IInitializeCluster
{
    private readonly ILogger<V1StatefulSetConfig> _logger;
    private readonly IDialogService _dialogService;
    private readonly INotificationManager _notificationManager;
    private ICluster _cluster;

    public override string Category => "Workloads";

    public override int Order => 3;

    public V1StatefulSetConfig(ILogger<V1StatefulSetConfig> logger, IDialogService dialogService, INotificationManager notificationManager)
    {
        _logger = logger;
        _dialogService = dialogService;
        _notificationManager = notificationManager;
    }

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<V1StatefulSet, int>()
            {
                Name = "Replicas",
                Display = x => x.Status.Replicas.ToString(),
                Field = x => x.Status.Replicas,
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
                CommandPath = nameof(ResourceListViewModel<V1Deployment>.ResourceConfig) + "." + nameof(RestartStatefulSetCommand),
                CommandParameterPath = "SelectedItem.Value"
            },
        ];
    }

    public override Control[]? Properties(V1StatefulSet resource)
    {
        return null;
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
                await _cluster.Client.AppsV1.PatchNamespacedStatefulSetAsync(new V1Patch(s_restartControllerPatch, V1Patch.PatchType.MergePatch), statefulSet.Metadata.Name, statefulSet.Metadata.NamespaceProperty);
            }
        }
        catch (Exception ex)
        {
            Utilities.HandleException(_logger, _notificationManager, ex, "Error Restarting StatefulSet", sendNotification: true);
        }
    }

    private bool CanRestartStatefulSet(V1StatefulSet statefulSet)
    {
        return statefulSet != null && _cluster.CanI<V1StatefulSet>(Verb.Patch, statefulSet.Namespace());
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
    }
}
