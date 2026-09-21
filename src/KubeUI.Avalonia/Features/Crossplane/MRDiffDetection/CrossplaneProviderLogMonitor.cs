using System.Collections.Concurrent;
using System.Text;
using k8s;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;

public sealed class CrossplaneProviderLogMonitor : IDisposable
{
    private readonly IPodLogSessionResolver _resolver;
    private readonly ILogger<CrossplaneProviderLogMonitor> _logger;
    private readonly object _gate = new();
    private CancellationTokenSource? _cancellation;
    private bool _disposed;

    public CrossplaneProviderLogMonitor(
        IPodLogSessionResolver resolver,
        ILogger<CrossplaneProviderLogMonitor> logger)
    {
        _resolver = resolver;
        _logger = logger;
    }

    public async Task StartAsync(
        IClusterRuntime cluster,
        GenericKubernetesObject provider,
        Action<string> lineReceived,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(lineReceived);

        CancellationTokenSource localCancellation;
        lock (_gate)
        {
            _cancellation?.Cancel();
            _cancellation?.Dispose();
            _cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            localCancellation = _cancellation;
        }

        var state = _resolver.CreateState(provider, string.Empty, previous: false, timestamps: true, tailLines: int.MaxValue);
        var resolution = _resolver.TryResolve(cluster, state);
        var pods = resolution?.RelatedPods ?? [];
        if (pods.Count == 0)
        {
            pods = FindProviderPods(cluster, provider.Metadata?.Name);
        }
        var tasks = new List<Task>();
        var seen = new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);
        foreach (var pod in pods)
        {
            foreach (var container in pod.Spec?.Containers ?? [])
            {
                tasks.Add(ReadContainerAsync(cluster, pod, container.Name, previous: false, lineReceived, seen, localCancellation.Token));
                if (HasRestartedContainer(pod, container.Name))
                {
                    tasks.Add(ReadContainerAsync(cluster, pod, container.Name, previous: true, lineReceived, seen, localCancellation.Token));
                }
            }
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task ReadContainerAsync(
        IClusterRuntime cluster,
        k8s.Models.V1Pod pod,
        string containerName,
        bool previous,
        Action<string> lineReceived,
        ConcurrentDictionary<string, byte> seen,
        CancellationToken cancellationToken)
    {
        if (cluster.Client is null)
        {
            return;
        }

        try
        {
            await using var stream = await cluster.Client.CoreV1.ReadNamespacedPodLogAsync(
                pod.Metadata?.Name ?? string.Empty,
                pod.Metadata?.NamespaceProperty ?? string.Empty,
                container: containerName,
                follow: !previous,
                previous: previous,
                timestamps: true,
                tailLines: null,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
            while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
            {
                var key = $"{pod.Metadata?.NamespaceProperty}\u0000{pod.Metadata?.Name}\u0000{containerName}\u0000{line}";
                if (seen.TryAdd(key, 0))
                {
                    lineReceived(line);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Crossplane provider log stream ended for {Pod}/{Container}", pod.Metadata?.Name, containerName);
        }
    }

    private static bool HasRestartedContainer(k8s.Models.V1Pod pod, string containerName)
    {
        return (pod.Status?.ContainerStatuses ?? []).Any(status =>
            string.Equals(status.Name, containerName, StringComparison.Ordinal) && status.RestartCount > 0);
    }

    private static List<k8s.Models.V1Pod> FindProviderPods(IClusterRuntime cluster, string? providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return [];
        }

        List<k8s.Models.V1Pod> pods = [];
        foreach (var pair in cluster.Objects)
        {
            if (pair.Value is not IResourceContainer container)
            {
                continue;
            }

            foreach (var pod in container.Snapshot().OfType<k8s.Models.V1Pod>())
            {
                var labels = pod.Metadata?.Labels;
                var hasProviderLabel = labels is not null
                    && labels.Any(label =>
                        (string.Equals(label.Key, "pkg.crossplane.io/provider", StringComparison.Ordinal)
                            || string.Equals(label.Key, "pkg.crossplane.io/provider-name", StringComparison.Ordinal))
                        && string.Equals(label.Value, providerName, StringComparison.Ordinal));
                var hasProviderName = pod.Metadata?.Name?.StartsWith(providerName + "-", StringComparison.Ordinal) == true;

                if (hasProviderLabel || hasProviderName)
                {
                    pods.Add(pod);
                }
            }
        }

        return pods
            .GroupBy(pod => $"{pod.Metadata?.NamespaceProperty}\u0000{pod.Metadata?.Name}", StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _cancellation?.Cancel();
            _cancellation?.Dispose();
            _cancellation = null;
        }
    }
}
