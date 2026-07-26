using k8s;
using k8s.Models;
using KubeUI.Kubernetes.Resources.Relationships;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Resources.Relationships;

public sealed class ResourceRelationshipBuilderTests
{
    [Fact]
    public void Builds_owner_relationships_with_explicit_direction()
    {
        V1Deployment deployment = new()
        {
            ApiVersion = "apps/v1",
            Kind = V1Deployment.KubeKind,
            Metadata = new() { Name = "web", NamespaceProperty = "demo", Uid = "deployment-uid" },
        };
        V1Pod pod = new()
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new()
            {
                Name = "web-1",
                NamespaceProperty = "demo",
                OwnerReferences = [new() { ApiVersion = "apps/v1", Kind = V1Deployment.KubeKind, Name = "web", Uid = "deployment-uid" }],
            },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build([deployment, pod], new HashSet<string> { "demo" }, hideNoise: true);

        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("apps/v1", V1Deployment.KubeKind, "demo", "web", "deployment-uid"),
            new("v1", V1Pod.KubeKind, "demo", "web-1", null),
            ResourceRelationshipKind.Owner));
    }

    [Fact]
    public void Uses_indexes_and_suppresses_duplicate_configuration_relationships()
    {
        V1ConfigMap configMap = new()
        {
            ApiVersion = "v1",
            Kind = V1ConfigMap.KubeKind,
            Metadata = new() { Name = "settings", NamespaceProperty = "demo", Uid = "config-uid" },
        };
        V1Pod pod = new()
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new() { Name = "web", NamespaceProperty = "demo" },
            Spec = new()
            {
                Containers = [new() { Name = "web", EnvFrom = [new() { ConfigMapRef = new() { Name = "settings" } }] }],
                Volumes = [new() { ConfigMap = new() { Name = "settings" } }],
            },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build([configMap, pod], new HashSet<string> { "demo" }, hideNoise: true);

        graph.Relationships.Count(x => x.Source.Name == "web" && x.Target.Name == "settings").ShouldBe(1);
    }

    [Fact]
    public void Preserves_distinct_labels_for_the_same_endpoints()
    {
        ResourceIdentity source = new("v1", V1Pod.KubeKind, "demo", "web", "web-uid");
        ResourceIdentity target = new("v1", V1Service.KubeKind, "demo", "web", "service-uid");

        IReadOnlyList<ResourceRelationship> relationships = ResourceRelationshipBuilder.SimplifyRelationships(
        [
            new(source, target, ResourceRelationshipKind.Label, "CONFIG_KEY"),
            new(source, target, ResourceRelationshipKind.Label, "OTHER_CONFIG_KEY"),
            new(source, target, ResourceRelationshipKind.Label, "CONFIG_KEY"),
        ]);

        relationships.Count.ShouldBe(2);
        relationships.ShouldContain(new ResourceRelationship(source, target, ResourceRelationshipKind.Label, "CONFIG_KEY"));
        relationships.ShouldContain(new ResourceRelationship(source, target, ResourceRelationshipKind.Label, "OTHER_CONFIG_KEY"));
    }

    [Fact]
    public void Removes_less_specific_relationship_when_owner_descendant_has_same_target()
    {
        ResourceIdentity owner = new("apps/v1", V1Deployment.KubeKind, "demo", "web", "deployment-uid");
        ResourceIdentity child = new("v1", V1Pod.KubeKind, "demo", "web", "pod-uid");
        ResourceIdentity target = new("v1", V1ConfigMap.KubeKind, "demo", "settings", "settings-uid");

        IReadOnlyList<ResourceRelationship> relationships = ResourceRelationshipBuilder.SimplifyRelationships(
        [
            new(owner, child, ResourceRelationshipKind.Owner),
            new(owner, target, ResourceRelationshipKind.Label, "OWNER_LABEL"),
            new(child, target, ResourceRelationshipKind.Label, "CHILD_LABEL"),
        ]);

        relationships.Any(x => x == new ResourceRelationship(owner, child, ResourceRelationshipKind.Owner)).ShouldBeTrue();
        relationships.Any(x => x == new ResourceRelationship(child, target, ResourceRelationshipKind.Label, "CHILD_LABEL")).ShouldBeTrue();
        relationships.Any(x => x == new ResourceRelationship(owner, target, ResourceRelationshipKind.Label, "OWNER_LABEL")).ShouldBeFalse();
    }

    [Fact]
    public void Applies_namespace_and_noise_filters()
    {
        V1Pod selected = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "selected", NamespaceProperty = "demo" } };
        V1Pod other = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "other", NamespaceProperty = "other" } };
        Corev1Event noise = new() { ApiVersion = "v1", Kind = Corev1Event.KubeKind, Metadata = new() { Name = "event", NamespaceProperty = "demo" } };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build([selected, other, noise], new HashSet<string> { "demo" }, hideNoise: true);

        graph.Resources.Select(x => x.Name()).ShouldBe(["selected"]);
    }

    [Fact]
    public void Keeps_only_cluster_scoped_resources_connected_to_selected_namespace()
    {
        V1Pod selected = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "selected", NamespaceProperty = "demo", Uid = "selected-uid" } };
        V1Pod other = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "other", NamespaceProperty = "other", Uid = "other-uid" } };
        V1Node unrelated = new() { ApiVersion = "v1", Kind = V1Node.KubeKind, Metadata = new() { Name = "unrelated" } };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build([selected, other, unrelated], new HashSet<string> { "demo" }, hideNoise: true);

        graph.Resources.Select(x => x.Name()).ShouldBe(["selected"]);
    }

    [Fact]
    public void Keeps_related_resources_from_other_namespaces()
    {
        V1Pod selected = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "selected", NamespaceProperty = "demo", Uid = "selected-uid" } };
        V1Pod related = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "related", NamespaceProperty = "other", Uid = "related-uid" } };

        ResourceRelationshipBuilder builder = new([new CrossNamespaceProvider()]);
        ResourceRelationshipGraph graph = builder.Build([selected, related], new HashSet<string> { "demo" }, hideNoise: true);

        graph.Resources.Select(resource => resource.Name()).ShouldBe(["selected", "related"]);
        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("v1", V1Pod.KubeKind, "demo", "selected", "selected-uid"),
            new("v1", V1Pod.KubeKind, "other", "related", "related-uid"),
            ResourceRelationshipKind.Reference));
    }

    [Fact]
    public void Resolves_argo_application_across_namespaces()
    {
        V1Pod managed = new()
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new()
            {
                Name = "managed",
                NamespaceProperty = "workload",
                Annotations = new Dictionary<string, string> { ["argocd.argoproj.io/tracking-id"] = "demo-app:apps/Deployment:workload/demo" },
            },
        };
        V1ConfigMap application = new()
        {
            ApiVersion = "argoproj.io/v1alpha1",
            Kind = "Application",
            Metadata = new() { Name = "demo-app", NamespaceProperty = "argocd" },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build([managed, application], new HashSet<string> { "workload" }, hideNoise: true);

        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("argoproj.io/v1alpha1", "Application", "argocd", "demo-app", null),
            new("v1", V1Pod.KubeKind, "workload", "managed", null),
            ResourceRelationshipKind.GitOps));
    }

    [Fact]
    public void Points_rbac_role_to_role_binding()
    {
        V1Role role = new()
        {
            ApiVersion = "rbac.authorization.k8s.io/v1",
            Kind = V1Role.KubeKind,
            Metadata = new() { Name = "reader", NamespaceProperty = "demo", Uid = "role-uid" },
        };
        V1RoleBinding binding = new()
        {
            ApiVersion = "rbac.authorization.k8s.io/v1",
            Kind = V1RoleBinding.KubeKind,
            Metadata = new() { Name = "reader-binding", NamespaceProperty = "demo", Uid = "binding-uid" },
            RoleRef = new() { ApiGroup = "rbac.authorization.k8s.io", Kind = V1Role.KubeKind, Name = "reader" },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build([role, binding], new HashSet<string> { "demo" }, hideNoise: true);

        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("rbac.authorization.k8s.io/v1", V1Role.KubeKind, "demo", "reader", "role-uid"),
            new("rbac.authorization.k8s.io/v1", V1RoleBinding.KubeKind, "demo", "reader-binding", "binding-uid"),
            ResourceRelationshipKind.Rbac));
    }

    [Fact]
    public void Resolves_flux_controllers_by_label_name_and_namespace()
    {
        V1ConfigMap helmManaged = new()
        {
            ApiVersion = "v1",
            Kind = V1ConfigMap.KubeKind,
            Metadata = new()
            {
                Name = "helm-managed",
                NamespaceProperty = "demo",
                Labels = new Dictionary<string, string>
                {
                    ["helm.toolkit.fluxcd.io/name"] = "release-a",
                    ["helm.toolkit.fluxcd.io/namespace"] = "demo",
                },
            },
        };
        V1ConfigMap kustomizeManaged = new()
        {
            ApiVersion = "v1",
            Kind = V1ConfigMap.KubeKind,
            Metadata = new()
            {
                Name = "kustomize-managed",
                NamespaceProperty = "demo",
                Labels = new Dictionary<string, string>
                {
                    ["kustomize.toolkit.fluxcd.io/name"] = "kustomization-a",
                    ["kustomize.toolkit.fluxcd.io/namespace"] = "demo",
                },
            },
        };
        V1ConfigMap helmRelease = new()
        {
            ApiVersion = "helm.toolkit.fluxcd.io/v2",
            Kind = "HelmRelease",
            Metadata = new() { Name = "release-a", NamespaceProperty = "demo", Uid = "helm-uid" },
        };
        V1ConfigMap kustomization = new()
        {
            ApiVersion = "kustomize.toolkit.fluxcd.io/v1",
            Kind = "Kustomization",
            Metadata = new() { Name = "kustomization-a", NamespaceProperty = "demo", Uid = "kustomize-uid" },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [helmManaged, kustomizeManaged, helmRelease, kustomization],
            new HashSet<string> { "demo" },
            hideNoise: true);

        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("helm.toolkit.fluxcd.io/v2", "HelmRelease", "demo", "release-a", "helm-uid"),
            new("v1", V1ConfigMap.KubeKind, "demo", "helm-managed", null),
            ResourceRelationshipKind.GitOps));
        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("kustomize.toolkit.fluxcd.io/v1", "Kustomization", "demo", "kustomization-a", "kustomize-uid"),
            new("v1", V1ConfigMap.KubeKind, "demo", "kustomize-managed", null),
            ResourceRelationshipKind.GitOps));
    }

    private sealed class CrossNamespaceProvider : IResourceRelationshipProvider
    {
        public void AddRelationships(
            IKubernetesObject<V1ObjectMeta> resource,
            ResourceRelationshipContext context,
            ICollection<ResourceRelationship> relationships)
        {
            if (resource.Name() != "selected"
                || !context.TryGet("v1", V1Pod.KubeKind, "other", "related", out IKubernetesObject<V1ObjectMeta>? related)
                || related == null)
            {
                return;
            }

            context.Add(relationships, resource, related, ResourceRelationshipKind.Reference);
        }
    }
}
