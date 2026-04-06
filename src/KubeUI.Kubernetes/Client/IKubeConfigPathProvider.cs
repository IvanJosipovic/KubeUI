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
}
