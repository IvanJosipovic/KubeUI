using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Clusters.Workspace;

/// <summary>Exposes the cluster-facing behavior of a configured Kubernetes resource kind.</summary>
public interface IClusterResourceConfig : IInitializeCluster
{
    bool IsNamespaced { get; }
    bool CanListAndWatch { get; }
    bool PermissionsLoaded { get; }
    bool SeedOnConnect => false;
    bool IsCustomResource => false;
    GroupApiVersionKind Kind { get; }
    V1CustomResourceDefinition? CustomResourceDefinition => null;
    string Name { get; }
    int Order { get; }
    IEnumerable<(Verb verb, string? subresource)> Permissions();
    IEnumerable<AuthorizationRequest> ListWatchAuthorizationRequests() =>
    [
        new AuthorizationRequest(Kind, Verb.List, null),
        new AuthorizationRequest(Kind, Verb.Watch, null),
    ];
    IEnumerable<AuthorizationRequest> AuthorizationRequests() =>
        Permissions().Select(permission => new AuthorizationRequest(Kind, permission.verb, permission.subresource));
    Task EvaluateListWatchAccessAsync();
    Task SeedResource(bool waitForReady = false);
    Task SeedResource(bool waitForReady, CancellationToken cancellationToken) => SeedResource(waitForReady);
}

/// <summary>Configures a resource kind discovered from a custom resource definition.</summary>
public interface IClusterCustomResourceConfig : IClusterResourceConfig
{
    void Configure(V1CustomResourceDefinition definition);
}

/// <summary>Creates the resource configuration selected for a discovered custom resource definition.</summary>
public interface IClusterCustomResourceConfigFactory
{
    IClusterCustomResourceConfig Create(V1CustomResourceDefinition definition);
}
