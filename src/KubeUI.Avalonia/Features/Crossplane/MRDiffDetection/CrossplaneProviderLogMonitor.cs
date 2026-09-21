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

    public static IReadOnlyList<CrossplaneProviderOption> GetProviders(IClusterRuntime cluster)
    {
        List<CrossplaneProviderOption> providers = [];
        foreach (var pair in cluster.Objects)
        {
            if (!string.Equals(pair.Key.Group, "pkg.crossplane.io", StringComparison.Ordinal)
                || !string.Equals(pair.Key.Kind, "Provider", StringComparison.Ordinal)
                || pair.Value is not IResourceContainer container)
            {
                continue;
            }

            foreach (var resource in container.Snapshot().OfType<GenericKubernetesObject>())
            {
                if (!string.IsNullOrWhiteSpace(resource.Metadata?.Name))
                {
                    providers.Add(new CrossplaneProviderOption(resource.Metadata!.Name!, resource));
                }
            }
        }

        return providers.OrderBy(provider => provider.Name, StringComparer.Ordinal).ToArray();
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
