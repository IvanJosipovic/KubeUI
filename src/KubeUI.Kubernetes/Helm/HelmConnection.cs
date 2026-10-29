namespace KubeUI.Kubernetes;

/// <summary>
/// Describes the Kubernetes connection a Helm operation runs against.
/// </summary>
/// <param name="KubeConfigPath">Path to the kubeconfig file, or <see langword="null"/> when Helm's default kubeconfig is used.</param>
/// <param name="Context">Kubernetes context name, or <see langword="null"/> when Helm's current context is used.</param>
/// <param name="Namespace">Default namespace for Helm operations, or <see langword="null"/> to use Helm's default namespace.</param>
public sealed record HelmConnection(string? KubeConfigPath, string? Context, string? Namespace);
