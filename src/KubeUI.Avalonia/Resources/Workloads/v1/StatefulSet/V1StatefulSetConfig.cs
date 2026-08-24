using FluentIcons.Common;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.StatefulSet;

public sealed partial class V1StatefulSetConfig : ResourceConfigBase<V1StatefulSet>
{
    public V1StatefulSetConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => Assets.Resources.ResourceConfig_Category_Workloads!;

    public override int Order => 3;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1StatefulSet, int>()
            {
                Key = "replicas",
                Name = Assets.Resources.V1StatefulSetConfig_Replicas!,
                Field = x => x.Status.Replicas,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    protected override IEnumerable<MenuItemViewModel> CreateCustomMenuItems(IEnumerable<V1StatefulSet>? selectedItems)
    {
        return [
            CreatePodLogsMenuItem(selectedItems),
            new()
            {
                Title = Assets.Resources.V1StatefulSetConfig_MenuItem_Restart,
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

    public override Control[] Properties(V1StatefulSet resource) => [new PropertiesView()];
}
