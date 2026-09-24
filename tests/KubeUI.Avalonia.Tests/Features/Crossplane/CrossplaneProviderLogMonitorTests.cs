using System.Collections.Concurrent;
using System.Text;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace KubeUI.Avalonia.Tests.Features.Crossplane;

public sealed class CrossplaneProviderLogMonitorTests
{
    [Fact]
    public void Provider_label_prevents_prefix_fallback_to_another_provider()
    {
        var correctlyLabeledDifferentProvider = CreatePod(
            "provider-beta-hash",
            labels: new Dictionary<string, string> { ["pkg.crossplane.io/provider"] = "provider-beta" });
        var unlabeledProviderPod = CreatePod("provider-hash");
        var container = new Mock<IResourceContainer>();
        container.Setup(resourceContainer => resourceContainer.Snapshot())
            .Returns(new List<IKubernetesObject<V1ObjectMeta>> { correctlyLabeledDifferentProvider, unlabeledProviderPod });
        var runtime = new Mock<IClusterRuntime>();
        runtime.SetupGet(cluster => cluster.Objects).Returns(new Dictionary<GroupApiVersionKind, object>
        {
            [GroupApiVersionKind.From<V1Pod>()] = container.Object
        });

        var podKeys = CrossplaneProviderLogMonitor.GetProviderPodKeys(runtime.Object, "provider");

        Assert.Equal(["default\0provider-hash"], podKeys);
    }

    [Fact]
    public async Task Current_stream_retries_after_failure_filters_lines_and_deduplicates_replays()
    {
        V1Pod pod = CreatePod("provider-hash");
        var resolver = new FixedPodResolver(pod);
        var streamClient = new ReplayingPodLogStreamClient(failFirstOpen: true);
        using var monitor = new CrossplaneProviderLogMonitor(resolver, streamClient, NullLogger<CrossplaneProviderLogMonitor>.Instance);
        using var cancellation = new CancellationTokenSource();
        var receivedLines = new ConcurrentQueue<string>();
        var receivedLine = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var monitoring = monitor.StartAsync(
            Mock.Of<IClusterRuntime>(),
            new GenericKubernetesObject { Metadata = new V1ObjectMeta { Name = "provider" } },
            line =>
            {
                receivedLines.Enqueue(line);
                receivedLine.TrySetResult();
            },
            cancellation.Token);

        await streamClient.SecondFollowOpen.Task.WaitAsync(TimeSpan.FromSeconds(5));
        monitor.Stop();
        await monitoring.WaitAsync(TimeSpan.FromSeconds(5));

        await receivedLine.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.True(streamClient.OpenCount >= 3);
        Assert.Single(receivedLines);
        Assert.Contains("Diff detected", receivedLines.Single(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Restarted_container_reads_previous_logs_once_and_follows_current_logs()
    {
        V1Pod pod = CreatePod("provider-hash", restartCount: 1);
        var resolver = new FixedPodResolver(pod);
        var streamClient = new ReplayingPodLogStreamClient(failFirstOpen: false);
        using var monitor = new CrossplaneProviderLogMonitor(resolver, streamClient, NullLogger<CrossplaneProviderLogMonitor>.Instance);
        var receivedLines = new ConcurrentQueue<string>();
        var monitoring = monitor.StartAsync(
            Mock.Of<IClusterRuntime>(),
            new GenericKubernetesObject { Metadata = new V1ObjectMeta { Name = "provider" } },
            receivedLines.Enqueue,
            CancellationToken.None);

        await streamClient.SecondFollowOpen.Task.WaitAsync(TimeSpan.FromSeconds(5));
        monitor.Stop();
        await monitoring.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Contains(streamClient.Options, options => options.Previous && !options.Follow);
        Assert.Contains(streamClient.Options, options => !options.Previous && options.Follow);
        Assert.Single(receivedLines);
    }

    private static V1Pod CreatePod(string name, IDictionary<string, string>? labels = null, int restartCount = 0)
        => new()
        {
            Metadata = new V1ObjectMeta { Name = name, NamespaceProperty = "default", Labels = labels },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "provider" }] },
            Status = new V1PodStatus
            {
                ContainerStatuses = [new V1ContainerStatus { Name = "provider", RestartCount = restartCount }]
            }
        };

    private sealed class FixedPodResolver(V1Pod pod) : IPodLogSessionResolver
    {
        public PodLogSessionState CreateState(
            IKubernetesObject<V1ObjectMeta> resource,
            string containerName,
            bool previous,
            bool timestamps,
            int tailLines = 500)
            => new("default", "provider", null, "Provider", null, null, null, containerName, previous, timestamps, tailLines);

        public PodLogMultiSessionState CreateMultiState(
            IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources,
            string containerName,
            bool previous,
            bool timestamps,
            int tailLines = 500)
            => throw new NotSupportedException();

        public PodLogSessionResolution? TryResolve(IClusterRuntime cluster, PodLogSessionState state)
            => new(pod, state.ContainerName, [pod], false, true, null);

        public PodLogMultiSessionResolution TryResolve(IClusterRuntime cluster, PodLogMultiSessionState state)
            => throw new NotSupportedException();
    }

    private sealed class ReplayingPodLogStreamClient(bool failFirstOpen) : IPodLogStreamClient
    {
        private readonly ConcurrentQueue<PodLogReadOptions> _options = new();
        private int _openCount;

        public int OpenCount => Volatile.Read(ref _openCount);
        public IReadOnlyCollection<PodLogReadOptions> Options => _options.ToArray();
        public TaskCompletionSource SecondFollowOpen { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<Stream> OpenAsync(IClusterRuntime cluster, PodLogReadOptions options, CancellationToken cancellationToken = default)
        {
            _options.Enqueue(options);
            var openCount = Interlocked.Increment(ref _openCount);
            if (options.Follow && openCount >= (failFirstOpen ? 3 : 2))
            {
                SecondFollowOpen.TrySetResult();
            }

            if (failFirstOpen && openCount == 1)
            {
                throw new InvalidOperationException("simulated stream failure");
            }

            var payload = Encoding.UTF8.GetBytes("ordinary provider message\nDiff detected resource diff\n");
            return Task.FromResult<Stream>(new MemoryStream(payload));
        }
    }
}
