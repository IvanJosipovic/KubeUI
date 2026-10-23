namespace KubeUI.Kubernetes;

/// <summary>
/// Provides the kubeconfig path used for local import and discovery.
/// </summary>
public interface IKubeConfigPathProvider
{
    /// <summary>
    /// Gets the default kubeconfig file path.
    /// </summary>
    string DefaultPath { get; }

    /// <summary>
    /// Gets the directory scanned for additional kubeconfig files, or <see langword="null"/> when discovery is not available.
    /// </summary>
    string? DiscoveryDirectory { get; }
}
