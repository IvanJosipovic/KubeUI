using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Core.v1.Node;

public sealed partial class V1NodeConfig : ResourceConfigBase<V1Node>
{
    public V1NodeConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override int Order => 5;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            new ResourceListColumn<V1Node, string>()
            {
                Key = "instance-type",
                Name = Assets.Resources.V1NodeConfig_Instance_Type!,
                Field = x => x.Metadata.Labels.TryGetValue("node.kubernetes.io/instance-type", out var value) ? value : "",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<V1Node, decimal>()
            {
                Key = "cpu",
                Name = Assets.Resources.V1NodeConfig_CPU!,
                Field = x => x.Status?.Capacity?.TryGetValue("cpu", out var value) == true ? value.ToDecimal() : 0,
                Display = x => x.Status?.Capacity?.TryGetValue("cpu", out var value) == true ? value.ToDecimal().ToString("0.##") + "c" : "0c",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1Node, decimal>()
            {
                Key = "memory",
                Name = Assets.Resources.V1NodeConfig_Memory!,
                Field = x => x.Status?.Capacity?.TryGetValue("memory", out var value) == true ? value.ToDecimal() : 0,
                Display = x => x.Status?.Capacity?.TryGetValue("memory", out var value) == true ? (value.ToDecimal() / 1048576 / 1024).ToString("0.##") + "Gi" : "0Gi",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1Node, decimal>()
            {
                Key = "disk",
                Name = Assets.Resources.V1NodeConfig_Disk!,
                Field = x => x.Status?.Capacity?.TryGetValue("ephemeral-storage", out var value) == true ? value.ToDecimal() : 0,
                Display = x => x.Status?.Capacity?.TryGetValue("ephemeral-storage", out var value) == true ? (value.ToDecimal() / 1048576 / 1024).ToString("0.##") + "Gi" : "0Gi",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            new ResourceListColumn<V1Node, string>()
            {
                Key = "taints",
                Name = Assets.Resources.V1NodeConfig_Taints!,
                Field = x => x?.Spec?.Taints is { Count: > 0 } taints ? string.Join(", ", taints.Select(x => $"{x.Key}={x.Effect}")) : "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1Node, string>()
            {
                Key = "version",
                Name = Assets.Resources.V1NodeConfig_Version!,
                Field = x => x.Status.NodeInfo.KubeletVersion,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1Node, string>()
            {
                Key = "status",
                Name = Assets.Resources.V1NodeConfig_Status!,
                Field = x => x.Status.Conditions.FirstOrDefault(x => x.Type == "Ready")?.Reason ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToCells)
            },
            AgeColumn(),
        ];
    }

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1Node>? selectedItems)
    {
        return [
            new()
            {
                Header = "Cordon",
                FluentIcon = Icon.Stop,
                Command = CordonNodeCommand,
                CommandParameter = selectedItems?.ToList(),
            },
            new()
            {
                Header = "UnCordon",
                FluentIcon = Icon.Play,
                Command = UnCordonNodeCommand,
                CommandParameter = selectedItems?.ToList(),
            },
            new()
            {
                Header = "Drain",
                FluentIcon = Icon.ArrowSync,
                Command = DrainNodeCommand,
                CommandParameter = selectedItems?.ToList(),
            },
        ];
    }

    [RelayCommand(CanExecute = nameof(CanCordonNode))]
    private async Task CordonNode(IList items)
    {
        ContentDialogSettings settings = new()
        {
            Title = Assets.Resources.ResourceListView_CordonNode_Title,
            Content = string.Format(Assets.Resources.ResourceListView_CordonNode_Content, items.Count),
            PrimaryButtonText = Assets.Resources.ResourceListView_CordonNode_Primary,
            SecondaryButtonText = Assets.Resources.ResourceListView_CordonNode_Secondary,
            DefaultButton = FAContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        const string patch = /*lang=json*/ """
        {
            "spec": {
                "unschedulable": true
            }
        }
        """;

        if (result == FAContentDialogResult.Primary)
        {
            foreach (var item in items.Cast<V1Node>().ToList())
            {
                try
                {
                    await Cluster.Client.CoreV1.PatchNodeAsync(new V1Patch(patch, V1Patch.PatchType.MergePatch), item.Name(), item.Namespace());
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
            Title = Assets.Resources.ResourceListView_UnCordonNode_Title,
            Content = string.Format(Assets.Resources.ResourceListView_UnCordonNode_Content, items.Count),
            PrimaryButtonText = Assets.Resources.ResourceListView_UnCordonNode_Primary,
            SecondaryButtonText = Assets.Resources.ResourceListView_UnCordonNode_Secondary,
            DefaultButton = FAContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        const string patch = /*lang=json*/ """
        {
            "spec": {
                "unschedulable": false
            }
        }
        """;

        if (result == FAContentDialogResult.Primary)
        {
            foreach (var item in items.Cast<V1Node>().ToList())
            {
                try
                {
                    await Cluster.Client.CoreV1.PatchNodeAsync(new V1Patch(patch, V1Patch.PatchType.MergePatch), item.Name(), item.Namespace());
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
            Title = Assets.Resources.ResourceListView_DrainNode_Title,
            Content = string.Format(Assets.Resources.ResourceListView_DrainNode_Content, items.Count),
            PrimaryButtonText = Assets.Resources.ResourceListView_DrainNode_Primary,
            SecondaryButtonText = Assets.Resources.ResourceListView_DrainNode_Secondary,
            DefaultButton = FAContentDialogButton.Secondary
        };

        var result = await _dialogService.ShowContentDialogAsync(this, settings);

        const string patch = /*lang=json*/ """
        {
            "spec": {
                "unschedulable": true
            }
        }
        """;

        if (result == FAContentDialogResult.Primary)
        {
            foreach (var item in items.Cast<V1Node>().ToList())
            {
                try
                {
                    await Cluster.Client.CoreV1.PatchNodeAsync(new V1Patch(patch, V1Patch.PatchType.MergePatch), item.Name(), item.Namespace());

                    await Cluster.SeedResource<V1Pod>(true);
                    var pods = Cluster.GetResourceList<V1Pod>();

                    foreach (var pod in pods)
                    {
                        if (pod.Spec.NodeName == item.Metadata.Name)
                        {
                            if (pod.Metadata.OwnerReferences.Any(x => x.ApiVersion == V1DaemonSet.KubeGroup + "/" + V1DaemonSet.KubeApiVersion &&
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
                                    Name = pod.Metadata.Name,
                                    NamespaceProperty = pod.Metadata.NamespaceProperty
                                }
                            };

                            try
                            {
                                await Cluster.Client.CoreV1.CreateNamespacedPodEvictionAsync(evict, pod.Metadata.Name, pod.Metadata.NamespaceProperty);
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

    public override Control[] Properties(V1Node resource) => [new PropertiesView()];
}
