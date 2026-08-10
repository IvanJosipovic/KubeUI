using k8s;

namespace KubeUI.Testing.Kubernetes.Scenarios;

internal static class KubernetesClientMaterializer
{
    public static k8s.Kubernetes Create(
        KubernetesClientConfiguration configuration,
        DelegatingHandler? handler = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return handler is null
            ? new k8s.Kubernetes(configuration)
            : new k8s.Kubernetes(configuration, handler);
    }
}
