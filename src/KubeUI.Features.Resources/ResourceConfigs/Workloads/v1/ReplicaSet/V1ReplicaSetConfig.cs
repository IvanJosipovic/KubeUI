using FluentIcons.Common;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Features.Resources.Common;
using KubeUI.Features.Resources.Workloads;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Resources.Workloads.v1.ReplicaSet;

public sealed partial class V1ReplicaSetConfig : ScalableWorkloadConfig<V1ReplicaSet>
{
    public V1ReplicaSetConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(NavigationSections.WorkloadsId);

    public override int Order => 4;

    protected override int GetDesiredReplicas(V1ReplicaSet workload) => workload.Spec?.Replicas ?? 0;

    protected override Task PatchReplicasAsync(V1ReplicaSet workload, int replicas)
        => Cluster.Runtime.Client!.AppsV1.PatchNamespacedReplicaSetScaleAsync(
            CreateScalePatch(replicas), workload.Name(), workload.Namespace());

    protected override IList<IResourceListColumn> CreateDefaultColumns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1ReplicaSet, int>()
            {
                Key = "desired",
                Name = Assets.Resources.V1ReplicaSetConfig_Desired!,
                Field = x => x.Spec.Replicas ?? 0,
                WidthMode = DynamicTableViewWidthMode.Header
            },
            new ResourceListColumn<V1ReplicaSet, int>()
            {
                Key = "current",
                Name = Assets.Resources.V1ReplicaSetConfig_Current!,
                Field = x => x.Status.AvailableReplicas ?? 0,
                WidthMode = DynamicTableViewWidthMode.Header
            },
            new ResourceListColumn<V1ReplicaSet, int>()
            {
                Key = "ready",
                Name = Assets.Resources.V1ReplicaSetConfig_Ready!,
                Field = x => x.Status.ReadyReplicas ?? 0,
                WidthMode = DynamicTableViewWidthMode.Header
            },
            AgeColumn(),
        ];
    }

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1ReplicaSet>? selectedItems)
    {
        return [
            CreatePodLogsMenuItem(selectedItems),
            CreateScaleMenuItem(selectedItems),
        ];
    }

    /// <summary>Requests permission to read pod logs for replica set workloads.</summary>
    public override IEnumerable<AuthorizationRequest> AuthorizationRequests()
    {
        return base.AuthorizationRequests().Append(
            new AuthorizationRequest(GroupApiVersionKind.From<V1Pod>(), Verb.Get, "log"));
    }

    public override Control[] Properties(V1ReplicaSet resource) => [new PropertiesView()];
}
