using k8s;
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

        var currentPod = await Client.CoreV1.ReadNamespacedPodAsync(podName, namespaceName).ConfigureAwait(false);
        var updatedPod = PodEphemeralContainerBuilder.WithDebugContainer(currentPod, targetContainerName, image);
        var patch = new V1Patch(
            KubernetesJson.Serialize(new
            {
                spec = new
                {
                    ephemeralContainers = updatedPod.Spec!.EphemeralContainers,
                },
            }),
            V1Patch.PatchType.MergePatch);

        await Client.CoreV1.PatchNamespacedPodEphemeralcontainersWithHttpMessagesAsync(
            patch,
            podName,
            namespaceName).ConfigureAwait(false);
    }
}
