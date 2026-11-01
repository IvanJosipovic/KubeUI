using System.Collections.Immutable;
using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes;

internal sealed class KubernetesMetricsCollector(
    ILogger logger,
    IThreadDispatcher dispatcher,
    ObservableCollection<PodMetrics> podMetrics,
    ObservableCollection<NodeMetrics> nodeMetrics) : IDisposable
{
    private static readonly TimeSpan s_retention = TimeSpan.FromHours(1);
    private static readonly TimeSpan s_disposeTimeout = TimeSpan.FromSeconds(5);
    private readonly SemaphoreSlim _syncLock = new(1, 1);
    private ImmutableDictionary<string, ImmutableArray<PodMetrics>> _podMetricsByResource =
        ImmutableDictionary.Create<string, ImmutableArray<PodMetrics>>(StringComparer.Ordinal);
    private ImmutableDictionary<string, ImmutableArray<NodeMetrics>> _nodeMetricsByResource =
        ImmutableDictionary.Create<string, ImmutableArray<NodeMetrics>>(StringComparer.Ordinal);
    private ImmutableArray<PodMetrics> _allPodMetrics = ImmutableArray<PodMetrics>.Empty;
    private ImmutableArray<NodeMetrics> _allNodeMetrics = ImmutableArray<NodeMetrics>.Empty;
    private readonly Lock _snapshotLock = new();
    private int _disposed;
    private CancellationTokenSource? _cancellation;
    private PeriodicTimer? _timer;
    private Task? _refreshTask;
    private int _generation;

    public async Task<bool> StartAsync(Cluster cluster, CancellationToken cancellationToken)
    {
        if (cluster.Client is not k8s.Kubernetes kube || !await CanUseMetricsServerAsync(cluster, kube, cancellationToken).ConfigureAwait(false))
        {
            logger.LogInformation("Kubernetes Metrics Server is not available for cluster {Name}.", cluster.Name);
            return false;
        }

        logger.LogInformation("Kubernetes Metrics Server metrics activated for cluster {Name}.", cluster.Name);
        _cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        Interlocked.Increment(ref _generation);
        await SyncAsync(cluster, _cancellation.Token).ConfigureAwait(false);
        _refreshTask = Task.Run(async () =>
        {
            try
            {
                while (await _timer.WaitForNextTickAsync(_cancellation.Token).ConfigureAwait(false))
                {
                    await SyncAsync(cluster, _cancellation.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Kubernetes Metrics Server refresh loop failed for cluster {Name}.", cluster.Name);
            }
        }, _cancellation.Token);
        return true;
    }

    public async Task StopAsync()
    {
        var cancellation = _cancellation;
        _cancellation = null;
        cancellation?.Cancel();
        _timer?.Dispose();
        _timer = null;
        Interlocked.Increment(ref _generation);
        if (_refreshTask is { } task)
        {
            try
            { await task.ConfigureAwait(false); }
            catch (OperationCanceledException) when (cancellation?.IsCancellationRequested == true) { }
            finally { _refreshTask = null; cancellation?.Dispose(); }
        }
        else
        {
            cancellation?.Dispose();
        }
    }

    /// <summary>Stops the refresh loop without blocking the calling thread indefinitely.</summary>
    /// <remarks>
    /// The stop operation waits for in-flight Kubernetes API calls to complete, which can take an
    /// unbounded amount of time on an unresponsive cluster. Blocking the UI thread on that would hang
    /// the application during shutdown, so the wait is bounded and the remaining work is abandoned.
    /// </remarks>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        try
        {
            StopAsync().WaitAsync(s_disposeTimeout).GetAwaiter().GetResult();
        }
        catch (TimeoutException)
        {
            logger.LogWarning("Timed out while stopping the Kubernetes Metrics Server collector; continuing without waiting for the refresh loop.");
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Ignoring failure while stopping the Kubernetes Metrics Server collector.");
        }

        _syncLock.Dispose();
    }

    public ImmutableArray<PodMetrics> GetPodMetricsSnapshot(string? namespaceName, string podName)
    {
        if (string.IsNullOrWhiteSpace(podName))
        {
            return ImmutableArray<PodMetrics>.Empty;
        }

        var key = string.Concat(namespaceName, "/", podName);
        var snapshot = Volatile.Read(ref _podMetricsByResource);
        return snapshot.TryGetValue(key, out var metrics) ? metrics : ImmutableArray<PodMetrics>.Empty;
    }

    public ImmutableArray<NodeMetrics> GetNodeMetricsSnapshot(string nodeName)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
        {
            return ImmutableArray<NodeMetrics>.Empty;
        }

        var snapshot = Volatile.Read(ref _nodeMetricsByResource);
        return snapshot.TryGetValue(nodeName, out var metrics) ? metrics : ImmutableArray<NodeMetrics>.Empty;
    }

    /// <summary>Gets an immutable snapshot of every retained pod metric.</summary>
    /// <returns>The retained pod metrics.</returns>
    public ImmutableArray<PodMetrics> GetAllPodMetricsSnapshot()
    {
        lock (_snapshotLock)
        {
            return _allPodMetrics;
        }
    }

    /// <summary>Gets an immutable snapshot of every retained node metric.</summary>
    /// <returns>The retained node metrics.</returns>
    public ImmutableArray<NodeMetrics> GetAllNodeMetricsSnapshot()
    {
        lock (_snapshotLock)
        {
            return _allNodeMetrics;
        }
    }

    public void RebuildSnapshotIndexes()
    {
        var podMetricsByResource = ImmutableDictionary.CreateBuilder<string, ImmutableArray<PodMetrics>>(StringComparer.Ordinal);
        Dictionary<string, List<PodMetrics>> podMetricGroups = new(StringComparer.Ordinal);
        foreach (var metric in podMetrics)
        {
            var key = string.Concat(metric.Namespace(), "/", metric.Name());
            if (!podMetricGroups.TryGetValue(key, out var group))
            {
                group = [];
                podMetricGroups.Add(key, group);
            }

            group.Add(metric);
        }

        foreach (var (key, group) in podMetricGroups)
        {
            podMetricsByResource.Add(key, group.ToImmutableArray());
        }

        var nodeMetricsByResource = ImmutableDictionary.CreateBuilder<string, ImmutableArray<NodeMetrics>>(StringComparer.Ordinal);
        Dictionary<string, List<NodeMetrics>> nodeMetricGroups = new(StringComparer.Ordinal);
        foreach (var metric in nodeMetrics)
        {
            var key = metric.Name() ?? string.Empty;
            if (!nodeMetricGroups.TryGetValue(key, out var group))
            {
                group = [];
                nodeMetricGroups.Add(key, group);
            }

            group.Add(metric);
        }

        foreach (var (key, group) in nodeMetricGroups)
        {
            nodeMetricsByResource.Add(key, group.ToImmutableArray());
        }

        Volatile.Write(ref _podMetricsByResource, podMetricsByResource.ToImmutable());
        Volatile.Write(ref _nodeMetricsByResource, nodeMetricsByResource.ToImmutable());
        lock (_snapshotLock)
        {
            _allPodMetrics = podMetrics.ToImmutableArray();
            _allNodeMetrics = nodeMetrics.ToImmutableArray();
        }
    }

    public async Task ClearMetricsAsync()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            return;
        }

        var syncLockAcquired = false;
        try
        {
            await _syncLock.WaitAsync().ConfigureAwait(false);
            syncLockAcquired = true;
            // The metrics collections are bound from the UI, so the mutation is queued on the
            // dispatcher thread. The action is posted rather than awaited so that stopping a
            // cluster never blocks the caller on a dispatcher that might not be pumping.
            dispatcher.Post(() =>
            {
                podMetrics.Clear();
                nodeMetrics.Clear();
            });
            Volatile.Write(ref _podMetricsByResource, ImmutableDictionary.Create<string, ImmutableArray<PodMetrics>>(StringComparer.Ordinal));
            Volatile.Write(ref _nodeMetricsByResource, ImmutableDictionary.Create<string, ImmutableArray<NodeMetrics>>(StringComparer.Ordinal));
            lock (_snapshotLock)
            {
                _allPodMetrics = ImmutableArray<PodMetrics>.Empty;
                _allNodeMetrics = ImmutableArray<NodeMetrics>.Empty;
            }
        }
        catch (ObjectDisposedException)
        {
            // The collector was disposed while the clear was waiting for the lock.
        }
        finally
        {
            if (syncLockAcquired)
            {
                _syncLock.Release();
            }
        }
    }

    public async Task SyncAsync(Cluster cluster, CancellationToken cancellationToken)
    {
        var syncLockAcquired = false;
        try
        {
            await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            syncLockAcquired = true;
            cancellationToken.ThrowIfCancellationRequested();

            var generation = Volatile.Read(ref _generation);
            var nodeList = await GetMetricsAsync(cluster, "/apis/metrics.k8s.io/v1beta1/nodes", CustomSourceGenerationContext.Default.NodeMetricsList, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            var podList = await GetMetricsAsync(cluster, "/apis/metrics.k8s.io/v1beta1/pods", CustomSourceGenerationContext.Default.PodMetricsList, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            if (generation != Volatile.Read(ref _generation))
            {
                // The collector was restarted while the requests were in flight; the results belong to a
                // previous cluster lifetime and must not be applied.
                return;
            }

            // The metrics collections are bound from the UI, so all mutations must be raised on the
            // UI thread. Pruning, appending and index rebuild happen together in a single batch so
            // observers see one consistent update instead of three. The batch is posted rather than
            // awaited so that a sync never blocks on a dispatcher that might not be pumping.
            dispatcher.Post(() =>
            {
                PruneExpired(nodeMetrics, static metric => metric.Timestamp);
                PruneExpired(podMetrics, static metric => metric.Timestamp);
                AppendRecent(nodeMetrics, nodeList.Items.OfType<NodeMetrics>(), static metric => metric.Name() ?? string.Empty, static metric => metric.Timestamp);
                AppendRecent(podMetrics, podList.Items.OfType<PodMetrics>(), static metric => string.Concat(metric.Namespace(), "/", metric.Name()), static metric => metric.Timestamp);
                RebuildSnapshotIndexes();
            });
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { logger.LogError(ex, "Error updating Kubernetes metrics"); }
        finally
        {
            if (syncLockAcquired)
            {
                _syncLock.Release();
            }
        }
    }

    private async Task<bool> CanUseMetricsServerAsync(Cluster cluster, k8s.Kubernetes kube, CancellationToken cancellationToken)
    {
        static V1SelfSubjectAccessReview CreateReview(string resource) => new()
        {
            ApiVersion = V1SelfSubjectAccessReview.KubeGroup + "/" + V1SelfSubjectAccessReview.KubeApiVersion,
            Kind = V1SelfSubjectAccessReview.KubeKind,
            Spec = new() { ResourceAttributes = new() { Group = "metrics.k8s.io", Resource = resource, Verb = "list" } },
        };
        var podResponse = await kube.CreateSelfSubjectAccessReviewAsync(CreateReview("pods"), cancellationToken: cancellationToken).ConfigureAwait(false);
        var nodeResponse = await kube.CreateSelfSubjectAccessReviewAsync(CreateReview("nodes"), cancellationToken: cancellationToken).ConfigureAwait(false);
        var apiGroups = await cluster.Client!.Apis.GetAPIVersionsAsync(cancellationToken).ConfigureAwait(false);
        var available = apiGroups.Groups.Any(static group => group.Name == "metrics.k8s.io")
            && podResponse.Status.Allowed && nodeResponse.Status.Allowed;
        logger.LogDebug("Kubernetes Metrics Server detection for cluster {Name}: result={Result}.", cluster.Name, available);
        return available;
    }

    private static async Task<TList> GetMetricsAsync<TList>(Cluster cluster, string path, System.Text.Json.Serialization.Metadata.JsonTypeInfo<TList> typeInfo, CancellationToken cancellationToken)
    {
        var kube = (k8s.Kubernetes)cluster.Client!;
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(kube.BaseUri, path));
        using var response = await kube.SendAuthenticatedAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync(typeInfo, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Kubernetes metrics API returned an empty response for '{path}'.");
    }

    private static void PruneExpired<TMetric>(ObservableCollection<TMetric> metrics, Func<TMetric, DateTime?> getTimestamp)
    {
        var cutoff = DateTime.UtcNow - s_retention;
        for (var i = metrics.Count - 1; i >= 0; i--)
        {
            var timestamp = getTimestamp(metrics[i]);
            if (!timestamp.HasValue || timestamp.Value.ToUniversalTime() < cutoff)
                metrics.RemoveAt(i);
        }
    }

    private static void AppendRecent<TMetric>(ObservableCollection<TMetric> stored, IEnumerable<TMetric> received, Func<TMetric, string> getKey, Func<TMetric, DateTime?> getTimestamp)
    {
        var cutoff = DateTime.UtcNow - s_retention;
        HashSet<(string Key, DateTime Timestamp)> known = [];
        for (var i = stored.Count - 1; i >= 0; i--)
        {
            var metric = stored[i];
            if (!known.Add((getKey(metric), getTimestamp(metric)!.Value.ToUniversalTime())))
                stored.RemoveAt(i);
        }
        foreach (var metric in received)
        {
            var timestamp = getTimestamp(metric);
            if (timestamp.HasValue && timestamp.Value.ToUniversalTime() >= cutoff && known.Add((getKey(metric), timestamp.Value.ToUniversalTime())))
                stored.Add(metric);
        }
    }
}
