using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Clusters.Workspace;

/// <summary>Exposes the cluster-facing behavior of a configured Kubernetes resource kind.</summary>
public interface IClusterResourceConfig : IInitializeCluster
{
    /// <summary>Gets whether the resource kind is namespaced rather than cluster-scoped.</summary>
    bool IsNamespaced { get; }

    /// <summary>Gets whether the resource kind supports list and watch informers.</summary>
    bool CanListAndWatch { get; }

    /// <summary>Gets whether the permission evaluation for this kind has completed.</summary>
    bool PermissionsLoaded { get; }

    /// <summary>Gets whether the resource is seeded when the cluster connects.</summary>
    bool SeedOnConnect => false;

    /// <summary>Gets whether this configuration represents a dynamically discovered resource.</summary>
    bool IsCustomResource => false;

    /// <summary>Gets the Kubernetes group/version/kind served by this configuration.</summary>
    GroupApiVersionKind Kind { get; }

    /// <summary>Gets the backing custom resource definition when available; discovery-backed configs may not have CRD read access.</summary>
    V1CustomResourceDefinition? CustomResourceDefinition => null;

    /// <summary>Gets the display name of the resource kind.</summary>
    string Name { get; }

    /// <summary>Gets the ordering weight used when processing resource configurations.</summary>
    int Order { get; }

    /// <summary>Lists the verbs and subresources this configuration requires permissions for.</summary>
    IEnumerable<(Verb verb, string? subresource)> Permissions();

    /// <summary>Lists the authorization requests needed to grant list and watch access.</summary>
    IEnumerable<AuthorizationRequest> ListWatchAuthorizationRequests() =>
    [
        new AuthorizationRequest(Kind, Verb.List, null),
        new AuthorizationRequest(Kind, Verb.Watch, null),
    ];

    /// <summary>Builds the authorization requests derived from <see cref="Permissions"/>.</summary>
    IEnumerable<AuthorizationRequest> AuthorizationRequests() =>
        Permissions().Select(permission => new AuthorizationRequest(Kind, permission.verb, permission.subresource));

    /// <summary>Evaluates list and watch access for the resource kind and reports the outcome.</summary>
    Task EvaluateListWatchAccessAsync();

    /// <summary>Seeds the resource kind so its data is available without waiting.</summary>
    /// <param name="waitForReady">Whether to wait until the seeded data is ready.</param>
    Task SeedResource(bool waitForReady = false);

    /// <summary>Seeds the resource kind, honoring a cancellation token.</summary>
    /// <param name="waitForReady">Whether to wait until the seeded data is ready.</param>
    /// <param name="cancellationToken">Cancels the seed operation.</param>
    Task SeedResource(bool waitForReady, CancellationToken cancellationToken);
}

/// <summary>Consumes optional printer-column metadata from a readable CRD without owning resource registration.</summary>
public interface IClusterCustomResourcePrinterColumnConsumer
{
    /// <summary>Updates supplemental printer columns for this discovery-backed resource.</summary>
    /// <param name="definition">A readable CRD, or null when no matching CRD is available.</param>
    void ConfigurePrinterColumns(V1CustomResourceDefinition? definition);
}

/// <summary>Configures a dynamic resource kind discovered from the Kubernetes API.</summary>
public interface IClusterCustomResourceConfig : IClusterResourceConfig
{
    /// <summary>Applies a resource endpoint from a complete API discovery snapshot.</summary>
    /// <param name="resource">The discovered resource endpoint.</param>
    void Configure(ApiDiscoveryResource resource);

    /// <summary>Applies a discovered custom resource definition to the configuration.</summary>
    /// <param name="definition">The custom resource definition to apply.</param>
    void Configure(V1CustomResourceDefinition definition);
}

/// <summary>Creates the resource configuration selected for a discovered dynamic resource.</summary>
public interface IClusterCustomResourceConfigFactory
{
    /// <summary>Creates the configuration matching a discovered API endpoint.</summary>
    /// <param name="resource">The discovered API endpoint.</param>
    /// <returns>The configuration serving the endpoint.</returns>
    IClusterCustomResourceConfig Create(ApiDiscoveryResource resource);

}
