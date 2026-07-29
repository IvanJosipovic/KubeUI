using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes.Resources.Relationships;
using KubeUI.Kubernetes.Resources.Relationships.Providers;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Resources.Relationships;

public sealed class ResourceRelationshipBuilderTests
{
    [Fact]
    public void Relates_pod_to_its_node()
    {
        V1Pod pod = new()
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new() { Name = "web", NamespaceProperty = "demo" },
            Spec = new() { NodeName = "node-a" },
        };
        V1Node node = new()
        {
            ApiVersion = "v1",
            Kind = V1Node.KubeKind,
            Metadata = new() { Name = "node-a" },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [pod, node],
            new HashSet<string> { "demo" },
            hideNoise: false);

        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("v1", V1Pod.KubeKind, "demo", "web", null),
            new("v1", V1Node.KubeKind, null, "node-a", null),
            ResourceRelationshipKind.Reference));
    }

    [Fact]
    public void Relates_ingress_to_its_ingress_class()
    {
        V1Ingress ingress = new()
        {
            ApiVersion = "networking.k8s.io/v1",
            Kind = V1Ingress.KubeKind,
            Metadata = new() { Name = "web", NamespaceProperty = "demo" },
            Spec = new() { IngressClassName = "nginx" },
        };
        V1IngressClass ingressClass = new()
        {
            ApiVersion = "networking.k8s.io/v1",
            Kind = V1IngressClass.KubeKind,
            Metadata = new() { Name = "nginx" },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [ingress, ingressClass],
            new HashSet<string> { "demo" },
            hideNoise: true);
        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("networking.k8s.io/v1", V1Ingress.KubeKind, "demo", "web", null),
            new("networking.k8s.io/v1", V1IngressClass.KubeKind, null, "nginx", null),
            ResourceRelationshipKind.Reference));
    }

    [Fact]
    public void Aggregates_seed_prerequisites_from_relationship_providers()
    {
        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            Array.Empty<IKubernetesObject<V1ObjectMeta>>(),
            new HashSet<string>(),
            hideNoise: true);

        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(V1Service)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(V1Ingress)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(V1EndpointSlice)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(V1Pod)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(V1Node)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(V1ConfigMap)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(V1Secret)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(V1PersistentVolumeClaim)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(V1RoleBinding)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(V1ClusterRoleBinding)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(Corev1Event)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(V1ServiceAccount)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(V1PersistentVolume)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(V1StorageClass)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(V1Role)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(typeof(V1ClusterRole)));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(
            new GroupApiVersionKind("protection.crossplane.io", "v1beta1", "Usage", "usages")));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(
            new GroupApiVersionKind("argoproj.io", "v1alpha1", "Application", "applications")));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(
            new GroupApiVersionKind("kustomize.toolkit.fluxcd.io", "v1", "Kustomization", "kustomizations")));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(
            new GroupApiVersionKind("helm.toolkit.fluxcd.io", "v2", "HelmRelease", "helmreleases")));
    }

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
    public void Keeps_cluster_scoped_crossplane_owner_chains_connected_to_selected_namespace()
    {
        TestDynamicResource provider = new()
        {
            ApiVersion = "pkg.crossplane.io/v1",
            Kind = "Provider",
            Metadata = new() { Name = "provider-aws", Uid = "provider-uid" },
        };
        TestDynamicResource providerRevision = new()
        {
            ApiVersion = "pkg.crossplane.io/v1",
            Kind = "ProviderRevision",
            Metadata = new()
            {
                Name = "provider-aws-abc123",
                Uid = "provider-revision-uid",
                OwnerReferences = [new() { ApiVersion = provider.ApiVersion, Kind = provider.Kind, Name = provider.Name(), Uid = provider.Uid() }],
            },
        };
        TestDynamicResource function = new()
        {
            ApiVersion = "pkg.crossplane.io/v1beta1",
            Kind = "Function",
            Metadata = new() { Name = "function-go-templating", Uid = "function-uid" },
        };
        TestDynamicResource functionRevision = new()
        {
            ApiVersion = "pkg.crossplane.io/v1beta1",
            Kind = "FunctionRevision",
            Metadata = new()
            {
                Name = "function-go-templating-abc123",
                Uid = "function-revision-uid",
                OwnerReferences = [new() { ApiVersion = function.ApiVersion, Kind = function.Kind, Name = function.Name(), Uid = function.Uid() }],
            },
        };
        TestDynamicResource selectedProviderResource = new()
        {
            ApiVersion = "example.crossplane.io/v1",
            Kind = "ProviderUsage",
            Metadata = new()
            {
                Name = "provider-usage",
                NamespaceProperty = "crossplane-system",
                OwnerReferences = [new() { ApiVersion = provider.ApiVersion, Kind = provider.Kind, Name = provider.Name(), Uid = provider.Uid() }],
            },
        };
        TestDynamicResource selectedFunctionResource = new()
        {
            ApiVersion = "example.crossplane.io/v1",
            Kind = "FunctionUsage",
            Metadata = new()
            {
                Name = "function-usage",
                NamespaceProperty = "crossplane-system",
                OwnerReferences = [new() { ApiVersion = function.ApiVersion, Kind = function.Kind, Name = function.Name(), Uid = function.Uid() }],
            },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [provider, providerRevision, function, functionRevision, selectedProviderResource, selectedFunctionResource],
            new HashSet<string> { "crossplane-system" },
            hideNoise: true);

        graph.Resources.Select(resource => resource.Name()).ShouldBe(
        [
            "provider-aws",
            "provider-aws-abc123",
            "function-go-templating",
            "function-go-templating-abc123",
            "provider-usage",
            "function-usage",
        ]);
        graph.Relationships.ShouldContain(new ResourceRelationship(
            new(provider.ApiVersion!, provider.Kind!, null, provider.Name()!, provider.Uid()),
            new(providerRevision.ApiVersion!, providerRevision.Kind!, null, providerRevision.Name()!, providerRevision.Uid()),
            ResourceRelationshipKind.Owner));
        graph.Relationships.ShouldContain(new ResourceRelationship(
            new(function.ApiVersion!, function.Kind!, null, function.Name()!, function.Uid()),
            new(functionRevision.ApiVersion!, functionRevision.Kind!, null, functionRevision.Name()!, functionRevision.Uid()),
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
        V1Pod completed = new()
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new() { Name = "completed", NamespaceProperty = "demo" },
            Status = new() { Phase = "Succeeded" },
        };
        V1Pod failed = new()
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new() { Name = "failed", NamespaceProperty = "demo" },
            Status = new() { Phase = "Failed" },
        };
        V1Pod other = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "other", NamespaceProperty = "other" } };
        Corev1Event noise = new() { ApiVersion = "v1", Kind = Corev1Event.KubeKind, Metadata = new() { Name = "event", NamespaceProperty = "demo" } };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build([selected, completed, failed, other, noise], new HashSet<string> { "demo" }, hideNoise: true);

        graph.Resources.Select(x => x.Name()).ShouldBe(["selected", "failed"]);
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
    public void Addition_delta_does_not_expand_global_parents_to_namespaced_children()
    {
        V1Pod selected = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "selected", NamespaceProperty = "demo", Uid = "selected-uid" } };
        V1Node parent = new() { ApiVersion = "v1", Kind = V1Node.KubeKind, Metadata = new() { Name = "parent", Uid = "parent-uid" } };
        V1Pod unrelatedChild = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "unrelated", NamespaceProperty = "other", Uid = "unrelated-uid" } };
        ResourceRelationshipBuilder builder = new([new GlobalParentProvider()]);

        ResourceRelationshipGraph delta = builder.BuildAdditionDelta(
            [selected, parent, unrelatedChild],
            new ResourceKey("v1", V1Node.KubeKind, null, "parent"),
            new HashSet<string>(),
            hideNoise: true);

        delta.Resources.Select(resource => resource.Name()).ShouldContain("parent");
        delta.Resources.Select(resource => resource.Name()).ShouldNotContain("unrelated");
    }

    [Fact]
    public void Addition_delta_excludes_unrelated_namespaced_resources()
    {
        V1Pod selected = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "selected", NamespaceProperty = "demo", Uid = "selected-uid" } };
        V1Pod unrelated = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "unrelated", NamespaceProperty = "other", Uid = "unrelated-uid" } };

        ResourceRelationshipGraph delta = new ResourceRelationshipBuilder().BuildAdditionDelta(
            [selected, unrelated],
            new ResourceKey("v1", V1Pod.KubeKind, "other", "unrelated"),
            new HashSet<string> { "demo" },
            hideNoise: true);

        delta.Resources.ShouldBeEmpty();
    }

    [Fact]
    public void Namespace_filter_does_not_traverse_owner_relationships_across_namespaces()
    {
        V1Deployment owner = new()
        {
            ApiVersion = "apps/v1",
            Kind = V1Deployment.KubeKind,
            Metadata = new() { Name = "owner", NamespaceProperty = "demo", Uid = "owner-uid" },
        };
        V1Pod unrelatedChild = new()
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new()
            {
                Name = "unrelated-child",
                NamespaceProperty = "other",
                Uid = "unrelated-child-uid",
                OwnerReferences =
                [
                    new()
                    {
                        ApiVersion = owner.ApiVersion,
                        Kind = owner.Kind,
                        Name = owner.Name(),
                        Uid = owner.Uid(),
                    },
                ],
            },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [owner, unrelatedChild],
            new HashSet<string> { "demo" },
            hideNoise: true);

        graph.Resources.Select(resource => resource.Name()).ShouldBe(["owner"]);
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
    public void Resolves_argo_application_from_instance_annotation()
    {
        V1Deployment managed = new()
        {
            ApiVersion = "apps/v1",
            Kind = V1Deployment.KubeKind,
            Metadata = new()
            {
                Name = "managed",
                NamespaceProperty = "workload",
                Annotations = new Dictionary<string, string> { ["argocd.argoproj.io/instance"] = "demo-app" },
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
            new("apps/v1", V1Deployment.KubeKind, "workload", "managed", null),
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
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(
            new GroupApiVersionKind("argoproj.io", "v1alpha1", "Application", "applications")));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(
            new GroupApiVersionKind("kustomize.toolkit.fluxcd.io", "v1", "Kustomization", "kustomizations")));
        graph.RequiredSeedPrerequisites.ShouldContain(new ResourceSeedPrerequisite(
            new GroupApiVersionKind("helm.toolkit.fluxcd.io", "v2", "HelmRelease", "helmreleases")));
    }

    [Fact]
    public void Resolves_flux_helm_release_from_cert_manager_labels_across_api_versions()
    {
        V1Deployment deployment = new()
        {
            ApiVersion = "apps/v1",
            Kind = V1Deployment.KubeKind,
            Metadata = new()
            {
                Name = "cert-manager",
                NamespaceProperty = "cert-manager",
                Labels = new Dictionary<string, string>
                {
                    ["helm.toolkit.fluxcd.io/name"] = "cert-manager",
                    ["helm.toolkit.fluxcd.io/namespace"] = "cert-manager",
                },
            },
        };
        V1ConfigMap helmRelease = new()
        {
            ApiVersion = "helm.toolkit.fluxcd.io/v2beta1",
            Kind = "HelmRelease",
            Metadata = new() { Name = "cert-manager", NamespaceProperty = "cert-manager", Uid = "helm-uid" },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [deployment, helmRelease],
            new HashSet<string> { "cert-manager" },
            hideNoise: true);

        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("helm.toolkit.fluxcd.io/v2beta1", "HelmRelease", "cert-manager", "cert-manager", "helm-uid"),
            new("apps/v1", V1Deployment.KubeKind, "cert-manager", "cert-manager", null),
            ResourceRelationshipKind.GitOps));
    }

    [Fact]
    public void Relates_flux_helm_release_to_its_kustomization()
    {
        TestDynamicResource helmRelease = new()
        {
            ApiVersion = "helm.toolkit.fluxcd.io/v2",
            Kind = "HelmRelease",
            Metadata = new()
            {
                Name = "envoy-envoy-gateway-system-public-5ea56ca9",
                NamespaceProperty = "envoy-gateway-system",
                Labels = new Dictionary<string, string>
                {
                    ["kustomize.toolkit.fluxcd.io/name"] = "app",
                    ["kustomize.toolkit.fluxcd.io/namespace"] = "flux-system",
                },
            },
        };
        TestDynamicResource kustomization = new()
        {
            ApiVersion = "kustomize.toolkit.fluxcd.io/v1",
            Kind = "Kustomization",
            Metadata = new() { Name = "app", NamespaceProperty = "flux-system" },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [helmRelease, kustomization],
            new HashSet<string> { "envoy-gateway-system" },
            hideNoise: true);

        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("kustomize.toolkit.fluxcd.io/v1", "Kustomization", "flux-system", "app", null),
            new("helm.toolkit.fluxcd.io/v2", "HelmRelease", "envoy-gateway-system", "envoy-envoy-gateway-system-public-5ea56ca9", null),
            ResourceRelationshipKind.GitOps));
    }

    [Fact]
    public void Tracks_unresolved_flux_helm_release_from_labels()
    {
        V1Deployment deployment = new()
        {
            ApiVersion = "apps/v1",
            Kind = V1Deployment.KubeKind,
            Metadata = new()
            {
                Name = "cert-manager",
                NamespaceProperty = "cert-manager",
                Labels = new Dictionary<string, string>
                {
                    ["helm.toolkit.fluxcd.io/name"] = "cert-manager",
                    ["helm.toolkit.fluxcd.io/namespace"] = "cert-manager",
                },
            },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [deployment],
            new HashSet<string> { "cert-manager" },
            hideNoise: true);

        graph.PendingReferences.ShouldContain(new UnresolvedResourceReference(
            "helm.toolkit.fluxcd.io",
            null,
            "HelmRelease",
            "cert-manager",
            "cert-manager"));
    }

    [Fact]
    public void Tracks_unresolved_gitops_controllers_from_resource_metadata()
    {
        V1Deployment deployment = new()
        {
            ApiVersion = "apps/v1",
            Kind = V1Deployment.KubeKind,
            Metadata = new()
            {
                Name = "managed",
                NamespaceProperty = "demo",
                Annotations = new Dictionary<string, string>
                {
                    ["argocd.argoproj.io/tracking-id"] = "demo-app:apps/Deployment:demo/managed",
                },
                Labels = new Dictionary<string, string>
                {
                    ["helm.toolkit.fluxcd.io/name"] = "release-a",
                    ["helm.toolkit.fluxcd.io/namespace"] = "demo",
                    ["kustomize.toolkit.fluxcd.io/name"] = "kustomization-a",
                    ["kustomize.toolkit.fluxcd.io/namespace"] = "demo",
                },
            },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [deployment],
            new HashSet<string> { "demo" },
            hideNoise: true);

        graph.PendingReferences.Count.ShouldBe(3);
        graph.PendingReferences.ShouldContain(new UnresolvedResourceReference("argoproj.io", "v1alpha1", "Application", null, "demo-app"));
        graph.PendingReferences.ShouldContain(new UnresolvedResourceReference("helm.toolkit.fluxcd.io", null, "HelmRelease", "demo", "release-a"));
        graph.PendingReferences.ShouldContain(new UnresolvedResourceReference("kustomize.toolkit.fluxcd.io", null, "Kustomization", "demo", "kustomization-a"));
    }

    [Fact]
    public void Tracks_unresolved_argo_application_from_data_product_tracking_id()
    {
        TestDynamicResource dataProduct = new()
        {
            ApiVersion = "data.platform.da.teck.com/v1alpha1",
            Kind = "DataProduct",
            Metadata = new()
            {
                Name = "canary",
                NamespaceProperty = "platform-test-data-product",
                Annotations = new Dictionary<string, string>
                {
                    ["argocd.argoproj.io/tracking-id"] = "platform-test-data-product:data.platform.da.teck.com/DataProduct:platform-test-data-product/canary",
                },
            },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [dataProduct],
            new HashSet<string> { "platform-test-data-product" },
            hideNoise: true);

        graph.PendingReferences.ShouldContain(new UnresolvedResourceReference(
            "argoproj.io",
            "v1alpha1",
            "Application",
            null,
            "platform-test-data-product"));
    }

    [Fact]
    public void Relates_service_to_matching_pods_and_endpoint_slices()
    {
        V1Service service = new()
        {
            ApiVersion = "v1",
            Kind = V1Service.KubeKind,
            Metadata = new() { Name = "web", NamespaceProperty = "demo" },
            Spec = new() { Selector = new Dictionary<string, string> { ["app"] = "web" } },
        };
        V1Pod pod = new()
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new() { Name = "web-0", NamespaceProperty = "demo", Uid = "pod-uid", Labels = new Dictionary<string, string> { ["app"] = "web" } },
        };
        V1EndpointSlice endpointSlice = new()
        {
            ApiVersion = "discovery.k8s.io/v1",
            Kind = V1EndpointSlice.KubeKind,
            Metadata = new()
            {
                Name = "web-abc",
                NamespaceProperty = "demo",
                Labels = new Dictionary<string, string> { ["kubernetes.io/service-name"] = "web" },
            },
            Endpoints = [new() { TargetRef = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Name = "web-0", NamespaceProperty = "demo", Uid = "pod-uid" } }],
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [service, pod, endpointSlice],
            new HashSet<string> { "demo" },
            hideNoise: true);
        ResourceRelationshipGraph endpointGraph = new ResourceRelationshipBuilder([new EndpointSliceRelationshipProvider()]).Build(
            [service, pod, endpointSlice],
            new HashSet<string> { "demo" },
            hideNoise: true);
        endpointGraph.Relationships.ShouldContain(new ResourceRelationship(
            new("discovery.k8s.io/v1", V1EndpointSlice.KubeKind, "demo", "web-abc", null),
            new("v1", V1Pod.KubeKind, "demo", "web-0", "pod-uid"),
            ResourceRelationshipKind.Reference));
        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("discovery.k8s.io/v1", V1EndpointSlice.KubeKind, "demo", "web-abc", null),
            new("v1", V1Pod.KubeKind, "demo", "web-0", "pod-uid"),
            ResourceRelationshipKind.Reference));
        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("v1", V1Service.KubeKind, "demo", "web", null),
            new("v1", V1Pod.KubeKind, "demo", "web-0", "pod-uid"),
            ResourceRelationshipKind.Selector));
    }

    [Fact]
    public void Relates_pod_disruption_budget_to_matching_pods()
    {
        V1PodDisruptionBudget budget = new()
        {
            ApiVersion = "policy/v1",
            Kind = V1PodDisruptionBudget.KubeKind,
            Metadata = new() { Name = "web", NamespaceProperty = "demo" },
            Spec = new() { Selector = new V1LabelSelector { MatchExpressions = [new() { Key = "tier", OperatorProperty = "In", Values = ["frontend"] }] } },
        };
        V1Pod pod = new()
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new() { Name = "web-0", NamespaceProperty = "demo", Labels = new Dictionary<string, string> { ["tier"] = "frontend" } },
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [budget, pod],
            new HashSet<string> { "demo" },
            hideNoise: true);

        graph.Relationships.ShouldContain(new ResourceRelationship(
            new("policy/v1", V1PodDisruptionBudget.KubeKind, "demo", "web", null),
            new("v1", V1Pod.KubeKind, "demo", "web-0", null),
            ResourceRelationshipKind.Selector));
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

    private sealed class GlobalParentProvider : IResourceRelationshipProvider
    {
        public void AddRelationships(
            IKubernetesObject<V1ObjectMeta> resource,
            ResourceRelationshipContext context,
            ICollection<ResourceRelationship> relationships)
        {
            if (resource.Name() != "parent")
            {
                return;
            }

            if (context.TryGet("v1", V1Pod.KubeKind, "demo", "selected", out IKubernetesObject<V1ObjectMeta>? selected)
                && selected != null)
            {
                context.Add(relationships, resource, selected, ResourceRelationshipKind.Owner);
            }

            if (context.TryGet("v1", V1Pod.KubeKind, "other", "unrelated", out IKubernetesObject<V1ObjectMeta>? unrelated)
                && unrelated != null)
            {
                context.Add(relationships, resource, unrelated, ResourceRelationshipKind.Owner);
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
