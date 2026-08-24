using FluentIcons.Common;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Deployment;

public sealed partial class V1DeploymentConfig : ResourceConfigBase<V1Deployment>
{
    public V1DeploymentConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => Assets.Resources.ResourceConfig_Category_Workloads!;

    public override int Order => 1;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1Deployment, int>()
            {
                Key = "pods",
                Name = Assets.Resources.V1DeploymentConfig_Pods!,
                Display = x => $"{x.Status?.AvailableReplicas ?? 0}/{x.Spec?.Replicas ?? 0}",
                Field = x => x.Status?.AvailableReplicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1Deployment, int>()
            {
                Key = "replicas",
                Name = Assets.Resources.V1DeploymentConfig_Replicas!,
                Field = x => x.Spec.Replicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1Deployment, string>()
            {
                Key = "available",
                Name = Assets.Resources.V1DeploymentConfig_Available!,
                Field = x => x.Status?.Conditions?.FirstOrDefault(x => x.Type == "Available")?.Status ?? "",
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1Deployment>? selectedItems)
    {
        return [
            CreatePodLogsMenuItem(selectedItems),
            new()
            {
                Title = Assets.Resources.V1DeploymentConfig_MenuItem_Restart,
                FluentIcon = Icon.ArrowSync,
                Command = RestartCommand,
                CommandParameter = selectedItems?.ToList()
            },
        ];
    }

    public override IEnumerable<AuthorizationRequest> AuthorizationRequests()
    {
        return base.AuthorizationRequests().Append(
            new AuthorizationRequest(GroupApiVersionKind.From<V1Pod>(), Verb.Get, "log"));
    }

    public override Control[] Properties(V1Deployment resource) => [new PropertiesView()];
}
