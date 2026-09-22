using k8s;
using k8s.KubeConfigModels;

namespace KubeUI.Testing.Kubernetes.Scenarios;

internal static class LiveKubernetesConfiguration
{
    public static KubernetesClientConfiguration CreateClientConfiguration(
        K8SConfiguration kubeConfig,
        string contextName)
    {
        ArgumentNullException.ThrowIfNull(kubeConfig);
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);

        if (kubeConfig.Contexts.All(context => !string.Equals(context.Name, contextName, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"The default host kubeconfig does not contain a context named '{contextName}'.");
        }

        return KubernetesClientConfiguration.BuildConfigFromConfigObject(
            kubeConfig,
            contextName,
            masterUrl: null);
    }
}
