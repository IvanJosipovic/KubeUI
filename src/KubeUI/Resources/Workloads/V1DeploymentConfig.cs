using Dock.Model.Core;
using k8s.Models;
using KubeUI.Client;
using Scrutor;

namespace KubeUI.Resources.Workloads.Pod;

[ServiceDescriptor<ResourceConfigBase<V1Deployment>>(ServiceLifetime.Transient)]
public sealed partial class V1DeploymentConfig : ResourceConfigBase<V1Deployment>, IInitializeCluster
{
    private ICluster _cluster;
    private IFactory _factory;

    public override string Category => "Workloads";

    public override int Order => 1;

    public V1DeploymentConfig(IFactory factory)
    {
        _factory = factory;
    }

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
                NameColumn(SortDirection.Ascending),
                NamespaceColumn(),
                new ResourceListViewDefinitionColumn<V1Deployment, int>()
                {
                    Name = "Pods",
                    Display = x => $"{x.Status?.AvailableReplicas.GetValueOrDefault()}/{x.Spec?.Replicas}",
                    Field = x => x.Status.AvailableReplicas.GetValueOrDefault(),
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Deployment, int>()
                {
                    Name = "Replicas",
                    Display = x => x.Spec.Replicas.GetValueOrDefault().ToString(),
                    Field = x => x.Spec.Replicas.GetValueOrDefault(),
                    Width = nameof(DataGridLengthUnitType.SizeToHeader)
                },
                new ResourceListViewDefinitionColumn<V1Deployment, string>()
                {
                    Name = "Available",
                    Field = x => x.Status.Conditions == null ? "" : x.Status.Conditions.FirstOrDefault(x => x.Type == "Available")?.Status ?? "",
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
                    CommandPath = nameof(ResourceListViewModel<V1Deployment>.RestartDeploymentCommand),
                    CommandParameterPath = "SelectedItem.Value"
                },
        ];
    }

    public void Initialize(ICluster cluster)
    {
        _cluster = cluster;
    }

    public override Control[]? Properties(V1Deployment resource)
    {
        return null;
    }
}
