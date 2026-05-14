using k8s;

namespace KubeUI.Kubernetes;

/// <summary>
/// Uses the Kubernetes client library's default kubeconfig location.
/// </summary>
public sealed class DefaultKubeConfigPathProvider : IKubeConfigPathProvider
{
    /// <inheritdoc />
    public string DefaultPath => KubernetesClientConfiguration.KubeConfigDefaultLocation;
}
