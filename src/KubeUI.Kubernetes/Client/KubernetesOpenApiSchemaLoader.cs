using System.Diagnostics;
using System.Text.Json;
using KubeUI.Kubernetes.Client;

namespace KubeUI.Kubernetes;

/// <summary>
/// Coordinates one-time, retryable OpenAPI schema loading for a cluster.
/// </summary>
public sealed class KubernetesOpenApiSchemaLoader : IDisposable
{
    private readonly KubernetesOpenApiSchemaCatalog _catalog;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private int _loadAttempted;
    private int _disposed;

    public KubernetesOpenApiSchemaLoader(
        KubernetesOpenApiSchemaCatalog catalog,
        ILogger logger)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task EnsureAsync(
        k8s.Kubernetes client,
        string clusterName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

        using var activity = KubeInstrumentation.Source.StartActivity(
            nameof(KubernetesOpenApiSchemaLoader),
            ActivityKind.Internal);
        activity?.SetTag("kubernetes.cluster.name", clusterName);
        activity?.SetTag("kubernetes.openapi.schema.count", _catalog.Count);

        if (Volatile.Read(ref _loadAttempted) != 0 && _catalog.Count > 0)
        {
            activity?.SetTag("kubernetes.openapi.schema.load.skipped", true);
            return;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (Volatile.Read(ref _loadAttempted) != 0)
            {
                activity?.SetTag("kubernetes.openapi.schema.load.skipped", true);
                return;
            }

            const int maxAttempts = 3;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await _catalog.LoadAsync(client, cancellationToken).ConfigureAwait(false);
                    Volatile.Write(ref _loadAttempted, 1);
                    activity?.SetTag("kubernetes.openapi.schema.count", _catalog.Count);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return;
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    _logger.LogDebug(
                        ex,
                        "Unable to load Kubernetes OpenAPI v3 schemas for {name} on attempt {attempt} of {maxAttempts}.",
                        clusterName,
                        attempt,
                        maxAttempts);
                }
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Reset()
    {
        Volatile.Write(ref _loadAttempted, 0);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            _gate.Dispose();
        }
    }
}
