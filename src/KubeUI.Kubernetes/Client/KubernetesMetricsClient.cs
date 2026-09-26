using System.Text.Json;
using k8s;
using k8s.Models;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Kubernetes;

internal static class KubernetesMetricsClient
{
    private const string MetricsApiGroup = "metrics.k8s.io";
    private const string MetricsApiVersion = "v1beta1";

    public static async Task<NodeMetricsList> GetKubernetesNodesMetricsAsync(
        IKubernetes kubernetes,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(kubernetes);

        var response = (JsonElement)await kubernetes.CustomObjects.GetClusterCustomObjectAsync(
            MetricsApiGroup,
            MetricsApiVersion,
            "nodes",
            string.Empty,
            cancellationToken).ConfigureAwait(false);

        return response.Deserialize(KubernetesJsonStaticContext.Default.NodeMetricsList)
            ?? throw new JsonException("Node metrics response was null.");
    }

    public static async Task<PodMetricsList> GetKubernetesPodsMetricsAsync(
        IKubernetes kubernetes,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(kubernetes);

        var response = (JsonElement)await kubernetes.CustomObjects.GetClusterCustomObjectAsync(
            MetricsApiGroup,
            MetricsApiVersion,
            "pods",
            string.Empty,
            cancellationToken).ConfigureAwait(false);

        return response.Deserialize(KubernetesJsonStaticContext.Default.PodMetricsList)
            ?? throw new JsonException("Pod metrics response was null.");
    }
}
