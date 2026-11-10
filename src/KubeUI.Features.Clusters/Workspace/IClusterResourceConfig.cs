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

    /// <summary>Gets whether this configuration was created from a custom resource definition.</summary>
    bool IsCustomResource => false;

    /// <summary>Gets the Kubernetes group/version/kind served by this configuration.</summary>
    GroupApiVersionKind Kind { get; }

    /// <summary>Gets the custom resource definition backing this configuration, when applicable.</summary>
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

/// <summary>Configures a resource kind discovered from a custom resource definition.</summary>
public interface IClusterCustomResourceConfig : IClusterResourceConfig
{
    /// <summary>Applies a discovered custom resource definition to the configuration.</summary>
    /// <param name="definition">The custom resource definition to apply.</param>
    void Configure(V1CustomResourceDefinition definition);
}

/// <summary>Creates the resource configuration selected for a discovered custom resource definition.</summary>
public interface IClusterCustomResourceConfigFactory
{
    /// <summary>Creates the configuration matching a discovered custom resource definition.</summary>
    /// <param name="definition">The discovered custom resource definition.</param>
    /// <returns>The configuration serving the definition.</returns>
    IClusterCustomResourceConfig Create(V1CustomResourceDefinition definition);
}
