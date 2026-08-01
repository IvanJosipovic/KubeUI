using k8s.Models;

namespace KubeUI.Kubernetes;

public partial class Cluster
{
    public async Task AddPodEphemeralDebugContainer(V1Pod pod, string? targetContainerName, string image)
    {
        ArgumentNullException.ThrowIfNull(pod);

        if (Client == null)
        {
            throw new InvalidOperationException("Cluster client is not connected.");
        }

        var namespaceName = pod.Namespace();
        var podName = pod.Name();
        if (string.IsNullOrWhiteSpace(namespaceName) || string.IsNullOrWhiteSpace(podName))
        {
            throw new InvalidOperationException("Pod must have a name and namespace.");
        }

        var updatedPod = PodEphemeralContainerBuilder.WithDebugContainer(pod, targetContainerName, image);

        await Client.CoreV1.ReplaceNamespacedPodEphemeralcontainersWithHttpMessagesAsync(updatedPod, podName, namespaceName).ConfigureAwait(false);
    }
}
