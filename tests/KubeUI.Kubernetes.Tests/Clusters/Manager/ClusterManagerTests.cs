using KubeUI.Kubernetes.Serialization;
using System.Reactive.Concurrency;
using k8s;
using k8s.KubeConfigModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Clusters.Manager;

public class ClusterManagerTests
{
    [Fact]
    public async Task LoadClustersAsync_treats_a_missing_kubeconfig_collection_as_empty()
    {
        var dispatcher = new RecordingThreadDispatcher();
        var kubeConfigPath = Path.Combine(Path.GetTempPath(), $"kubeui-{Guid.NewGuid():N}.config");
        await using var services = KubernetesTestServiceProvider.Build(new NullKubeConfigSettingsStore(), collection =>
        {
            collection.AddSingleton<IThreadDispatcher>(dispatcher);
            collection.AddSingleton<IKubeConfigPathProvider>(new TestKubeConfigPathProvider(kubeConfigPath));
        });

        using var manager = new ClusterManager(
            NullLogger<ClusterManager>.Instance,
            services,
            services.GetRequiredService<IClusterSettingsStore>(),
            dispatcher,
            services.GetRequiredService<IKubeConfigPathProvider>(),
            services.GetRequiredService<IKubeConfigDiscovery>(),
            services.GetRequiredService<IKubernetesYamlSerializer>());

        await manager.LoadClustersAsync(TestContext.Current.CancellationToken);

        manager.Clusters.ShouldBeEmpty();
    }

    [Fact]
    public async Task LoadFromConfig_MarshalsClusterAdditionsThroughDispatcher()
    {
        var dispatcher = new RecordingThreadDispatcher();
        var kubeConfigPath = Path.Combine(Path.GetTempPath(), $"kubeui-{Guid.NewGuid():N}.config");

        await using var services = KubernetesTestServiceProvider.Build(new KubernetesTestSettingsStore(kubeConfigPath), collection =>
        {
            collection.AddSingleton<IThreadDispatcher>(dispatcher);
            collection.AddSingleton<IKubeConfigPathProvider>(new TestKubeConfigPathProvider(kubeConfigPath));
        });

        using var manager = new ClusterManager(
            NullLogger<ClusterManager>.Instance,
            services,
            services.GetRequiredService<IClusterSettingsStore>(),
            dispatcher,
            services.GetRequiredService<IKubeConfigPathProvider>(),
            services.GetRequiredService<IKubeConfigDiscovery>(),
            services.GetRequiredService<IKubernetesYamlSerializer>());

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

        await Task.Run(() => manager.LoadFromConfig(config), TestContext.Current.CancellationToken);

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
        await using var services = KubernetesTestServiceProvider.Build(new KubernetesTestSettingsStore(kubeConfigPath), collection =>
        {
            collection.AddSingleton<IThreadDispatcher>(dispatcher);
            collection.AddSingleton<IKubeConfigPathProvider>(new TestKubeConfigPathProvider(kubeConfigPath));
        });

        var existingConfig = CreateKubeConfig("existing-context", "existing-cluster", "existing-user", "https://existing.example.com");
        File.WriteAllText(kubeConfigPath, Serialization.KubernetesYaml.Serialize(existingConfig));

        using var manager = new ClusterManager(
            NullLogger<ClusterManager>.Instance,
            services,
            services.GetRequiredService<IClusterSettingsStore>(),
            dispatcher,
            services.GetRequiredService<IKubeConfigPathProvider>(),
            services.GetRequiredService<IKubeConfigDiscovery>(),
            services.GetRequiredService<IKubernetesYamlSerializer>());

        await manager.LoadClustersAsync(TestContext.Current.CancellationToken);
        dispatcher.Drain();
        manager.GetCluster("existing-context").ShouldNotBeNull();

        var importedConfig = CreateKubeConfig("imported-context", "imported-cluster", "imported-user", "https://imported.example.com");
        var importedYaml = Serialization.KubernetesYaml.Serialize(importedConfig);
        importedYaml.ShouldContain("imported-context");
        importedYaml.ShouldContain("imported-cluster");

        manager.ImportIntoKubeConfig(importedConfig);
        dispatcher.Drain();

        var rawYaml = File.ReadAllText(kubeConfigPath);
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
        await using var services = KubernetesTestServiceProvider.Build(new KubernetesTestSettingsStore(kubeConfigPath), collection =>
        {
            collection.AddSingleton<IThreadDispatcher>(dispatcher);
            collection.AddSingleton<IKubeConfigPathProvider>(new TestKubeConfigPathProvider(kubeConfigPath));
        });

        var config = CreateKubeConfig(
            "clusterUser_test-aks_test-aks",
            "test-aks",
            "user-1",
            "https://test-aks.example.com");

        config.FileName = kubeConfigPath;
        File.WriteAllText(kubeConfigPath, Serialization.KubernetesYaml.Serialize(config));

        using var manager = new ClusterManager(
            NullLogger<ClusterManager>.Instance,
            services,
            services.GetRequiredService<IClusterSettingsStore>(),
            dispatcher,
            services.GetRequiredService<IKubeConfigPathProvider>(),
            services.GetRequiredService<IKubeConfigDiscovery>(),
            services.GetRequiredService<IKubernetesYamlSerializer>());

        await manager.LoadClustersAsync(TestContext.Current.CancellationToken);
        dispatcher.Drain();

        var cluster = manager.GetCluster("clusterUser_test-aks_test-aks");
        cluster.ShouldNotBeNull();

        manager.RemoveCluster(cluster!);

        dispatcher.Drain();

        manager.GetCluster("clusterUser_test-aks_test-aks").ShouldBeNull();
        var updated = KubernetesClientConfiguration.LoadKubeConfig(kubeConfigPath);
        updated.Contexts.Select(x => x.Name).ShouldNotContain("clusterUser_test-aks_test-aks");
    }

    [Fact]
    public async Task LoadClustersAsync_discovers_kubeconfigs_in_discovery_directory_without_persisting_them()
    {
        using var directory = new TemporaryDirectory();
        var defaultPath = WriteKubeConfig(directory.Path, "config", "default-context");
        var discoveredPath = WriteKubeConfig(directory.Path, "dev.yaml", "dev-context");
        File.WriteAllText(Path.Combine(directory.Path, "notes.txt"), "not a kubeconfig");
        var settings = new KubernetesTestSettingsStore();
        await using var scope = CreateManager(settings, defaultPath, directory.Path);

        await scope.Manager.LoadClustersAsync(TestContext.Current.CancellationToken);
        scope.Dispatcher.Drain();

        scope.Manager.GetCluster("default-context").ShouldNotBeNull();
        scope.Manager.GetCluster("dev-context")!.KubeConfigPath.ShouldBe(discoveredPath);
        scope.Manager.Clusters.Count.ShouldBe(2);
        settings.KubeConfigPaths.ShouldBe([defaultPath]);
    }

    [Fact]
    public async Task Deleting_discovered_file_removes_its_clusters()
    {
        using var directory = new TemporaryDirectory();
        var defaultPath = WriteKubeConfig(directory.Path, "config", "default-context");
        var discoveredPath = WriteKubeConfig(directory.Path, "dev.yaml", "dev-context");
        await using var scope = CreateManager(new KubernetesTestSettingsStore(), defaultPath, directory.Path);
        await scope.Manager.LoadClustersAsync(TestContext.Current.CancellationToken);
        scope.Dispatcher.Drain();
        scope.Manager.GetCluster("dev-context").ShouldNotBeNull();

        File.Delete(discoveredPath);

        await KubeUI.Testing.Utilities.TestWait.UntilAsync(
            () => scope.Manager.GetCluster("dev-context") is null,
            TimeSpan.FromSeconds(3), cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: scope.Dispatcher.Drain);
        scope.Manager.GetCluster("default-context").ShouldNotBeNull();
    }

    [Fact]
    public async Task Discovered_contexts_do_not_duplicate_existing_names()
    {
        using var directory = new TemporaryDirectory();
        var defaultPath = WriteKubeConfig(directory.Path, "config", "shared-context");
        WriteKubeConfig(directory.Path, "other.yaml", "shared-context");
        await using var scope = CreateManager(new KubernetesTestSettingsStore(), defaultPath, directory.Path);

        await scope.Manager.LoadClustersAsync(TestContext.Current.CancellationToken);
        scope.Dispatcher.Drain();

        scope.Manager.Clusters.Count.ShouldBe(1);
        scope.Manager.GetCluster("shared-context")!.KubeConfigPath.ShouldBe(defaultPath);
    }

    [Fact]
    public async Task Disabling_discovery_discards_queued_discovered_additions()
    {
        using var directory = new TemporaryDirectory();
        var defaultPath = WriteKubeConfig(directory.Path, "config", "default-context");
        WriteKubeConfig(directory.Path, "dev.yaml", "dev-context");
        await using var scope = CreateManager(new KubernetesTestSettingsStore(), defaultPath, directory.Path);

        await scope.Manager.LoadClustersAsync(TestContext.Current.CancellationToken);
        List<string> addedNames = [];
        scope.Manager.Clusters.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (IClusterRuntime cluster in e.NewItems)
                {
                    addedNames.Add(cluster.Name);
                }
            }
        };
        scope.Manager.SetKubeConfigAutoDiscovery(false);
        scope.Dispatcher.Drain();

        addedNames.ShouldNotContain("dev-context");
        scope.Manager.GetCluster("default-context").ShouldNotBeNull();
        scope.Manager.GetCluster("dev-context").ShouldBeNull();
    }

    [Fact]
    public async Task Enabling_discovery_does_not_block_and_disabling_cancels_a_pending_scan()
    {
        using var directory = new TemporaryDirectory();
        var defaultPath = Path.Combine(directory.Path, "config");
        var discoveredPath = WriteKubeConfig(directory.Path, "dev.yaml", "dev-context");
        using var discovery = new BlockingDiscovery(discoveredPath, TestContext.Current.CancellationToken);
        var settings = new KubernetesTestSettingsStore { KubeConfigAutoDiscoveryEnabled = false };
        await using var scope = new ManagerScope(settings, defaultPath, directory.Path, discovery);

        try
        {
            var enable = Task.Run(() => scope.Manager.SetKubeConfigAutoDiscovery(true), TestContext.Current.CancellationToken);
            await discovery.Started.Task.WaitAsync(TestContext.Current.CancellationToken);
            await enable.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            var discoveryTask = scope.Manager.DiscoveryTask.ShouldNotBeNull();
            scope.Manager.SetKubeConfigAutoDiscovery(false);
            discovery.Release();
            await discovery.Finished.Task.WaitAsync(TestContext.Current.CancellationToken);
            await discoveryTask.WaitAsync(TestContext.Current.CancellationToken);
            scope.Dispatcher.Drain();

            scope.Manager.GetCluster("dev-context").ShouldBeNull();
        }
        finally
        {
            discovery.Release();
        }
    }

    [Fact]
    public async Task Dispose_can_be_called_more_than_once()
    {
        using var directory = new TemporaryDirectory();
        await using var scope = CreateManager(new KubernetesTestSettingsStore(), Path.Combine(directory.Path, "config"), directory.Path);

        scope.Manager.Dispose();
        scope.Manager.Dispose();
    }

    [Fact]
    public async Task LoadClustersAsync_skips_discovery_when_disabled()
    {
        using var directory = new TemporaryDirectory();
        var defaultPath = WriteKubeConfig(directory.Path, "config", "default-context");
        WriteKubeConfig(directory.Path, "dev.yaml", "dev-context");
        var settings = new KubernetesTestSettingsStore { KubeConfigAutoDiscoveryEnabled = false };
        await using var scope = CreateManager(settings, defaultPath, directory.Path);

        await scope.Manager.LoadClustersAsync(TestContext.Current.CancellationToken);
        scope.Dispatcher.Drain();

        scope.Manager.GetCluster("default-context").ShouldNotBeNull();
        scope.Manager.GetCluster("dev-context").ShouldBeNull();
    }

    [Fact]
    public async Task SetKubeConfigAutoDiscovery_unloads_and_reloads_discovered_clusters()
    {
        using var directory = new TemporaryDirectory();
        var defaultPath = WriteKubeConfig(directory.Path, "config", "default-context");
        WriteKubeConfig(directory.Path, "dev.yaml", "dev-context");
        var settings = new KubernetesTestSettingsStore();
        await using var scope = CreateManager(settings, defaultPath, directory.Path);
        await scope.Manager.LoadClustersAsync(TestContext.Current.CancellationToken);
        scope.Dispatcher.Drain();

        scope.Manager.SetKubeConfigAutoDiscovery(false);
        scope.Dispatcher.Drain();

        settings.KubeConfigAutoDiscoveryEnabled.ShouldBeFalse();
        scope.Manager.KubeConfigAutoDiscoveryEnabled.ShouldBeFalse();
        scope.Manager.GetCluster("dev-context").ShouldBeNull();
        scope.Manager.GetCluster("default-context").ShouldNotBeNull();

        scope.Manager.SetKubeConfigAutoDiscovery(true);
        await scope.Manager.DiscoveryTask!.WaitAsync(TestContext.Current.CancellationToken);
        scope.Dispatcher.Drain();

        settings.KubeConfigAutoDiscoveryEnabled.ShouldBeTrue();
        scope.Manager.GetCluster("dev-context").ShouldNotBeNull();
    }

    [Fact]
    public async Task RemoveKubeConfig_removes_clusters_and_persisted_path_without_touching_file()
    {
        using var directory = new TemporaryDirectory();
        using var otherDirectory = new TemporaryDirectory();
        var defaultPath = WriteKubeConfig(directory.Path, "config", "default-context");
        var customPath = WriteKubeConfig(otherDirectory.Path, "custom.yaml", "custom-context");
        var settings = new KubernetesTestSettingsStore();
        await using var scope = CreateManager(settings, defaultPath, directory.Path);
        await scope.Manager.LoadClustersAsync(TestContext.Current.CancellationToken);
        scope.Manager.LoadFromConfigFromPath(customPath);
        scope.Dispatcher.Drain();
        scope.Manager.GetCluster("custom-context").ShouldNotBeNull();
        settings.KubeConfigPaths.ShouldContain(customPath);

        scope.Manager.RemoveKubeConfig(customPath);
        scope.Dispatcher.Drain();

        scope.Manager.GetCluster("custom-context").ShouldBeNull();
        settings.KubeConfigPaths.ShouldNotContain(customPath);
        File.Exists(customPath).ShouldBeTrue();
    }

    [Fact]
    public async Task Late_file_change_event_after_RemoveKubeConfig_does_not_restore_it()
    {
        using var directory = new TemporaryDirectory();
        using var otherDirectory = new TemporaryDirectory();
        var defaultPath = WriteKubeConfig(directory.Path, "config", "default-context");
        var customPath = WriteKubeConfig(otherDirectory.Path, "custom.yaml", "custom-context");
        var settings = new KubernetesTestSettingsStore();
        await using var scope = CreateManager(settings, defaultPath, directory.Path);
        await scope.Manager.LoadClustersAsync(TestContext.Current.CancellationToken);
        scope.Manager.LoadFromConfigFromPath(customPath);
        scope.Dispatcher.Drain();
        scope.Manager.RemoveKubeConfig(customPath);
        scope.Dispatcher.Drain();

        scope.Manager.OnKubeConfigFileChanged(customPath);
        scope.Dispatcher.Drain();

        scope.Manager.GetCluster("custom-context").ShouldBeNull();
        settings.KubeConfigPaths.ShouldNotContain(customPath);
    }

    [Fact]
    public async Task RemoveKubeConfig_keeps_clusters_from_discoverable_file_and_ignores_default_path()
    {
        using var directory = new TemporaryDirectory();
        var defaultPath = WriteKubeConfig(directory.Path, "config", "default-context");
        var discoverablePath = WriteKubeConfig(directory.Path, "dev.yaml", "dev-context");
        var settings = new KubernetesTestSettingsStore();
        await using var scope = CreateManager(settings, defaultPath, directory.Path);
        await scope.Manager.LoadClustersAsync(TestContext.Current.CancellationToken);
        scope.Manager.LoadFromConfigFromPath(discoverablePath);
        scope.Dispatcher.Drain();

        scope.Manager.RemoveKubeConfig(discoverablePath);
        scope.Manager.RemoveKubeConfig(defaultPath);
        scope.Dispatcher.Drain();

        settings.KubeConfigPaths.ShouldNotContain(discoverablePath);
        settings.KubeConfigPaths.ShouldContain(defaultPath);
        scope.Manager.GetCluster("dev-context").ShouldNotBeNull();
        scope.Manager.GetCluster("default-context").ShouldNotBeNull();
    }

    private static ManagerScope CreateManager(KubernetesTestSettingsStore settings, string defaultPath, string? discoveryDirectory)
        => new(settings, defaultPath, discoveryDirectory);

    private static string WriteKubeConfig(string directory, string fileName, string contextName)
    {
        var path = Path.Combine(directory, fileName);
        var config = CreateKubeConfig(contextName, contextName + "-cluster", contextName + "-user", "https://" + contextName + ".example.com");
        File.WriteAllText(path, Serialization.KubernetesYaml.Serialize(config));
        return path;
    }

    private sealed class ManagerScope : IAsyncDisposable
    {
        private readonly ServiceProvider _services;

        public ManagerScope(KubernetesTestSettingsStore settings, string defaultPath, string? discoveryDirectory, IKubeConfigDiscovery? discovery = null)
        {
            Dispatcher = new RecordingThreadDispatcher();
            var dispatcher = Dispatcher;
            _services = KubernetesTestServiceProvider.Build(settings, collection =>
            {
                collection.AddSingleton<IThreadDispatcher>(dispatcher);
                collection.AddSingleton<IKubeConfigPathProvider>(new TestKubeConfigPathProvider(defaultPath, discoveryDirectory));
                if (discovery is not null)
                {
                    collection.AddSingleton(discovery);
                }
            });

            Manager = new ClusterManager(
                NullLogger<ClusterManager>.Instance,
                _services,
                settings,
                dispatcher,
                _services.GetRequiredService<IKubeConfigPathProvider>(),
                _services.GetRequiredService<IKubeConfigDiscovery>(),
                _services.GetRequiredService<IKubernetesYamlSerializer>());
        }

        public ClusterManager Manager { get; }

        public RecordingThreadDispatcher Dispatcher { get; }

        public async ValueTask DisposeAsync()
        {
            Manager.Dispose();
            await _services.DisposeAsync();
        }
    }

    private sealed class BlockingDiscovery(string path, CancellationToken cancellationToken) : IKubeConfigDiscovery, IDisposable
    {
        private readonly ManualResetEventSlim _release = new();
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Finished { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public IReadOnlyList<string> DiscoverFiles(string directory)
        {
            Started.TrySetResult();
            _release.Wait(cancellationToken);
            Finished.TrySetResult();
            return [path];
        }

        public bool IsKubeConfig(string candidate) => candidate == path;

        public void Release() => _release.Set();

        public void Dispose() => _release.Dispose();
    }

    private sealed class RecordingThreadDispatcher : IThreadDispatcher
    {
        private readonly Queue<Action> _pending = new();

        public IScheduler Scheduler => ImmediateScheduler.Instance;

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

    private sealed class TestKubeConfigPathProvider : IKubeConfigPathProvider
    {
        public TestKubeConfigPathProvider(string defaultPath, string? discoveryDirectory = null)
        {
            DefaultPath = defaultPath;
            DiscoveryDirectory = discoveryDirectory;
        }

        public string DefaultPath { get; }

        public string? DiscoveryDirectory { get; }
    }

    private sealed class NullKubeConfigSettingsStore : IClusterSettingsStore
    {
        public IReadOnlyCollection<string> KubeConfigPaths => null!;

        public bool KubeConfigAutoDiscoveryEnabled { get; set; }

        public void RemoveKubeConfigPath(string path)
        {
        }

        public void AddKubeConfigPath(string path)
        {
        }

        public IReadOnlyCollection<string> GetClusterNamespaces(IClusterRuntime cluster) => [];
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
