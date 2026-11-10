using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Resources.List;

/// <summary>Creates resource-list view models for configured resource kinds.</summary>
public interface IResourceListDocumentFactory
{
    /// <summary>Creates and initializes a resource-list view model for a configured resource kind.</summary>
    /// <param name="cluster">Workspace that owns the resource configuration.</param>
    /// <param name="resourceConfig">Configuration for the resource kind to display.</param>
    /// <returns>The initialized view model, or <see langword="null"/> when the resource model is unregistered or its configuration does not match the model.</returns>
    IResourceListViewModel? Create(ClusterWorkspace cluster, IResourceConfig resourceConfig);
}

/// <summary>Creates an initialized typed resource-list view model from a resource configuration.</summary>
public interface IResourceListViewModelCreator
{
    /// <summary>Gets the CLR type represented by the resource configuration.</summary>
    Type ResourceType { get; }

    /// <summary>Creates and initializes the typed resource-list view model.</summary>
    /// <param name="cluster">Workspace that owns the resource configuration.</param>
    /// <param name="kind">Resource kind to display.</param>
    /// <returns>The initialized resource-list view model.</returns>
    IResourceListViewModel Create(ClusterWorkspace cluster, GroupApiVersionKind kind);
}

internal sealed class ResourceListDocumentFactory : IResourceListDocumentFactory
{
    public IResourceListViewModel? Create(ClusterWorkspace cluster, IResourceConfig resourceConfig)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(resourceConfig);

        var kind = resourceConfig.Kind;
        if (!cluster.Runtime.ModelCatalog.TryGetResourceType(kind, out var resourceType))
        {
            return null;
        }

        if (resourceType == typeof(GenericKubernetesObject))
        {
            var viewModel = cluster.ServiceProvider.GetRequiredService<ResourceListViewModel<GenericKubernetesObject>>();
            viewModel.InitializeResource(cluster, kind);
            return viewModel;
        }

        if (resourceConfig is not IResourceListViewModelCreator creator
            || creator.ResourceType != resourceType)
        {
            return null;
        }

        return creator.Create(cluster, kind);
    }
}
