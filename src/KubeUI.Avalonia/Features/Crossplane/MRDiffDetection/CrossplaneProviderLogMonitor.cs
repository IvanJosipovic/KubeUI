using System.Security.Cryptography;
using System.Text;
using k8s;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;

public sealed class CrossplaneProviderLogMonitor : IDisposable
{
    private const int InitialLogTailLines = 500;
    private readonly IPodLogSessionResolver _resolver;
    private readonly IPodLogStreamClient _streamClient;
    private readonly ILogger<CrossplaneProviderLogMonitor> _logger;
    private readonly object _gate = new();
    private readonly RecentLogLineSet _seen = new(4096);
    private CancellationTokenSource? _cancellation;
    private string? _providerName;
    private bool _disposed;

    public CrossplaneProviderLogMonitor(
        IPodLogSessionResolver resolver,
        IPodLogStreamClient streamClient,
        ILogger<CrossplaneProviderLogMonitor> logger)
    {
        _resolver = resolver;
        _streamClient = streamClient;
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
        CancellationTokenSource? previousCancellation;
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var providerName = provider.Metadata?.Name;
            if (!string.Equals(_providerName, providerName, StringComparison.Ordinal))
            {
                _seen.Clear();
                _providerName = providerName;
            }

            previousCancellation = _cancellation;
            _cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            localCancellation = _cancellation;
        }

        previousCancellation?.Cancel();
        try
        {
            var state = _resolver.CreateState(provider, string.Empty, previous: false, timestamps: true, tailLines: InitialLogTailLines);
            var resolution = _resolver.TryResolve(cluster, state);
            var pods = resolution?.RelatedPods ?? [];
            if (pods.Count == 0)
            {
                pods = FindProviderPods(cluster, provider.Metadata?.Name);
            }
            var tasks = new List<Task>();
            foreach (var pod in pods)
            {
                foreach (var container in pod.Spec?.Containers ?? [])
                {
                    tasks.Add(ReadContainerAsync(cluster, pod, container.Name, previous: false, lineReceived, _seen, localCancellation.Token));
                    if (HasRestartedContainer(pod, container.Name))
                    {
                        tasks.Add(ReadContainerAsync(cluster, pod, container.Name, previous: true, lineReceived, _seen, localCancellation.Token));
                    }
                }
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        finally
        {
            lock (_gate)
            {
                if (ReferenceEquals(_cancellation, localCancellation))
                {
                    _cancellation = null;
                }
            }

            localCancellation.Dispose();
        }
    }

    private async Task ReadContainerAsync(
        IClusterRuntime cluster,
        k8s.Models.V1Pod pod,
        string containerName,
        bool previous,
        Action<string> lineReceived,
        RecentLogLineSet seen,
        CancellationToken cancellationToken)
    {
        do
        {
            try
            {
                PodLogReadOptions options = new(
                    pod.Metadata?.NamespaceProperty ?? string.Empty,
                    pod.Metadata?.Name ?? string.Empty,
                    containerName,
                    previous,
                    Timestamps: true,
                    Follow: !previous,
                    TailLines: InitialLogTailLines);
                await using var stream = await _streamClient.OpenAsync(cluster, options, cancellationToken).ConfigureAwait(false);
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
                while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
                {
                    if (!line.Contains("Diff detected", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var lineHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(line)));
                    var key = $"{pod.Metadata?.NamespaceProperty}\u0000{pod.Metadata?.Name}\u0000{containerName}\u0000{lineHash}";
                    if (seen.TryAdd(key))
                    {
                        lineReceived(line);
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Crossplane provider log stream ended for {Pod}/{Container}", pod.Metadata?.Name, containerName);
            }

            if (previous || cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        } while (!cancellationToken.IsCancellationRequested);
    }

    private static bool HasRestartedContainer(k8s.Models.V1Pod pod, string containerName)
    {
        return (pod.Status?.ContainerStatuses ?? []).Any(status =>
            string.Equals(status.Name, containerName, StringComparison.Ordinal) && status.RestartCount > 0);
    }

    public static HashSet<string> GetProviderPodKeys(IClusterRuntime cluster, string providerName)
    {
        return FindProviderPods(cluster, providerName)
            .Select(pod => $"{pod.Metadata?.NamespaceProperty}\u0000{pod.Metadata?.Name}")
            .ToHashSet(StringComparer.Ordinal);
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
                    && labels.Keys.Any(key =>
                        string.Equals(key, "pkg.crossplane.io/provider", StringComparison.Ordinal)
                        || string.Equals(key, "pkg.crossplane.io/provider-name", StringComparison.Ordinal));
                var matchesProviderLabel = labels is not null
                    && labels.Any(label =>
                        (string.Equals(label.Key, "pkg.crossplane.io/provider", StringComparison.Ordinal)
                            || string.Equals(label.Key, "pkg.crossplane.io/provider-name", StringComparison.Ordinal))
                        && string.Equals(label.Value, providerName, StringComparison.Ordinal));
                var hasProviderName = pod.Metadata?.Name?.StartsWith(providerName + "-", StringComparison.Ordinal) == true;

                if (hasProviderLabel ? matchesProviderLabel : hasProviderName)
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
        CancellationTokenSource? cancellation;
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            cancellation = _cancellation;
            _cancellation = null;
        }

        cancellation?.Cancel();
    }

    public void Stop()
    {
        CancellationTokenSource? cancellation;
        lock (_gate)
        {
            cancellation = _cancellation;
            _cancellation = null;
        }

        cancellation?.Cancel();
    }

    private sealed class RecentLogLineSet(int capacity)
    {
        private readonly object _gate = new();
        private readonly HashSet<string> _keys = new(StringComparer.Ordinal);
        private readonly Queue<string> _orderedKeys = new();

        public bool TryAdd(string key)
        {
            lock (_gate)
            {
                if (!_keys.Add(key))
                {
                    return false;
                }

                _orderedKeys.Enqueue(key);
                if (_orderedKeys.Count > capacity)
                {
                    _keys.Remove(_orderedKeys.Dequeue());
                }

                return true;
            }
        }

        public void Clear()
        {
            lock (_gate)
            {
                _keys.Clear();
                _orderedKeys.Clear();
            }
        }
    }
}
