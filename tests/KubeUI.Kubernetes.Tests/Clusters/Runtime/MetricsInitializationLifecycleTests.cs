using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using k8s.Models;
using KubeUI.Testing.Kubernetes.Bootstrap;
using KubeUI.Testing.Kubernetes.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Clusters.Runtime;

public sealed class MetricsInitializationLifecycleTests
{
    [Fact]
    public async Task Connect_returns_while_metrics_initialize_and_disconnect_cancels_initialization()
    {
        var settings = new KubernetesTestSettingsStore();
        var metrics = new BlockingMetricsService();
        await using var services = KubernetesTestServiceProvider.Build(
            settings,
            collection => collection.AddSingleton<IMetricsService>(metrics));
        var generator = new TestClusterGenerator(services);
        await using var testCluster = await generator.CreateAsync(
            TestClusterConfig.Fake(),
            TestContext.Current.CancellationToken);

        var connectTask = testCluster.Cluster.Connect();
        await metrics.InitializationStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
        await connectTask.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        testCluster.Cluster.Connected.ShouldBeTrue();
        metrics.InitializationCompleted.Task.IsCompleted.ShouldBeFalse();

        await testCluster.Cluster.Disconnect().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await metrics.InitializationCancelled.Task.WaitAsync(TestContext.Current.CancellationToken);
        testCluster.Cluster.Connected.ShouldBeFalse();
    }

    private sealed class BlockingMetricsService : IMetricsService
    {
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ObservableCollection<PodMetrics> KubernetesPodMetrics { get; } = [];

        public ObservableCollection<NodeMetrics> KubernetesNodeMetrics { get; } = [];

        public bool IsMetricsAvailable => false;

        public ActiveMetricsBackend ActiveMetricsBackend => ActiveMetricsBackend.None;

        public TaskCompletionSource InitializationStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource InitializationCancelled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource InitializationCompleted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add { }
            remove { }
        }

        public ImmutableArray<PodMetrics> GetKubernetesPodMetricsSnapshot(string? namespaceName, string podName)
            => ImmutableArray<PodMetrics>.Empty;

        public ImmutableArray<NodeMetrics> GetKubernetesNodeMetricsSnapshot(string nodeName)
            => ImmutableArray<NodeMetrics>.Empty;

        public ImmutableArray<PodMetrics> GetAllKubernetesPodMetricsSnapshot()
            => ImmutableArray<PodMetrics>.Empty;

        public ImmutableArray<PodMetrics> GetCurrentKubernetesPodMetricsSnapshot()
            => ImmutableArray<PodMetrics>.Empty;

        public ImmutableArray<NodeMetrics> GetAllKubernetesNodeMetricsSnapshot()
            => ImmutableArray<NodeMetrics>.Empty;

        public async Task InitializeAsync(Cluster cluster, CancellationToken cancellationToken = default)
        {
            InitializationStarted.TrySetResult();
            try
            {
                await _release.Task.WaitAsync(cancellationToken);
                InitializationCompleted.TrySetResult();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                InitializationCancelled.TrySetResult();
                throw;
            }
        }

        public Task StopAsync() => Task.CompletedTask;

        public Task<IReadOnlyList<MetricProviderInfo>> GetAvailablePrometheusProvidersAsync()
            => Task.FromResult<IReadOnlyList<MetricProviderInfo>>([]);

        public Task<MetricResultSet> QueryPrometheusAsync(MetricRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(MetricResultSet.BackendUnavailableResult);
    }
}
