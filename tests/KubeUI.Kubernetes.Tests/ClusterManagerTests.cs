using System.Collections.ObjectModel;
using k8s;
using k8s.KubeConfigModels;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;
using KubeUI.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public class ClusterManagerTests
{
    [Fact]
    public async Task LoadFromConfig_MarshalsClusterAdditionsThroughDispatcher()
    {
        var dispatcher = new RecordingThreadDispatcher();
        var kubeConfigPath = Path.Combine(Path.GetTempPath(), $"kubeui-{Guid.NewGuid():N}.config");

        var services = new ServiceCollection()
            .AddSingleton<IThreadDispatcher>(dispatcher)
            .AddSingleton<IKubeConfigPathProvider>(new TestKubeConfigPathProvider(kubeConfigPath))
            .AddSingleton<IClusterSettingsStore>(new TestClusterSettingsStore(kubeConfigPath))
            .AddTransient<IClusterRuntime, TestClusterRuntime>()
            .BuildServiceProvider();

        var manager = new ClusterManager(
            NullLogger<ClusterManager>.Instance,
            services,
            services.GetRequiredService<IClusterSettingsStore>(),
            dispatcher,
            services.GetRequiredService<IKubeConfigPathProvider>());

        var config = new K8SConfiguration
        {
            FileName = "/tmp/config",
            Contexts =
            [
                new Context
                {
                    Name = "dev-cluster",
                }
            ]
        };

        await Task.Run(() => manager.LoadFromConfig(config));

        manager.Clusters.ShouldBeEmpty();
        dispatcher.PendingCount.ShouldBeGreaterThan(0);

        dispatcher.Drain();

        var cluster = manager.GetCluster("dev-cluster");
        cluster.ShouldNotBeNull();
        cluster!.KubeConfigPath.ShouldBe("/tmp/config");
    }

    [Fact]
    public async Task ImportIntoKubeConfig_merges_config_into_default_kubeconfig()
    {
        var dispatcher = new RecordingThreadDispatcher();
        var kubeConfigPath = Path.Combine(Path.GetTempPath(), $"kubeui-{Guid.NewGuid():N}.config");
        var services = new ServiceCollection()
            .AddSingleton<IThreadDispatcher>(dispatcher)
            .AddSingleton<IKubeConfigPathProvider>(new TestKubeConfigPathProvider(kubeConfigPath))
            .AddSingleton<IClusterSettingsStore>(new TestClusterSettingsStore(kubeConfigPath))
            .AddTransient<IClusterRuntime, TestClusterRuntime>()
            .BuildServiceProvider();

        K8SConfiguration existingConfig = CreateKubeConfig("existing-context", "existing-cluster", "existing-user", "https://existing.example.com");
        File.WriteAllText(kubeConfigPath, KubeUI.Kubernetes.Serialization.KubernetesYaml.Serialize(existingConfig));

        var manager = new ClusterManager(
            NullLogger<ClusterManager>.Instance,
            services,
            services.GetRequiredService<IClusterSettingsStore>(),
            dispatcher,
            services.GetRequiredService<IKubeConfigPathProvider>());

        dispatcher.Drain();

        manager.GetCluster("existing-context").ShouldNotBeNull();

        K8SConfiguration importedConfig = CreateKubeConfig("imported-context", "imported-cluster", "imported-user", "https://imported.example.com");
        string importedYaml = KubeUI.Kubernetes.Serialization.KubernetesYaml.Serialize(importedConfig);
        importedYaml.ShouldContain("imported-context");
        importedYaml.ShouldContain("imported-cluster");

        manager.ImportIntoKubeConfig(importedConfig);
        dispatcher.Drain();

        string rawYaml = File.ReadAllText(kubeConfigPath);
        rawYaml.ShouldContain("imported-context");
        rawYaml.ShouldContain("imported-cluster");

        var mergedConfig = KubernetesClientConfiguration.LoadKubeConfig(kubeConfigPath);
        mergedConfig.Contexts.Select(x => x.Name).ShouldContain("existing-context");
        mergedConfig.Contexts.Select(x => x.Name).ShouldContain("imported-context");

        manager.GetCluster("imported-context").ShouldNotBeNull();
    }

    [Fact]
    public async Task RemoveCluster_uses_context_name_when_cluster_name_differs()
    {
        var dispatcher = new RecordingThreadDispatcher();
        var kubeConfigPath = Path.Combine(Path.GetTempPath(), $"kubeui-{Guid.NewGuid():N}.config");
        var services = new ServiceCollection()
            .AddSingleton<IThreadDispatcher>(dispatcher)
            .AddSingleton<IKubeConfigPathProvider>(new TestKubeConfigPathProvider(kubeConfigPath))
            .AddSingleton<IClusterSettingsStore>(new TestClusterSettingsStore(kubeConfigPath))
            .AddTransient<IClusterRuntime, TestClusterRuntime>()
            .BuildServiceProvider();

        K8SConfiguration config = CreateKubeConfig(
            "clusterUser_test-aks_test-aks",
            "test-aks",
            "user-1",
            "https://test-aks.example.com");

        config.FileName = kubeConfigPath;
        File.WriteAllText(kubeConfigPath, KubeUI.Kubernetes.Serialization.KubernetesYaml.Serialize(config));

        var manager = new ClusterManager(
            NullLogger<ClusterManager>.Instance,
            services,
            services.GetRequiredService<IClusterSettingsStore>(),
            dispatcher,
            services.GetRequiredService<IKubeConfigPathProvider>());

        dispatcher.Drain();

        var cluster = manager.GetCluster("clusterUser_test-aks_test-aks");
        cluster.ShouldNotBeNull();

        manager.RemoveCluster(cluster!);

        dispatcher.Drain();

        manager.GetCluster("clusterUser_test-aks_test-aks").ShouldBeNull();
        var updated = KubernetesClientConfiguration.LoadKubeConfig(kubeConfigPath);
        updated.Contexts.Select(x => x.Name).ShouldNotContain("clusterUser_test-aks_test-aks");
    }

    private sealed class RecordingThreadDispatcher : IThreadDispatcher
    {
        private readonly Queue<Action> _pending = new();

        public int PendingCount => _pending.Count;

        public void Invoke(Action action)
        {
            ArgumentNullException.ThrowIfNull(action);

            lock (_pending)
            {
                _pending.Enqueue(action);
            }
        }

        public void Drain()
        {
            while (true)
            {
                Action? next;
                lock (_pending)
                {
                    if (_pending.Count == 0)
                    {
                        return;
                    }

                    next = _pending.Dequeue();
                }

                next();
            }
        }
    }

    private sealed class TestClusterSettingsStore : IClusterSettingsStore
    {
        private readonly Collection<string> _kubeConfigPaths;

        public TestClusterSettingsStore(string kubeConfigPath)
        {
            _kubeConfigPaths = [kubeConfigPath];
        }

        public IReadOnlyCollection<string> KubeConfigPaths => _kubeConfigPaths;

        public void AddKubeConfigPath(string path)
        {
            if (!_kubeConfigPaths.Contains(path))
            {
                _kubeConfigPaths.Add(path);
            }
        }

        public IReadOnlyCollection<string> GetClusterNamespaces(IClusterRuntime cluster)
        {
            return [];
        }

        public ClusterMetricsSettings GetClusterMetricsSettings(IClusterRuntime cluster)
        {
            return new ClusterMetricsSettings();
        }

        public void Persist()
        {
        }
    }

    private sealed class TestKubeConfigPathProvider : IKubeConfigPathProvider
    {
        public TestKubeConfigPathProvider(string defaultPath)
        {
            DefaultPath = defaultPath;
        }

        public string DefaultPath { get; }
    }

    private static K8SConfiguration CreateKubeConfig(string contextName, string clusterName, string userName, string server)
    {
        k8s.KubeConfigModels.Cluster cluster = new()
        {
            Name = clusterName,
            ClusterEndpoint = new ClusterEndpoint
            {
                Server = server
            }
        };

        User user = new()
        {
            Name = userName,
            UserCredentials = new UserCredentials()
        };

        Context context = new()
        {
            Name = contextName,
            ContextDetails = new ContextDetails
            {
                Cluster = clusterName,
                User = userName
            }
        };

        return new K8SConfiguration
        {
            ApiVersion = "v1",
            Kind = "Config",
            CurrentContext = contextName,
            Clusters =
            [
                cluster
            ],
            Users =
            [
                user
            ],
            Contexts =
            [
                context
            ]
        };
    }
}
