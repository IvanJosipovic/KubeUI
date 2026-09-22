using k8s;
using k8s.Models;
using KubeUI.Kubernetes.Resources.Relationships;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

/// <summary>Applies display-only resource filters to an already scoped graph.</summary>
internal static class ResourceGraphDisplayFilter
{
    public static ResourceRelationshipGraph Apply(
        ResourceRelationshipGraph graph,
        IReadOnlySet<string> selectedTypes,
        bool showNotReadyOnly)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(selectedTypes);

        var resources = graph.Resources
            .Where(resource => resource.Kind is string kind && selectedTypes.Contains(kind))
            .Where(resource => !showNotReadyOnly || ResourceReadiness.IsNotReady(resource))
            .ToArray();
        var identities = resources.Select(Identity).ToHashSet();
        return new ResourceRelationshipGraph(
            resources,
            graph.Relationships.Where(relationship => identities.Contains(relationship.Source) && identities.Contains(relationship.Target)).ToArray(),
            graph.UnresolvedReferences,
            graph.RequiredSeedPrerequisites);
    }

    private static ResourceIdentity Identity(IKubernetesObject<V1ObjectMeta> resource)
        => new(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty, resource.Uid());
}
