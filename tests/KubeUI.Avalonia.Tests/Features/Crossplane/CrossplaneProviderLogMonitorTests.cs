using System.Collections.Concurrent;
using System.Text;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;
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
    public void Provider_container_restart_counts_include_the_pod_and_container_identity()
    {
        var pod = CreatePod("provider-hash", restartCount: 3);
        var container = new Mock<IResourceContainer>();
        container.Setup(resourceContainer => resourceContainer.Snapshot())
            .Returns(new List<IKubernetesObject<V1ObjectMeta>> { pod });
        var runtime = new Mock<IClusterRuntime>();
        runtime.SetupGet(cluster => cluster.Objects).Returns(new Dictionary<GroupApiVersionKind, object>
        {
            [GroupApiVersionKind.From<V1Pod>()] = container.Object
        });

        var restartCounts = CrossplaneProviderLogMonitor.GetProviderContainerRestartCounts(runtime.Object, "provider");

        Assert.Equal(3, restartCounts["default\0provider-hash\0provider"]);
    }

    [Fact]
    public async Task Current_stream_retries_after_failure_filters_lines_and_deduplicates_replays()
    {
        var pod = CreatePod("provider-hash");
        var resolver = new FixedPodResolver(pod);
        var streamClient = new ReplayingPodLogStreamClient(failFirstOpen: true);
        using var monitor = new CrossplaneProviderLogMonitor(resolver, streamClient, NullLogger<CrossplaneProviderLogMonitor>.Instance);
        using var cancellation = new CancellationTokenSource();
        var receivedLines = new ConcurrentQueue<string>();
        var receivedLine = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var monitoring = monitor.StartAsync(
            Mock.Of<IClusterRuntime>(),
            new GenericKubernetesObject { Metadata = new V1ObjectMeta { Name = "provider" } },
            (line, _) =>
            {
                receivedLines.Enqueue(line);
                receivedLine.TrySetResult();
                return ValueTask.CompletedTask;
            },
            cancellation.Token);

        await streamClient.SecondFollowOpen.Task.WaitAsync(TimeSpan.FromSeconds(5));
        monitor.Stop();
        await monitoring.WaitAsync(TimeSpan.FromSeconds(5));

        await receivedLine.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.True(streamClient.OpenCount >= 3);
        Assert.Single(receivedLines);
        Assert.Contains("Diff detected", receivedLines.Single(), StringComparison.Ordinal);
        Assert.All(streamClient.Options, options => Assert.Equal(500, options.TailLines));
    }

    [Fact]
    public async Task Restarted_container_reads_previous_logs_once_and_follows_current_logs()
    {
        var pod = CreatePod("provider-hash", restartCount: 1);
        var resolver = new FixedPodResolver(pod);
        var streamClient = new ReplayingPodLogStreamClient(failFirstOpen: false);
        using var monitor = new CrossplaneProviderLogMonitor(resolver, streamClient, NullLogger<CrossplaneProviderLogMonitor>.Instance);
        var receivedLines = new ConcurrentQueue<string>();
        var receivedLine = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var monitoring = monitor.StartAsync(
            Mock.Of<IClusterRuntime>(),
            new GenericKubernetesObject { Metadata = new V1ObjectMeta { Name = "provider" } },
            (line, _) =>
            {
                receivedLines.Enqueue(line);
                receivedLine.TrySetResult();
                return ValueTask.CompletedTask;
            },
            CancellationToken.None);

        await streamClient.SecondFollowOpen.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await receivedLine.Task.WaitAsync(TimeSpan.FromSeconds(5));
        monitor.Stop();
        await monitoring.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Contains(streamClient.Options, options => options.Previous && !options.Follow);
        Assert.Contains(streamClient.Options, options => !options.Previous && options.Follow);
        Assert.All(streamClient.Options, options => Assert.Equal(500, options.TailLines));
        Assert.Single(receivedLines);
    }

    [Fact]
    public async Task Resetting_rows_allows_the_same_provider_log_tail_to_be_replayed()
    {
        var pod = CreatePod("provider-hash");
        var resolver = new FixedPodResolver(pod);
        var streamClient = new ReplayingPodLogStreamClient(failFirstOpen: false);
        using var monitor = new CrossplaneProviderLogMonitor(resolver, streamClient, NullLogger<CrossplaneProviderLogMonitor>.Instance);
        var receivedLines = new ConcurrentQueue<string>();
        var firstLine = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondLine = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var receivedCount = 0;

        ValueTask ReceiveLine(string line, CancellationToken _)
        {
            receivedLines.Enqueue(line);
            var count = Interlocked.Increment(ref receivedCount);
            if (count == 1)
            {
                firstLine.TrySetResult();
            }
            else if (count == 2)
            {
                secondLine.TrySetResult();
            }

            return ValueTask.CompletedTask;
        }

        var firstMonitoring = monitor.StartAsync(
            Mock.Of<IClusterRuntime>(),
            new GenericKubernetesObject { Metadata = new V1ObjectMeta { Name = "provider" } },
            ReceiveLine,
            CancellationToken.None,
            resetSeen: true);
        await streamClient.SecondFollowOpen.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await firstLine.Task.WaitAsync(TimeSpan.FromSeconds(5));
        monitor.Stop();
        await firstMonitoring.WaitAsync(TimeSpan.FromSeconds(5));

        var secondMonitoring = monitor.StartAsync(
            Mock.Of<IClusterRuntime>(),
            new GenericKubernetesObject { Metadata = new V1ObjectMeta { Name = "provider" } },
            ReceiveLine,
            CancellationToken.None,
            resetSeen: true);
        await secondLine.Task.WaitAsync(TimeSpan.FromSeconds(5));
        monitor.Stop();
        await secondMonitoring.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(2, receivedLines.Count);
    }

    [Fact]
    public async Task New_monitor_session_does_not_deduplicate_a_late_line_from_the_previous_session()
    {
        var pod = CreatePod("provider-hash");
        using var streamClient = new GatedPodLogStreamClient();
        using var monitor = new CrossplaneProviderLogMonitor(
            new FixedPodResolver(pod),
            streamClient,
            NullLogger<CrossplaneProviderLogMonitor>.Instance);
        var cluster = Mock.Of<IClusterRuntime>();
        var provider = new GenericKubernetesObject { Metadata = new V1ObjectMeta { Name = "provider" } };
        var activeGeneration = 1;
        var acceptedSessions = new ConcurrentQueue<int>();
        var secondSessionAccepted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        Func<string, CancellationToken, ValueTask> CreateCallback(int session)
            => (_, _) =>
            {
                if (session == Volatile.Read(ref activeGeneration))
                {
                    acceptedSessions.Enqueue(session);
                    if (session == 2)
                    {
                        secondSessionAccepted.TrySetResult();
                    }
                }

                return ValueTask.CompletedTask;
            };

        async Task WaitForStepAsync(Task task, string step)
        {
            try
            {
                await task.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException exception)
            {
                throw new TimeoutException(
                    $"Timed out at {step}; opens={streamClient.OpenCount}, "
                    + $"firstRead={streamClient.FirstReadStarted.Task.IsCompleted}, "
                    + $"secondRead={streamClient.SecondReadStarted.Task.IsCompleted}, "
                    + $"accepted={string.Join(",", acceptedSessions)}",
                    exception);
            }
        }

        var firstMonitoring = monitor.StartAsync(
            cluster,
            provider,
            CreateCallback(1),
            CancellationToken.None,
            resetSeen: true);
        await WaitForStepAsync(streamClient.FirstReadStarted.Task, "first stream read");

        Volatile.Write(ref activeGeneration, 2);
        var secondMonitoring = monitor.StartAsync(
            cluster,
            provider,
            CreateCallback(2),
            CancellationToken.None,
            resetSeen: true);
        await WaitForStepAsync(streamClient.SecondReadStarted.Task, "second stream read");

        streamClient.ReleaseFirstRead();
        await WaitForStepAsync(firstMonitoring, "previous monitoring session completion");
        streamClient.ReleaseSecondRead();
        await WaitForStepAsync(secondSessionAccepted.Task, "current session callback");
        monitor.Stop();
        await WaitForStepAsync(secondMonitoring, "current monitoring session completion");

        Assert.Equal([2], acceptedSessions);
    }

    [Fact]
    public async Task Callback_failure_is_propagated_and_does_not_suppress_a_retry()
    {
        var pod = CreatePod("provider-hash");
        var resolver = new FixedPodResolver(pod);
        var streamClient = new ReplayingPodLogStreamClient(failFirstOpen: false);
        using var monitor = new CrossplaneProviderLogMonitor(resolver, streamClient, NullLogger<CrossplaneProviderLogMonitor>.Instance);
        var cluster = Mock.Of<IClusterRuntime>();
        var provider = new GenericKubernetesObject { Metadata = new V1ObjectMeta { Name = "provider" } };
        var failedMonitoring = monitor.StartAsync(
            cluster,
            provider,
            static (_, _) => throw new InvalidOperationException("callback failed"),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() => failedMonitoring.WaitAsync(TimeSpan.FromSeconds(5)));

        var receivedLine = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var retriedMonitoring = monitor.StartAsync(
            cluster,
            provider,
            (_, _) =>
            {
                receivedLine.TrySetResult();
                return ValueTask.CompletedTask;
            },
            CancellationToken.None);
        await receivedLine.Task.WaitAsync(TimeSpan.FromSeconds(5));
        monitor.Stop();
        await retriedMonitoring.WaitAsync(TimeSpan.FromSeconds(5));
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

    private sealed class GatedPodLogStreamClient : IPodLogStreamClient, IDisposable
    {
        private readonly GatedReadStream _firstStream = new("Diff detected resource diff\n");
        private readonly GatedReadStream _secondStream = new("Diff detected resource diff\n");
        private int _openCount;

        public int OpenCount => Volatile.Read(ref _openCount);
        public TaskCompletionSource FirstReadStarted => _firstStream.ReadStarted;
        public TaskCompletionSource SecondReadStarted => _secondStream.ReadStarted;

        public void ReleaseFirstRead() => _firstStream.Release();
        public void ReleaseSecondRead() => _secondStream.Release();

        public Task<Stream> OpenAsync(
            IClusterRuntime cluster,
            PodLogReadOptions options,
            CancellationToken cancellationToken = default)
        {
            var stream = Interlocked.Increment(ref _openCount) switch
            {
                1 => _firstStream,
                2 => _secondStream,
                _ => Stream.Null
            };
            return Task.FromResult(stream);
        }

        public void Dispose()
        {
            _firstStream.Dispose();
            _secondStream.Dispose();
        }
    }

    private sealed class GatedReadStream(string content) : Stream
    {
        private readonly byte[] _content = Encoding.UTF8.GetBytes(content);
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _offset;

        public TaskCompletionSource ReadStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Release() => _release.TrySetResult();

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => ReadCoreAsync(buffer);

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
            => ReadCoreAsync(buffer.AsMemory(offset, count)).AsTask();

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        private async ValueTask<int> ReadCoreAsync(Memory<byte> buffer)
        {
            ReadStarted.TrySetResult();
            // Let an in-flight read complete after cancellation to reproduce a stale log line.
            await _release.Task.ConfigureAwait(false);
            var count = Math.Min(buffer.Length, _content.Length - _offset);
            _content.AsMemory(_offset, count).CopyTo(buffer);
            _offset += count;
            return count;
        }
    }
}
