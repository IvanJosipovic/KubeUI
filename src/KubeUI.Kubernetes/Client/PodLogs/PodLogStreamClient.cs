using System.IO;
using k8s;

namespace KubeUI.Kubernetes;

/// <summary>
/// Opens pod log streams against the current cluster.
/// </summary>
public interface IPodLogStreamClient
{
    /// <summary>
    /// Opens a streaming log request for the resolved pod session.
    /// </summary>
    /// <param name="cluster">The cluster runtime to query.</param>
    /// <param name="options">The resolved log request options.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>A stream that yields log lines.</returns>
    Task<Stream> OpenAsync(IClusterRuntime cluster, PodLogReadOptions options, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class PodLogStreamClient : IPodLogStreamClient
{
    /// <inheritdoc />
    public Task<Stream> OpenAsync(IClusterRuntime cluster, PodLogReadOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(options);

        if (cluster.Client is null)
        {
            throw new InvalidOperationException("The cluster client is not available.");
        }

        return cluster.Client.CoreV1.ReadNamespacedPodLogAsync(
            options.PodName,
            options.PodNamespace,
            container: options.ContainerName,
            follow: options.Follow,
            pretty: true,
            previous: options.Previous,
            tailLines: options.TailLines,
            timestamps: options.Timestamps,
            cancellationToken: cancellationToken);
    }
}
