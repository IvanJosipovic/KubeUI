using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class GitOpsRelationshipProvider : IResourceRelationshipProvider
{
    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(new GroupApiVersionKind("argoproj.io", "v1alpha1", "Application", "applications")),
        new(new GroupApiVersionKind("kustomize.toolkit.fluxcd.io", "v1", "Kustomization", "kustomizations")),
        new(new GroupApiVersionKind("helm.toolkit.fluxcd.io", "v2", "HelmRelease", "helmreleases")),
    ];

    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        AddArgo(context, relationships, resource);

        AddFlux(context, relationships, resource, "kustomize.toolkit.fluxcd.io/name", "kustomize.toolkit.fluxcd.io/namespace", "kustomize.toolkit.fluxcd.io", "Kustomization");
        AddFlux(context, relationships, resource, "helm.toolkit.fluxcd.io/name", "helm.toolkit.fluxcd.io/namespace", "helm.toolkit.fluxcd.io", "HelmRelease");
    }

    private static void AddArgo(ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships, IKubernetesObject<V1ObjectMeta> resource)
    {
        string? trackingId = TryGet(resource.Metadata?.Annotations, "argocd.argoproj.io/tracking-id");
        string? name = trackingId?.Split(':', 2).FirstOrDefault()
            ?? TryGet(resource.Metadata?.Annotations, "argocd.argoproj.io/instance")
            ?? TryGet(resource.Metadata?.Labels, "argocd.argoproj.io/instance");
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (context.TryGetByGroupAndKind("argoproj.io", "Application", out IReadOnlyList<IKubernetesObject<V1ObjectMeta>> applications))
        {
            foreach (IKubernetesObject<V1ObjectMeta> application in applications)
            {
                if (application.ApiVersion != "argoproj.io/v1alpha1"
                    || application.Name() != name
                    || ReferenceEquals(application, resource))
                {
                    continue;
                }

                context.Add(relationships, application, resource, ResourceRelationshipKind.GitOps);
                return;
            }
        }

        context.RecordUnresolved("argoproj.io", "Application", null, name, "v1alpha1");
    }

    private static void AddFlux(ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships, IKubernetesObject<V1ObjectMeta> resource, string nameKey, string namespaceKey, string apiGroup, string kind)
    {
        string? name = TryGet(resource.Metadata?.Labels, nameKey);
        string? namespaceName = TryGet(resource.Metadata?.Labels, namespaceKey);
        if (name == null)
        {
            return;
        }

        if (!context.TryGetByGroupAndKind(apiGroup, kind, out IReadOnlyList<IKubernetesObject<V1ObjectMeta>> controllers))
        {
            context.RecordUnresolved(apiGroup, kind, namespaceName, name);
            return;
        }

        foreach (IKubernetesObject<V1ObjectMeta> controller in controllers)
        {
            if (controller.Namespace() != namespaceName
                || controller.Name() != name
                || ReferenceEquals(controller, resource))
            {
                continue;
            }

            context.Add(relationships, controller, resource, ResourceRelationshipKind.GitOps);
            return;
        }

        context.RecordUnresolved(apiGroup, kind, namespaceName, name);
    }

    private static string? TryGet(IDictionary<string, string>? values, string key)
        => values != null && values.TryGetValue(key, out string? value) ? value : null;
}
