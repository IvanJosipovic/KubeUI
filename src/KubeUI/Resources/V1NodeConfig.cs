using Avalonia.Controls.Notifications;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using KubeUI.Client.Informer;
using KubeUI.Resources.Workloads.Pod;
using Scrutor;
using static KubeUI.Client.Cluster;

namespace KubeUI.Resources;

[ServiceDescriptor<ResourceConfigBase<V1Node>>(ServiceLifetime.Transient)]
public sealed partial class V1NodeConfig : ResourceConfigBase<V1Node>
{
    private readonly ILogger<V1DaemonSetConfig> _logger;
    private readonly IDialogService _dialogService;
    private readonly INotificationManager _notificationManager;

    public override bool ShowNamespaces => false;

    public override int Order => 5;

    public V1NodeConfig(ILogger<V1DaemonSetConfig> logger, IDialogService dialogService, INotificationManager notificationManager)
    {
        _logger = logger;
        _dialogService = dialogService;
        _notificationManager = notificationManager;
    }

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1Node, string>()
            {
                Name = "Instance Type",
                Field = x => x.Metadata.Labels.TryGetValue("node.kubernetes.io/instance-type", out var value) ? value : "",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<V1Node, decimal>()
            {
                Name = "CPU",
                Field = x => x.Status?.Capacity?.TryGetValue("cpu", out var value) == true ? value.ToDecimal() : 0,
                Display = x => x.Status?.Capacity?.TryGetValue("cpu", out var value) == true ? value.ToDecimal().ToString("0.##") + "c" : "0c",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1Node, decimal>()
            {
                Name = "Memory",
                Field = x => x.Status?.Capacity?.TryGetValue("memory", out var value) == true ? value.ToDecimal() : 0,
                Display = x => x.Status?.Capacity?.TryGetValue("memory", out var value) == true ? (value.ToDecimal() / 1048576 / 1024).ToString("0.##") + "Gi" : "0Gi",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1Node, decimal>()
            {
                Name = "Disk",
                Field = x => x.Status?.Capacity?.TryGetValue("ephemeral-storage", out var value) == true ? value.ToDecimal() : 0,
                Display = x => x.Status?.Capacity?.TryGetValue("ephemeral-storage", out var value) == true ? (value.ToDecimal() / 1048576 / 1024).ToString("0.##") + "Gi" : "0",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<V1Node, string>()
            {
                Name = "Taints",
                Field = x => x?.Spec?.Taints?.Select(x => $"{x.Key}={x.Effect}").Aggregate((x,y) => $"{x}, {y}") ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1Node, string>()
            {
                Name = "Version",
                Field = x => x.Status.NodeInfo.KubeletVersion,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1Node, string>()
            {
                Name = "Status",
                Field = x => x.Status.Conditions.FirstOrDefault(x => x.Type == "Ready")?.Reason ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceMenuItem> MenuItems()
    {
        return [
            new()
            {
                Header = "Cordon",
                IconResource = "stop_regular",
                CommandPath = nameof(CordonNodeCommand),
                CommandParameterPath = "SelectedItems",
            },
            new()
            {
                Header = "UnCordon",
                IconResource = "play_regular",
                CommandPath = nameof(UnCordonNodeCommand),
                CommandParameterPath = "SelectedItems",
            },
            new()
            {
                Header = "Drain",
                IconResource = "arrow_sync_regular",
                CommandPath = nameof(DrainNodeCommand),
                CommandParameterPath = "SelectedItems",
            },
        ];
    }

    [RelayCommand(CanExecute = nameof(CanCordonNode))]
    private async Task CordonNode(IList items)
    {
        ContentDialogSettings settings = new()
        {
            Title = Assets.Resources.ResourceListViewModel_CordonNode_Title,
            Content = string.Format(Assets.Resources.ResourceListViewModel_CordonNode_Content, items.Count),
            PrimaryButtonText = Assets.Resources.ResourceListViewModel_CordonNode_Primary,
            SecondaryButtonText = Assets.Resources.ResourceListViewModel_CordonNode_Secondary,
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        var patch = $$"""
        {
            "spec": {
                "unschedulable": true
            }
        }
        """;

        if (result == ContentDialogResult.Primary)
        {
            foreach (var item in items.Cast<KeyValuePair<NamespacedName, V1Node>>().ToList())
            {
                try
                {
                    await Cluster.Client.CoreV1.PatchNodeAsync(new V1Patch(patch, V1Patch.PatchType.MergePatch), item.Key.Name, item.Key.Namespace);
                }
                catch (Exception ex)
                {
                    Utilities.HandleException(_logger, _notificationManager, ex, "Error Cordoning Node", sendNotification: true);
                }
            }
        }
    }

    private bool CanCordonNode(IList? items)
    {
        return items?.Count > 0 && Cluster.CanI<V1Node>(Verb.Patch);
    }

    [RelayCommand(CanExecute = nameof(CanUnCordonNode))]
    private async Task UnCordonNode(IList items)
    {
        ContentDialogSettings settings = new()
        {
            Title = Assets.Resources.ResourceListViewModel_UnCordonNode_Title,
            Content = string.Format(Assets.Resources.ResourceListViewModel_UnCordonNode_Content, items.Count),
            PrimaryButtonText = Assets.Resources.ResourceListViewModel_UnCordonNode_Primary,
            SecondaryButtonText = Assets.Resources.ResourceListViewModel_UnCordonNode_Secondary,
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        var patch = $$"""
        {
            "spec": {
                "unschedulable": false
            }
        }
        """;

        if (result == ContentDialogResult.Primary)
        {
            foreach (var item in items.Cast<KeyValuePair<NamespacedName, V1Node>>().ToList())
            {
                try
                {
                    await Cluster.Client.CoreV1.PatchNodeAsync(new V1Patch(patch, V1Patch.PatchType.MergePatch), item.Key.Name, item.Key.Namespace);
                }
                catch (Exception ex)
                {
                    Utilities.HandleException(_logger, _notificationManager, ex, "Error UnCordoning Node", sendNotification: true);
                }
            }
        }
    }

    private bool CanUnCordonNode(IList? items)
    {
        return items?.Count > 0 && Cluster.CanI<V1Node>(Verb.Patch);
    }

    [RelayCommand(CanExecute = nameof(CanDrainNode))]
    private async Task DrainNode(IList items)
    {
        ContentDialogSettings settings = new()
        {
            Title = Assets.Resources.ResourceListViewModel_DrainNode_Title,
            Content = string.Format(Assets.Resources.ResourceListViewModel_DrainNode_Content, items.Count),
            PrimaryButtonText = Assets.Resources.ResourceListViewModel_DrainNode_Primary,
            SecondaryButtonText = Assets.Resources.ResourceListViewModel_DrainNode_Secondary,
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        var patch = $$"""
        {
            "spec": {
                "unschedulable": true
            }
        }
        """;

        if (result == ContentDialogResult.Primary)
        {
            foreach (var item in items.Cast<KeyValuePair<NamespacedName, V1Node>>().ToList())
            {
                try
                {
                    await Cluster.Client.CoreV1.PatchNodeAsync(new V1Patch(patch, V1Patch.PatchType.MergePatch), item.Key.Name, item.Key.Namespace);

                    var pods = await Cluster.GetObjectDictionaryAsync<V1Pod>();

                    foreach (var pod in pods)
                    {
                        if (pod.Value.Spec.NodeName == item.Value.Metadata.Name)
                        {
                            if (pod.Value.Metadata.OwnerReferences.Any(x => x.ApiVersion == V1DaemonSet.KubeGroup + "/" + V1DaemonSet.KubeApiVersion &&
                                                                            x.Kind == V1DaemonSet.KubeKind &&
                                                                            x.Controller == true &&
                                                                            x.BlockOwnerDeletion == true))
                            {
                                continue;
                            }

                            V1Eviction evict = new()
                            {
                                ApiVersion = V1Eviction.KubeGroup + "/" + V1Eviction.KubeApiVersion,
                                Kind = V1Eviction.KubeKind,
                                Metadata = new()
                                {
                                    Name = pod.Value.Metadata.Name,
                                    NamespaceProperty = pod.Value.Metadata.NamespaceProperty
                                }
                            };

                            try
                            {
                                await Cluster.Client.CoreV1.CreateNamespacedPodEvictionAsync(evict, pod.Value.Metadata.Name, pod.Value.Metadata.NamespaceProperty);
                            }
                            catch (Exception ex)
                            {
                                Utilities.HandleException(_logger, _notificationManager, ex, "Error Evicting Pod", sendNotification: true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utilities.HandleException(_logger, _notificationManager, ex, "Error Draining Node", sendNotification: true);
                }
            }
        }
    }

    private bool CanDrainNode(IList? items)
    {
        return items?.Count > 0 && Cluster.CanI<V1Node>(Verb.Patch);
    }
}
