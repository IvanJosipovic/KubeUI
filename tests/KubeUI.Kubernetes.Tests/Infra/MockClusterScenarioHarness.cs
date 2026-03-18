using System.Text;
using k8s;
using k8s.Models;
using KubeUI.Kubernetes;
using KubeUI.Testing;

namespace KubeUI.Kubernetes.Tests.Infra;

public sealed class MockClusterScenarioHarness : IClusterScenarioHarness
{
    private const string LimitedNamespace = "my-app";
    private const string LimitedServiceAccountName = "my-serviceaccount";
    private readonly TestClusterRuntime _cluster = new();

    public IClusterRuntime Cluster => _cluster;

    public bool SupportsLimitedAccessScenarios => true;

    public async Task InitializeAsync()
    {
        _cluster.Name = "mock";
        await _cluster.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "default" }
        });
        await _cluster.AddOrUpdateResource(new V1Node
        {
            Metadata = new V1ObjectMeta { Name = "node-1" }
        });
    }

    public Task<T> CreateDirectAsync<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return CreateOrUpdateAsync(item);
    }

    public async Task CreateCustomResourceDefinitionAsync(V1CustomResourceDefinition crd)
    {
        await _cluster.AddOrUpdateResource(crd);
    }

    public async Task<IClusterRuntime> CreateLimitedAccessClusterAsync(bool includeNamespaceFallback)
    {
        await EnsureLimitedAccessResourcesAsync();

        var limited = new TestClusterRuntime
        {
            Name = includeNamespaceFallback ? "mock-limited-fallback" : "mock-limited",
            DefaultPermissionAllowed = false,
            ListNamespaces = !includeNamespaceFallback,
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        await limited.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = LimitedNamespace }
        });
        await limited.AddOrUpdateResource(new V1Node
        {
            Metadata = new V1ObjectMeta { Name = "node-1" }
        });
        await limited.AddOrUpdateResource(new V1Secret
        {
            ApiVersion = V1Secret.KubeApiVersion,
            Kind = V1Secret.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = LimitedServiceAccountName,
                NamespaceProperty = LimitedNamespace
            },
            Data = new Dictionary<string, byte[]>
            {
                ["token"] = Encoding.UTF8.GetBytes("fake-token")
            }
        });

        ConfigureLimitedPermissions(limited);

        return limited;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    private async Task EnsureLimitedAccessResourcesAsync()
    {
        await _cluster.ImportYaml(new MemoryStream(Encoding.UTF8.GetBytes(SharedScenarioData.LimitedAccessYaml)));
        await _cluster.SeedResource<V1ServiceAccount>(true);
    }

    private static void ConfigureLimitedPermissions(TestClusterRuntime cluster)
    {
        SetAll(cluster, typeof(V1Namespace), false);
        SetAll(cluster, typeof(V1Pod), false);

        cluster.SetPermission<V1Node>(Verb.Get, true);
        cluster.SetPermission<V1Node>(Verb.List, true);
        cluster.SetPermission<V1Node>(Verb.Watch, true);

        cluster.SetPermission<V1Secret>(Verb.Get, true, LimitedNamespace);
        cluster.SetPermission<V1Secret>(Verb.List, true, LimitedNamespace);
        cluster.SetPermission<V1Secret>(Verb.Watch, true, LimitedNamespace);

        cluster.SetPermission<V1Pod>(Verb.Delete, true, LimitedNamespace);
        cluster.SetPermission<V1Pod>(Verb.Get, true, LimitedNamespace);
        cluster.SetPermission<V1Pod>(Verb.List, true, LimitedNamespace);
        cluster.SetPermission<V1Pod>(Verb.Watch, true, LimitedNamespace);
        cluster.SetPermission<V1Pod>(Verb.Get, true, LimitedNamespace, "log");
        cluster.SetPermission<V1Pod>(Verb.Create, true, LimitedNamespace, "exec");
        cluster.SetPermission<V1Pod>(Verb.Create, true, LimitedNamespace, "portforward");
    }

    private static void SetAll(TestClusterRuntime cluster, Type type, bool allowed)
    {
        cluster.SetPermission(type, Verb.Create, allowed);
        cluster.SetPermission(type, Verb.Delete, allowed);
        cluster.SetPermission(type, Verb.Get, allowed);
        cluster.SetPermission(type, Verb.List, allowed);
        cluster.SetPermission(type, Verb.Patch, allowed);
        cluster.SetPermission(type, Verb.Update, allowed);
        cluster.SetPermission(type, Verb.Watch, allowed);
        cluster.SetPermission(type, Verb.Get, allowed, subresource: "log");
        cluster.SetPermission(type, Verb.Create, allowed, subresource: "exec");
        cluster.SetPermission(type, Verb.Create, allowed, subresource: "portforward");
    }

    private async Task<T> CreateOrUpdateAsync<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await _cluster.AddOrUpdateResource(item);
        return item;
    }
}

