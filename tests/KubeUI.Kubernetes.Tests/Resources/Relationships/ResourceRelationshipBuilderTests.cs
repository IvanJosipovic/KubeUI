using k8s;
using k8s.Models;
using KubeUI.Kubernetes.Resources.Relationships;
using KubeUI.Kubernetes.Resources.Relationships.Providers;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Resources.Relationships;

public sealed class ResourceRelationshipBuilderTests
{
    [Fact]
    public void Relates_crossplane_usage_references_without_a_static_usage_model()
    {
        TestDynamicResource schema = new()
        {
            ApiVersion = "unity.databricks.m.crossplane.io/v1beta1",
            Kind = "Schema",
            Metadata = new() { NamespaceProperty = "platform-test-data-product", Name = "schema", Uid = "schema-uid" },
        };
        TestDynamicResource catalog = new()
        {
            ApiVersion = "unity.databricks.m.crossplane.io/v1beta1",
            Kind = "Catalog",
            Metadata = new() { NamespaceProperty = "platform-test-data-product", Name = "catalog", Uid = "catalog-uid" },
        };
        TestDynamicUsage usage = new()
        {
            ApiVersion = "protection.crossplane.io/v1beta1",
            Kind = "Usage",
            Metadata = new() { NamespaceProperty = "platform-test-data-product", Name = "usage", Uid = "usage-uid" },
            Spec = new()
            {
                By = CreateReference("Schema", "schema"),
                Of = CreateReference("Catalog", "catalog"),
            },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [schema, catalog, usage],
            new HashSet<string> { "platform-test-data-product" },
            hideNoise: true);

        graph.Relationships.ShouldContain(new ResourceRelationship(
            new(schema.ApiVersion!, schema.Kind!, schema.Namespace(), schema.Name()!, schema.Uid()),
            new(catalog.ApiVersion!, catalog.Kind!, catalog.Namespace(), catalog.Name()!, catalog.Uid()),
            ResourceRelationshipKind.Reference,
            "uses"));
    }

    [Fact]
    public void Ignores_crossplane_usage_with_an_incomplete_reference()
    {
        TestDynamicUsage usage = new()
        {
            ApiVersion = "protection.crossplane.io/v1beta1",
            Kind = "Usage",
            Metadata = new() { NamespaceProperty = "platform-test-data-product", Name = "usage", Uid = "usage-uid" },
            Spec = new()
            {
                By = CreateReference("Schema", "schema"),
            },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [usage],
            new HashSet<string> { "platform-test-data-product" },
            hideNoise: true);

        graph.Relationships.ShouldBeEmpty();
    }

    private static TestDynamicUsageReference CreateReference(string kind, string name)
        => new()
        {
            ApiVersion = "unity.databricks.m.crossplane.io/v1beta1",
            Kind = kind,
            ResourceRef = new() { Name = name },
        };

    [Fact]
    public void Relates_persistent_volumes_and_claims_to_their_storage_class()
    {
        V1StorageClass storageClass = new()
        {
            ApiVersion = "storage.k8s.io/v1",
            Kind = V1StorageClass.KubeKind,
            Metadata = new() { Name = "fast", Uid = "storage-class-uid" },
        };
        V1PersistentVolume volume = new()
        {
            ApiVersion = "v1",
            Kind = V1PersistentVolume.KubeKind,
            Metadata = new() { Name = "volume", Uid = "volume-uid" },
            Spec = new() { StorageClassName = "fast" },
        };
        V1PersistentVolume unrelatedVolume = new()
        {
            ApiVersion = "v1",
            Kind = V1PersistentVolume.KubeKind,
            Metadata = new() { Name = "unrelated", Uid = "unrelated-volume-uid" },
            Spec = new() { StorageClassName = "fast" },
        };
        V1PersistentVolumeClaim claim = new()
        {
            ApiVersion = "v1",
            Kind = V1PersistentVolumeClaim.KubeKind,
            Metadata = new() { Name = "claim", NamespaceProperty = "demo", Uid = "claim-uid" },
            Spec = new() { VolumeName = "volume", StorageClassName = "fast" },
        };

        ResourceRelationshipContext context = new(
            new Dictionary<string, IKubernetesObject<V1ObjectMeta>> { [storageClass.Uid()!] = storageClass, [volume.Uid()!] = volume, [unrelatedVolume.Uid()!] = unrelatedVolume, [claim.Uid()!] = claim },
            new Dictionary<ResourceKey, IKubernetesObject<V1ObjectMeta>>
            {
                [new(storageClass.ApiVersion!, storageClass.Kind!, storageClass.Namespace(), storageClass.Name()!)] = storageClass,
                [new(volume.ApiVersion!, volume.Kind!, volume.Namespace(), volume.Name()!)] = volume,
                [new(unrelatedVolume.ApiVersion!, unrelatedVolume.Kind!, unrelatedVolume.Namespace(), unrelatedVolume.Name()!)] = unrelatedVolume,
                [new(claim.ApiVersion!, claim.Kind!, claim.Namespace(), claim.Name()!)] = claim,
            },
            new Dictionary<string, IReadOnlyList<IKubernetesObject<V1ObjectMeta>>>
            {
                [storageClass.Kind!] = [storageClass],
                [volume.Kind!] = [volume, unrelatedVolume],
                [claim.Kind!] = [claim],
            });
        HashSet<ResourceRelationship> providerRelationships = [];
        new StorageRelationshipProvider().AddRelationships(volume, context, providerRelationships);
        providerRelationships.ShouldContain(new ResourceRelationship(
            new("v1", V1PersistentVolume.KubeKind, null, "volume", "volume-uid"),
            new("storage.k8s.io/v1", V1StorageClass.KubeKind, null, "fast", "storage-class-uid"),
            ResourceRelationshipKind.Storage));

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build([storageClass, volume, unrelatedVolume, claim], new HashSet<string> { "demo" }, hideNoise: true);

        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("v1", V1PersistentVolume.KubeKind, null, "volume", "volume-uid"),
            new("storage.k8s.io/v1", V1StorageClass.KubeKind, null, "fast", "storage-class-uid"),
            ResourceRelationshipKind.Storage));
        graph.Relationships.Any(relationship =>
            relationship.Source.Name == "claim"
            && relationship.Target.Name == "fast"
            && relationship.Kind == ResourceRelationshipKind.Storage).ShouldBeFalse();
        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("v1", V1PersistentVolumeClaim.KubeKind, "demo", "claim", "claim-uid"),
            new("v1", V1PersistentVolume.KubeKind, null, "volume", "volume-uid"),
            ResourceRelationshipKind.Storage));
        graph.Resources.Select(resource => resource.Name()).ShouldNotContain("unrelated");
    }

    [Fact]
    public void Does_not_use_a_storage_class_as_a_namespace_filter_bridge()
    {
        V1StorageClass storageClass = new()
        {
            ApiVersion = "storage.k8s.io/v1",
            Kind = V1StorageClass.KubeKind,
            Metadata = new() { Name = "fast", Uid = "storage-class-uid" },
        };
        V1PersistentVolumeClaim selectedClaim = new()
        {
            ApiVersion = "v1",
            Kind = V1PersistentVolumeClaim.KubeKind,
            Metadata = new() { Name = "selected", NamespaceProperty = "demo", Uid = "selected-uid" },
            Spec = new() { StorageClassName = "fast" },
        };
        V1PersistentVolumeClaim otherClaim = new()
        {
            ApiVersion = "v1",
            Kind = V1PersistentVolumeClaim.KubeKind,
            Metadata = new() { Name = "other", NamespaceProperty = "other", Uid = "other-uid" },
            Spec = new() { StorageClassName = "fast" },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build([storageClass, selectedClaim, otherClaim], new HashSet<string> { "demo" }, hideNoise: true);

        graph.Resources.Select(resource => resource.Name()).ShouldBe(["fast", "selected"]);
        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("v1", V1PersistentVolumeClaim.KubeKind, "demo", "selected", "selected-uid"),
            new("storage.k8s.io/v1", V1StorageClass.KubeKind, null, "fast", "storage-class-uid"),
            ResourceRelationshipKind.Storage));
    }

    [Fact]
    public void Removes_transitive_relationships_of_any_kind()
    {
        ResourceIdentity first = new("v1", "First", "demo", "first", null);
        ResourceIdentity second = new("v1", "Second", "demo", "second", null);
        ResourceIdentity third = new("v1", "Third", "demo", "third", null);

        IReadOnlyList<ResourceRelationship> relationships = ResourceRelationshipBuilder.SimplifyRelationships(
        [
            new(first, second, ResourceRelationshipKind.Reference),
            new(second, third, ResourceRelationshipKind.Reference),
            new(first, third, ResourceRelationshipKind.Reference),
        ]);

        relationships.ShouldBe(
        [
            new(first, second, ResourceRelationshipKind.Reference),
            new(second, third, ResourceRelationshipKind.Reference),
        ]);

        ResourceIdentity claim = new("v1", V1PersistentVolumeClaim.KubeKind, "demo", "claim", "claim-uid");
        ResourceIdentity volume = new("v1", V1PersistentVolume.KubeKind, null, "volume", "volume-uid");
        ResourceIdentity storageClass = new("storage.k8s.io/v1", V1StorageClass.KubeKind, null, "fast", "storage-class-uid");

        ResourceRelationshipBuilder.SimplifyRelationships(
        [
            new(claim, volume, ResourceRelationshipKind.Storage),
            new(volume, storageClass, ResourceRelationshipKind.Storage),
            new(claim, storageClass, ResourceRelationshipKind.Storage),
        ]).ShouldBe(
        [
            new(claim, volume, ResourceRelationshipKind.Storage),
            new(volume, storageClass, ResourceRelationshipKind.Storage),
        ]);
    }

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

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder([new PodTemplateReferenceRelationshipProvider()]).Build([configMap, pod], new HashSet<string> { "demo" }, hideNoise: true);

        graph.Relationships.ShouldBe(
        [
            new ResourceRelationship(
                new("v1", V1Pod.KubeKind, "demo", "web", null),
                new("v1", V1ConfigMap.KubeKind, "demo", "settings", "config-uid"),
                ResourceRelationshipKind.Reference),
        ]);
    }

    [Fact]
    public void Relates_secret_backed_volumes_from_pods_and_workload_templates()
    {
        V1Secret secret = new()
        {
            ApiVersion = "v1",
            Kind = V1Secret.KubeKind,
            Metadata = new() { Name = "tls-client", NamespaceProperty = "demo", Uid = "secret-uid" },
        };
        V1PodTemplateSpec template = new()
        {
            Spec = new()
            {
                Volumes = [new() { Name = "tls-client-certs", Secret = new() { SecretName = "tls-client" } }],
            },
        };
        IKubernetesObject<V1ObjectMeta>[] consumers =
        [
            new V1Pod { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "pod", NamespaceProperty = "demo" }, Spec = template.Spec },
            new V1Deployment { ApiVersion = "apps/v1", Kind = V1Deployment.KubeKind, Metadata = new() { Name = "deployment", NamespaceProperty = "demo" }, Spec = new() { Template = template } },
            new V1ReplicaSet { ApiVersion = "apps/v1", Kind = V1ReplicaSet.KubeKind, Metadata = new() { Name = "replicaset", NamespaceProperty = "demo" }, Spec = new() { Template = template } },
            new V1StatefulSet { ApiVersion = "apps/v1", Kind = V1StatefulSet.KubeKind, Metadata = new() { Name = "statefulset", NamespaceProperty = "demo" }, Spec = new() { Template = template } },
            new V1DaemonSet { ApiVersion = "apps/v1", Kind = V1DaemonSet.KubeKind, Metadata = new() { Name = "daemonset", NamespaceProperty = "demo" }, Spec = new() { Template = template } },
            new V1Job { ApiVersion = "batch/v1", Kind = V1Job.KubeKind, Metadata = new() { Name = "job", NamespaceProperty = "demo" }, Spec = new() { Template = template } },
            new V1CronJob { ApiVersion = "batch/v1", Kind = V1CronJob.KubeKind, Metadata = new() { Name = "cronjob", NamespaceProperty = "demo" }, Spec = new() { JobTemplate = new() { Spec = new() { Template = template } } } },
        ];

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build([secret, .. consumers], new HashSet<string> { "demo" }, hideNoise: true);

        graph.Relationships
            .Where(relationship => relationship.Target.Name == "tls-client")
            .Select(relationship => (relationship.Source.Name, relationship.Kind, relationship.Label))
            .ShouldBe(
            [
                ("pod", ResourceRelationshipKind.Reference, null),
                ("deployment", ResourceRelationshipKind.Reference, null),
                ("replicaset", ResourceRelationshipKind.Reference, null),
                ("statefulset", ResourceRelationshipKind.Reference, null),
                ("daemonset", ResourceRelationshipKind.Reference, null),
                ("job", ResourceRelationshipKind.Reference, null),
                ("cronjob", ResourceRelationshipKind.Reference, null),
            ]);
    }

    [Fact]
    public void Relates_secret_key_environment_variables_from_pods_and_workload_templates()
    {
        V1Secret secret = new()
        {
            ApiVersion = "v1",
            Kind = V1Secret.KubeKind,
            Metadata = new() { Name = "azure-app-reg-pwd-secret", NamespaceProperty = "demo", Uid = "secret-uid" },
        };
        V1PodTemplateSpec template = new()
        {
            Spec = new()
            {
                Containers =
                [
                    new()
                    {
                        Name = "app",
                        Env = [new() { Name = "settings__cookie__clientSecret", ValueFrom = new() { SecretKeyRef = new() { Name = "azure-app-reg-pwd-secret", Key = "attribute.value" } } }],
                    },
                ],
            },
        };
        IKubernetesObject<V1ObjectMeta>[] consumers =
        [
            new V1Pod { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "pod", NamespaceProperty = "demo" }, Spec = template.Spec },
            new V1Deployment { ApiVersion = "apps/v1", Kind = V1Deployment.KubeKind, Metadata = new() { Name = "deployment", NamespaceProperty = "demo" }, Spec = new() { Template = template } },
            new V1ReplicaSet { ApiVersion = "apps/v1", Kind = V1ReplicaSet.KubeKind, Metadata = new() { Name = "replicaset", NamespaceProperty = "demo" }, Spec = new() { Template = template } },
            new V1StatefulSet { ApiVersion = "apps/v1", Kind = V1StatefulSet.KubeKind, Metadata = new() { Name = "statefulset", NamespaceProperty = "demo" }, Spec = new() { Template = template } },
            new V1DaemonSet { ApiVersion = "apps/v1", Kind = V1DaemonSet.KubeKind, Metadata = new() { Name = "daemonset", NamespaceProperty = "demo" }, Spec = new() { Template = template } },
            new V1Job { ApiVersion = "batch/v1", Kind = V1Job.KubeKind, Metadata = new() { Name = "job", NamespaceProperty = "demo" }, Spec = new() { Template = template } },
            new V1CronJob { ApiVersion = "batch/v1", Kind = V1CronJob.KubeKind, Metadata = new() { Name = "cronjob", NamespaceProperty = "demo" }, Spec = new() { JobTemplate = new() { Spec = new() { Template = template } } } },
        ];

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build([secret, .. consumers], new HashSet<string> { "demo" }, hideNoise: true);

        graph.Relationships
            .Where(relationship => relationship.Target.Name == "azure-app-reg-pwd-secret")
            .Select(relationship => (relationship.Source.Name, relationship.Kind, relationship.Label))
            .ShouldBe(
            [
                ("pod", ResourceRelationshipKind.Reference, null),
                ("deployment", ResourceRelationshipKind.Reference, null),
                ("replicaset", ResourceRelationshipKind.Reference, null),
                ("statefulset", ResourceRelationshipKind.Reference, null),
                ("daemonset", ResourceRelationshipKind.Reference, null),
                ("job", ResourceRelationshipKind.Reference, null),
                ("cronjob", ResourceRelationshipKind.Reference, null),
            ]);
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
    public void Addition_delta_keeps_cluster_scoped_resources_connected_to_selected_namespace()
    {
        V1Pod selected = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "selected", NamespaceProperty = "demo", Uid = "selected-uid" } };
        V1Node related = new() { ApiVersion = "v1", Kind = V1Node.KubeKind, Metadata = new() { Name = "related", Uid = "related-uid" } };
        ResourceRelationshipBuilder builder = new([new SelectedToNodeProvider()]);

        ResourceRelationshipGraph delta = builder.BuildAdditionDelta(
            [selected, related],
            new ResourceKey("v1", V1Node.KubeKind, null, "related"),
            new HashSet<string> { "demo" },
            hideNoise: true);

        delta.Resources.Select(resource => resource.Name()).ShouldBe(["selected", "related"]);
        delta.Relationships.ShouldContain(new ResourceRelationship(
            new("v1", V1Pod.KubeKind, "demo", "selected", "selected-uid"),
            new("v1", V1Node.KubeKind, null, "related", "related-uid"),
            ResourceRelationshipKind.Reference));
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

    private sealed class SelectedToNodeProvider : IResourceRelationshipProvider
    {
        public void AddRelationships(
            IKubernetesObject<V1ObjectMeta> resource,
            ResourceRelationshipContext context,
            ICollection<ResourceRelationship> relationships)
        {
            if (resource.Name() == "selected"
                && context.TryGet("v1", V1Node.KubeKind, null, "related", out IKubernetesObject<V1ObjectMeta>? related)
                && related != null)
            {
                context.Add(relationships, resource, related, ResourceRelationshipKind.Reference);
            }
        }
    }

    private class TestDynamicResource : IKubernetesObject<V1ObjectMeta>
    {
        public string? ApiVersion { get; set; }
        public string? Kind { get; set; }
        public V1ObjectMeta Metadata { get; set; } = new();
    }

    private sealed class TestDynamicUsage : TestDynamicResource
    {
        public TestDynamicUsageSpec Spec { get; set; } = new();
    }

    private sealed class TestDynamicUsageSpec
    {
        public TestDynamicUsageReference By { get; set; } = new();
        public TestDynamicUsageReference Of { get; set; } = new();
    }

    private sealed class TestDynamicUsageReference
    {
        public string? ApiVersion { get; set; }
        public string? Kind { get; set; }
        public TestDynamicResourceReference ResourceRef { get; set; } = new();
    }

    private sealed class TestDynamicResourceReference
    {
        public string? Name { get; set; }
    }
}
