using Helm.NET;

namespace KubeUI.Kubernetes;

/// <summary>
/// Creates <see cref="IHelmClient"/> instances bound to a Kubernetes connection.
/// </summary>
public interface IHelmClientFactory
{
    /// <summary>
    /// Creates a Helm client for the given connection.
    /// </summary>
    /// <param name="connection">The Kubernetes connection the client operates against.</param>
    /// <returns>A client ready for Helm operations.</returns>
    IHelmClient Create(HelmConnection connection);
}
