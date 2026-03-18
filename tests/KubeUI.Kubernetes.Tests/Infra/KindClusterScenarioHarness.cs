using System.Text;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubeUI.Avalonia;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KubeUI.Kubernetes.Tests.Infra;

public sealed class KindClusterScenarioHarness : IClusterScenarioHarness
{
    private readonly ServiceProvider _services;
    private readonly TestSettingsService _settingsService;
    private string _name = Guid.NewGuid().ToString("N");

    public KindClusterScenarioHarness()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Information));
        services.AddKubeUIKubernetesServices();
        services.AddSingleton<ISettingsService, TestSettingsService>();
        services.AddSingleton<IClusterSettingsStore>(sp => sp.GetRequiredService<ISettingsService>().Clusters);
        services.AddSingleton<IHostApplicationLifetime, TestHostApplicationLifetime>();

        _services = services.BuildServiceProvider();
        _services.ConfigureKubeUIKubernetesJson();
        _settingsService = (TestSettingsService)_services.GetRequiredService<ISettingsService>();
    }

    public IClusterRuntime Cluster { get; private set; } = null!;

    public k8s.Kubernetes Kubernetes { get; private set; } = null!;

    public K8SConfiguration KubeConfig { get; private set; } = null!;

    public bool SupportsLimitedAccessScenarios => true;

    public async Task InitializeAsync()
    {
        await Kind.DownloadClient();
        await Kind.CreateCluster(_name);

        KubeConfig = await Kind.GetK8SConfiguration(_name);
        Kubernetes = await Kind.GetKubernetesClient(_name);
        Cluster = await CreateClusterAsync($"kind-{_name}", KubeConfig);
    }

    public async Task<T> CreateDirectAsync<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var client = Kubernetes.GetGenericClient<T>();

        if (string.IsNullOrEmpty(item.Namespace()))
        {
            return await client.CreateAsync<T>(item);
        }

        return await client.CreateNamespacedAsync<T>(item, item.Namespace());
    }

    public async Task CreateCustomResourceDefinitionAsync(V1CustomResourceDefinition crd)
    {
        await Kubernetes.CreateCustomResourceDefinitionAsync(crd);
    }

    public async Task<IClusterRuntime> CreateLimitedAccessClusterAsync(bool includeNamespaceFallback)
    {
        var yaml = includeNamespaceFallback ? SharedScenarioData.LimitedAccessNoNamespaceYaml : SharedScenarioData.LimitedAccessYaml;

        await Cluster.ImportYaml(new MemoryStream(Encoding.UTF8.GetBytes(yaml)));
        await Cluster.SeedResource<V1ServiceAccount>(true);
        await ClusterScenarioAssertions.WaitForResourceAsync<V1ServiceAccount>(Cluster, "my-app", "my-serviceaccount");

        var secret = await WaitForServiceAccountTokenAsync("my-app", "my-serviceaccount");
        var config = KubeUI.Kubernetes.Serialization.KubernetesYaml.Deserialize<K8SConfiguration>(KubeUI.Kubernetes.Serialization.KubernetesYaml.Serialize(KubeConfig));
        var clusterName = includeNamespaceFallback ? "limited-fallback" : "limited";

        config.Clusters.First().Name = clusterName;
        var context = config.Contexts.First();
        context.Name = clusterName;
        context.ContextDetails.Cluster = clusterName;
        context.ContextDetails.User = clusterName;

        var user = config.Users.First();
        user.Name = clusterName;
        user.UserCredentials = new() { Token = Encoding.UTF8.GetString(secret.Data!["token"]) };

        var limited = await CreateClusterAsync(clusterName, config);

        if (includeNamespaceFallback)
        {
            _settingsService.Settings = new Settings();
            _settingsService.Settings.GetClusterSettings(limited).Namespaces.Add("my-app");
        }

        return limited;
    }

    public async ValueTask DisposeAsync()
    {
        await _services.DisposeAsync();

        try
        {
            await Kind.DeleteCluster(_name);
        }
        catch
        {
        }
    }

    private async Task<IClusterRuntime> CreateClusterAsync(string name, K8SConfiguration config)
    {
        var cluster = _services.GetRequiredService<IClusterRuntime>();
        cluster.Name = name;
        cluster.KubeConfig = config;
        cluster.KubeConfigPath = string.Empty;
        await cluster.Connect();
        return cluster;
    }

    private async Task<V1Secret> WaitForServiceAccountTokenAsync(string @namespace, string name)
    {
        var timeout = TimeSpan.FromMinutes(2);
        var start = DateTime.UtcNow;

        while (true)
        {
            var secret = await Kubernetes.CoreV1.ReadNamespacedSecretAsync(name, @namespace);
            if (secret.Data != null && secret.Data.ContainsKey("token"))
            {
                return secret;
            }

            if (DateTime.UtcNow - start > timeout)
            {
                throw new InvalidOperationException($"Secret '{name}' in namespace '{@namespace}' does not contain a token after waiting.");
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}


