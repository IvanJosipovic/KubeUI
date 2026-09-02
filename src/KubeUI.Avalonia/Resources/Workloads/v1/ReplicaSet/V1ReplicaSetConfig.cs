using FluentIcons.Common;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.ReplicaSet;

public sealed partial class V1ReplicaSetConfig : ResourceConfigBase<V1ReplicaSet>
{
    public V1ReplicaSetConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => Assets.Resources.ResourceConfig_Category_Workloads!;

    public override int Order => 4;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1ReplicaSet, int>()
            {
                Key = "desired",
                Name = Assets.Resources.V1ReplicaSetConfig_Desired!,
                Field = x => x.Spec.Replicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1ReplicaSet, int>()
            {
                Key = "current",
                Name = Assets.Resources.V1ReplicaSetConfig_Current!,
                Field = x => x.Status.AvailableReplicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1ReplicaSet, int>()
            {
                Key = "ready",
                Name = Assets.Resources.V1ReplicaSetConfig_Ready!,
                Field = x => x.Status.ReadyReplicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1ReplicaSet>? selectedItems)
    {
        return [
            CreatePodLogsMenuItem(selectedItems),
            new()
            {
                Title = Assets.Resources.V1ReplicaSetConfig_MenuItem_Restart,
                FluentIcon = Icon.ArrowSync,
                Command = RestartCommand,
                CommandParameter = selectedItems?.ToList(),
            },
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
