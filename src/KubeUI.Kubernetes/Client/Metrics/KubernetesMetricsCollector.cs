using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes;

internal sealed class KubernetesMetricsCollector(
    ILogger logger,
    ObservableCollection<PodMetrics> podMetrics,
    ObservableCollection<NodeMetrics> nodeMetrics) : IDisposable
{
    private static readonly TimeSpan s_retention = TimeSpan.FromHours(1);
    private CancellationTokenSource? _cancellation;
    private PeriodicTimer? _timer;
    private Task? _refreshTask;

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
        if (_refreshTask is { } task)
        {
            try { await task.ConfigureAwait(false); }
            catch (OperationCanceledException) when (cancellation?.IsCancellationRequested == true) { }
            finally { _refreshTask = null; cancellation?.Dispose(); }
        }
        else
        {
            cancellation?.Dispose();
        }
    }

    public void Dispose() => StopAsync().GetAwaiter().GetResult();

    public async Task SyncAsync(Cluster cluster, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            PruneExpired(nodeMetrics, static metric => metric.Timestamp);
            PruneExpired(podMetrics, static metric => metric.Timestamp);
            var nodeList = await GetMetricsAsync(cluster, "/apis/metrics.k8s.io/v1beta1/nodes", CustomSourceGenerationContext.Default.NodeMetricsList, cancellationToken).ConfigureAwait(false);
            AppendRecent(nodeMetrics, nodeList.Items.OfType<NodeMetrics>(), static metric => metric.Name() ?? string.Empty, static metric => metric.Timestamp);
            cancellationToken.ThrowIfCancellationRequested();
            var podList = await GetMetricsAsync(cluster, "/apis/metrics.k8s.io/v1beta1/pods", CustomSourceGenerationContext.Default.PodMetricsList, cancellationToken).ConfigureAwait(false);
            AppendRecent(podMetrics, podList.Items.OfType<PodMetrics>(), static metric => string.Concat(metric.Namespace(), "/", metric.Name()), static metric => metric.Timestamp);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { logger.LogError(ex, "Error updating Kubernetes metrics"); }
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
            if (!timestamp.HasValue || timestamp.Value.ToUniversalTime() < cutoff) metrics.RemoveAt(i);
        }
    }

    private static void AppendRecent<TMetric>(ObservableCollection<TMetric> stored, IEnumerable<TMetric> received, Func<TMetric, string> getKey, Func<TMetric, DateTime?> getTimestamp)
    {
        var cutoff = DateTime.UtcNow - s_retention;
        HashSet<(string Key, DateTime Timestamp)> known = [];
        for (var i = stored.Count - 1; i >= 0; i--)
        {
            var metric = stored[i];
            if (!known.Add((getKey(metric), getTimestamp(metric)!.Value.ToUniversalTime()))) stored.RemoveAt(i);
        }
        foreach (var metric in received)
        {
            var timestamp = getTimestamp(metric);
            if (timestamp.HasValue && timestamp.Value.ToUniversalTime() >= cutoff && known.Add((getKey(metric), timestamp.Value.ToUniversalTime()))) stored.Add(metric);
        }
    }
}
