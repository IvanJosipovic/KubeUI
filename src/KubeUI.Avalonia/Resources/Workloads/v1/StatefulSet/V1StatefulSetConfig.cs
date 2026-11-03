using k8s;
using FluentIcons.Common;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Resources.Workloads;

namespace KubeUI.Avalonia.Resources.Workloads.v1.StatefulSet;

public sealed partial class V1StatefulSetConfig : ScalableWorkloadConfig<V1StatefulSet>
{
    public V1StatefulSetConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.WorkloadsId);

    public override int Order => 3;

    protected override int GetDesiredReplicas(V1StatefulSet workload) => workload.Spec?.Replicas ?? 0;

    protected override Task PatchReplicasAsync(V1StatefulSet workload, int replicas)
        => Cluster.Runtime.Client!.AppsV1.PatchNamespacedStatefulSetScaleAsync(
            CreateScalePatch(replicas), workload.Name(), workload.Namespace());

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1StatefulSet, int>()
            {
                Key = "replicas",
                Name = Assets.Resources.V1StatefulSetConfig_Replicas!,
                Field = x => x.Status.Replicas,
                WidthMode = DynamicTableViewWidthMode.Header
            },
            AgeColumn(),
        ];
    }

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1StatefulSet>? selectedItems)
    {
        return [
            CreatePodLogsMenuItem(selectedItems),
            CreateScaleMenuItem(selectedItems),
            new()
            {
                Title = Assets.Resources.V1StatefulSetConfig_MenuItem_Restart,
                FluentIcon = Icon.ArrowSync,
                Command = RestartCommand,
                CommandParameter = selectedItems?.ToList()
            },
        ];
    }

    /// <summary>Requests permission to read pod logs for stateful set workloads.</summary>
    public override IEnumerable<AuthorizationRequest> AuthorizationRequests()
    {
        return base.AuthorizationRequests().Append(
            new AuthorizationRequest(GroupApiVersionKind.From<V1Pod>(), Verb.Get, "log"));
    }

    public override Control[] Properties(V1StatefulSet resource) => [new PropertiesView()];
}
