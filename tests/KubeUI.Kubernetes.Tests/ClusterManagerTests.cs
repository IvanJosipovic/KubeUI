using System.Collections.ObjectModel;
using k8s;
using k8s.KubeConfigModels;
using KubeUI.Kubernetes;
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
        var previousKubeConfig = Environment.GetEnvironmentVariable("KUBECONFIG");
        Environment.SetEnvironmentVariable("KUBECONFIG", kubeConfigPath);

        var services = new ServiceCollection()
            .AddSingleton<IThreadDispatcher>(dispatcher)
            .AddSingleton<IClusterSettingsStore>(new TestClusterSettingsStore(kubeConfigPath))
            .AddTransient<IClusterRuntime, TestClusterRuntime>()
            .BuildServiceProvider();

        try
        {
            var manager = new ClusterManager(
                NullLogger<ClusterManager>.Instance,
                services,
                services.GetRequiredService<IClusterSettingsStore>(),
                dispatcher);

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
        finally
        {
            Environment.SetEnvironmentVariable("KUBECONFIG", previousKubeConfig);
        }
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
}
