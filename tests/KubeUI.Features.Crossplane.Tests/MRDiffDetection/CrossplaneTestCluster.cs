using k8s;
using k8s.Models;
using KubeUI.Kubernetes;
using KubeUI.Testing.Kubernetes.Bootstrap;
using KubeUI.Testing.Utilities;

namespace KubeUI.Features.Crossplane.Tests.MRDiffDetection;

/// <summary>
/// A generated fake cluster whose informers observe the seeded Crossplane fixtures.
/// Disposal disposes the underlying <see cref="TestCluster"/>.
/// </summary>
internal sealed record CrossplaneClusterFixture(TestCluster Cluster) : IAsyncDisposable
{
    public ValueTask DisposeAsync() => Cluster.DisposeAsync();
}

/// <summary>
/// Builds Crossplane MR Diff Detection clusters through the production runtime and
/// Kubernetes client with the fake transport, seeding CRDs and resources via production
/// APIs instead of <see cref="IClusterRuntime"/> doubles.
/// </summary>
internal static class CrossplaneTestCluster
{
    public static readonly GroupApiVersionKind ProviderKind = new("pkg.crossplane.io", "v1", "Provider", "providers");
    public static readonly GroupApiVersionKind WidgetKind = new("example.com", "v1", "Widget", "widgets");

    /// <summary>Creates a cluster with pods only, ready in the pod informer.</summary>
    public static async Task<CrossplaneClusterFixture> CreatePodsAsync(
        CancellationToken cancellationToken,
        KubernetesBackend backend = KubernetesBackend.Fake,
        params V1Pod[] pods)
    {
        var config = TestClusterConfig.Fake();
        config.Type = backend;
        config.Name = "crossplane-test";
        config.InitialResources =
        [
            new V1Namespace { Metadata = new() { Name = "default" } },
            new V1Namespace { Metadata = new() { Name = "crossplane-system" } },
        ];

        var cluster = await new TestClusterGenerator().CreateAsync(config, cancellationToken);
        try
        {
            await cluster.Cluster.Connect().WaitAsync(cancellationToken);
            await WarmPermissionsAsync(cluster, cancellationToken, GroupApiVersionKind.From<V1Pod>());
            await cluster.Cluster.SeedResource<V1Pod>(true, cancellationToken);
            foreach (var pod in pods)
            {
                await cluster.Cluster.CreateAsync(pod, cancellationToken);
            }

            await WaitForResourcesAsync(cluster, cancellationToken, GroupApiVersionKind.From<V1Pod>(), pods.Select(pod => pod.Name()));
            return new CrossplaneClusterFixture(cluster);
        }
        catch
        {
            await cluster.DisposeAsync();
            throw;
        }
    }

    /// <summary>Populates the authorization cache for a kind through production SAR reviews,
    /// which <see cref="Cluster.SeedResource"/> requires before starting informers.</summary>
    private static async Task WarmPermissionsAsync(
        TestCluster cluster,
        CancellationToken cancellationToken,
        params GroupApiVersionKind[] kinds)
    {
        foreach (var kind in kinds)
        {
            await cluster.Cluster.UpdateCanI(kind, Verb.List).WaitAsync(cancellationToken);
            await cluster.Cluster.UpdateCanI(kind, Verb.Watch).WaitAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Creates a cluster with the Provider and Widget CRDs registered, one
    /// <paramref name="providerName"/> resource, one Widget resource, and provider pods.
    /// Informers for both custom resource kinds are ready; the Provider and pod informers
    /// are seeded by the view model under test.
    /// </summary>
    public static async Task<CrossplaneClusterFixture> CreateWithProviderAsync(
        CancellationToken cancellationToken,
        V1Pod[] pods,
        string providerName = "provider-demo",
        string? widgetUid = null)
    {
        var config = TestClusterConfig.Fake();
        config.Name = "test-cluster";
        config.InitialResources =
        [
            new V1Namespace { Metadata = new() { Name = "default" } },
            new V1Namespace { Metadata = new() { Name = "crossplane-system" } },
        ];

        var cluster = await new TestClusterGenerator().CreateAsync(config, cancellationToken);
        try
        {
            await cluster.Cluster.Connect().WaitAsync(cancellationToken);
            await WarmPermissionsAsync(
                cluster,
                cancellationToken,
                GroupApiVersionKind.From<V1CustomResourceDefinition>());
            await cluster.Cluster.SeedResource<V1CustomResourceDefinition>(true, cancellationToken);
            foreach (var crd in (V1CustomResourceDefinition[])[ProviderCrd(), WidgetCrd()])
            {
                await cluster.Cluster.CreateAsync(crd, cancellationToken);
            }

            try
            {
                await TestWait.UntilAsync(
                    () => cluster.Cluster.ModelCatalog.IsCustomResource(ProviderKind)
                        && cluster.Cluster.ModelCatalog.IsCustomResource(WidgetKind),
                    TimeSpan.FromSeconds(10),
                    cancellationToken: cancellationToken);
            }
            catch (TimeoutException)
            {
                var uris = cluster.FakeApi is null
                    ? "<no fake api>"
                    : string.Join(",", cluster.FakeApi.RequestUris.Select(uri => uri?.AbsolutePath).Distinct());
                throw new TimeoutException(
                    $"Custom resource kinds were not registered (provider={cluster.Cluster.ModelCatalog.IsCustomResource(ProviderKind)}, "
                    + $"widget={cluster.Cluster.ModelCatalog.IsCustomResource(WidgetKind)}); requests: {uris}");
            }
            await WarmPermissionsAsync(
                cluster,
                cancellationToken,
                ProviderKind,
                WidgetKind,
                GroupApiVersionKind.From<V1Pod>());
            await cluster.Cluster.SeedResource(ProviderKind, true, cancellationToken);
            await cluster.Cluster.SeedResource(WidgetKind, true, cancellationToken);
            await cluster.Cluster.SeedResource<V1Pod>(true, cancellationToken);

            await CreateCustomResourceAsync(
                cluster,
                cancellationToken,
                ProviderKind,
                new GenericKubernetesObject
                {
                    ApiVersion = "pkg.crossplane.io/v1",
                    Kind = "Provider",
                    Metadata = new() { Name = providerName },
                });
            await CreateCustomResourceAsync(
                cluster,
                cancellationToken,
                WidgetKind,
                new GenericKubernetesObject
                {
                    ApiVersion = "example.com/v1",
                    Kind = "Widget",
                    Metadata = new() { Uid = widgetUid ?? "uid-1", Name = "widget-one", NamespaceProperty = "default" },
                });
            foreach (var pod in pods)
            {
                await cluster.Cluster.CreateAsync(pod, cancellationToken);
            }

            await WaitForResourcesAsync(cluster, cancellationToken, ProviderKind, [providerName]);
            await WaitForResourcesAsync(cluster, cancellationToken, WidgetKind, ["widget-one"]);
            await WaitForResourcesAsync(
                cluster,
                cancellationToken,
                GroupApiVersionKind.From<V1Pod>(),
                pods.Select(pod => pod.Name()));
            return new CrossplaneClusterFixture(cluster);
        }
        catch
        {
            await cluster.DisposeAsync();
            throw;
        }
    }

    /// <summary>
    /// Replaces a pod's status through the status subresource so the informer observes the
    /// change; the fake transport preserves status on regular updates like a real API server.
    /// </summary>
    public static async Task<V1Pod> ReplacePodStatusAsync(
        CrossplaneClusterFixture fixture,
        V1Pod pod,
        string @namespace,
        CancellationToken cancellationToken)
    {
        var replaced = await fixture.Cluster.Cluster.Client!.CoreV1
            .ReplaceNamespacedPodStatusAsync(pod, pod.Name(), @namespace, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        pod.Metadata.Uid = replaced.Metadata.Uid;
        pod.Metadata.ResourceVersion = replaced.Metadata.ResourceVersion;
        return replaced;
    }

    /// <summary>
    /// Creates a custom resource through the production client. A dedicated
    /// <see cref="GenericClient"/> is used because <see cref="GenericKubernetesObject"/>
    /// carries no <see cref="KubernetesEntityAttribute"/> for generic kind resolution.
    /// </summary>
    public static Task CreateCustomResourceAsync(
        CrossplaneClusterFixture fixture,
        CancellationToken cancellationToken,
        GroupApiVersionKind kind,
        GenericKubernetesObject resource)
        => CreateCustomResourceAsync(fixture.Cluster, cancellationToken, kind, resource);

    public static async Task CreateCustomResourceAsync(
        TestCluster cluster,
        CancellationToken cancellationToken,
        GroupApiVersionKind kind,
        GenericKubernetesObject resource)
    {
        using var client = new GenericClient(cluster.Cluster.Client!, kind.Group, kind.ApiVersion, kind.PluralName, false);
        if (string.IsNullOrEmpty(resource.Metadata?.NamespaceProperty))
        {
            await client.CreateAsync(resource).WaitAsync(cancellationToken);
            return;
        }

        await client.CreateNamespacedAsync(resource, resource.Metadata.NamespaceProperty).WaitAsync(cancellationToken);
    }

    /// <summary>Deletes a custom resource through the production client so the informer observes the change.</summary>
    public static Task DeleteCustomResourceAsync(
        CrossplaneClusterFixture fixture,
        CancellationToken cancellationToken,
        GroupApiVersionKind kind,
        GenericKubernetesObject resource)
        => DeleteCustomResourceAsync(fixture.Cluster, cancellationToken, kind, resource);

    public static async Task DeleteCustomResourceAsync(
        TestCluster cluster,
        CancellationToken cancellationToken,
        GroupApiVersionKind kind,
        GenericKubernetesObject resource)
    {
        using var client = new GenericClient(cluster.Cluster.Client!, kind.Group, kind.ApiVersion, kind.PluralName, false);
        if (string.IsNullOrEmpty(resource.Metadata?.NamespaceProperty))
        {
            await client.DeleteAsync<GenericKubernetesObject>(resource.Metadata!.Name).WaitAsync(cancellationToken);
            return;
        }

        await client.DeleteNamespacedAsync<GenericKubernetesObject>(resource.Metadata!.NamespaceProperty, resource.Metadata.Name).WaitAsync(cancellationToken);
    }

    private static async Task WaitForResourcesAsync(
        TestCluster cluster,
        CancellationToken cancellationToken,
        GroupApiVersionKind kind,
        IEnumerable<string> names)
    {
        var expected = names.ToHashSet(StringComparer.Ordinal);
        try
        {
            await TestWait.UntilAsync(
                () => cluster.Cluster.Objects.TryGetValue(kind, out var container)
                    && container is IResourceContainer { IsSeeded: true } seeded
                    && expected.SetEquals(seeded.Snapshot().Select(resource => resource.Name())),
                TimeSpan.FromSeconds(10),
                cancellationToken: cancellationToken);
        }
        catch (TimeoutException)
        {
            var details = "no container";
            if (cluster.Cluster.Objects.TryGetValue(kind, out var container) && container is IResourceContainer seeded)
            {
                details = $"informerCount={seeded.InformerCount}, snapshot=[{string.Join(",", seeded.Snapshot().Select(resource => resource.Name()))}]";
            }

            var uris = cluster.FakeApi is null
                ? "<no fake api>"
                : string.Join(",", cluster.FakeApi.RequestUris.Select(uri => uri?.AbsolutePath));
            throw new TimeoutException(
                $"Resources were not observed for {kind}; expected [{string.Join(",", expected)}], found {details}; requests: {uris}");
        }
    }

    private static V1CustomResourceDefinition ProviderCrd() => new()
    {
        ApiVersion = "apiextensions.k8s.io/v1",
        Kind = "CustomResourceDefinition",
        Metadata = new() { Name = "providers.pkg.crossplane.io" },
        Spec = new()
        {
            Group = ProviderKind.Group,
            Scope = "Cluster",
            Names = new()
            {
                Plural = ProviderKind.PluralName,
                Singular = "provider",
                Kind = ProviderKind.Kind,
                ListKind = "ProviderList",
            },
            Versions = [CrdVersion("v1")],
        },
    };

    private static V1CustomResourceDefinition WidgetCrd() => new()
    {
        ApiVersion = "apiextensions.k8s.io/v1",
        Kind = "CustomResourceDefinition",
        Metadata = new() { Name = "widgets.example.com" },
        Spec = new()
        {
            Group = WidgetKind.Group,
            Scope = "Namespaced",
            Names = new()
            {
                Plural = WidgetKind.PluralName,
                Singular = "widget",
                Kind = WidgetKind.Kind,
                ListKind = "WidgetList",
            },
            Versions = [CrdVersion("v1")],
        },
    };

    private static V1CustomResourceDefinitionVersion CrdVersion(string name) => new()
    {
        Name = name,
        Served = true,
        Storage = true,
        Schema = new()
        {
            OpenAPIV3Schema = new()
            {
                Type = "object",
                Properties = new Dictionary<string, V1JSONSchemaProps>
                {
                    ["apiVersion"] = new() { Type = "string" },
                    ["kind"] = new() { Type = "string" },
                    ["metadata"] = new() { Type = "object" },
                },
            },
        },
    };
}
