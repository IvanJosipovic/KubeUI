using Helm.NET;

namespace KubeUI.Kubernetes;

/// <summary>
/// Creates <see cref="HelmClient"/> instances from a <see cref="HelmConnection"/>.
/// </summary>
public sealed class HelmClientFactory : IHelmClientFactory
{
    /// <inheritdoc />
    public IHelmClient Create(HelmConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        return new HelmClient(CreateOptions(connection));
    }

    /// <summary>
    /// Maps a connection to Helm client options, normalizing empty values to Helm's defaults.
    /// </summary>
    /// <param name="connection">The Kubernetes connection to map.</param>
    /// <returns>The Helm client options for the connection.</returns>
    internal static HelmClientOptions CreateOptions(HelmConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return new HelmClientOptions
        {
            Kubeconfig = string.IsNullOrWhiteSpace(connection.KubeConfigPath) ? null : connection.KubeConfigPath,
            KubeContext = string.IsNullOrWhiteSpace(connection.Context) ? null : connection.Context,
            DefaultNamespace = string.IsNullOrWhiteSpace(connection.Namespace) ? "default" : connection.Namespace,
        };
    }
}
