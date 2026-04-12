using k8s.Models;

namespace KubeUI.Kubernetes;

internal static class PodEphemeralContainerBuilder
{
    internal static V1Pod WithDebugContainer(V1Pod pod, string? targetContainerName, string image)
    {
        ArgumentNullException.ThrowIfNull(pod);

        if (pod.Spec == null)
        {
            throw new InvalidOperationException("Pod spec is required to add an ephemeral debug container.");
        }

        if (string.IsNullOrWhiteSpace(pod.Metadata?.Name) || string.IsNullOrWhiteSpace(pod.Metadata.NamespaceProperty))
        {
            throw new InvalidOperationException("Pod metadata must include a name and namespace.");
        }

        if (string.IsNullOrWhiteSpace(image))
        {
            throw new ArgumentException("Debug container image must be provided.", nameof(image));
        }

        string? resolvedTargetContainerName = string.IsNullOrWhiteSpace(targetContainerName) ? null : targetContainerName;
        if (resolvedTargetContainerName != null && !ContainerExists(pod, resolvedTargetContainerName))
        {
            throw new InvalidOperationException($"Container '{resolvedTargetContainerName}' was not found in pod '{pod.Namespace()}/{pod.Name()}'.");
        }

        List<V1EphemeralContainer> ephemeralContainers = pod.Spec.EphemeralContainers?.ToList() ?? new List<V1EphemeralContainer>();
        ephemeralContainers.Add(CreateDebugContainer(image, resolvedTargetContainerName));

        return pod with
        {
            Spec = pod.Spec with
            {
                EphemeralContainers = ephemeralContainers,
            },
        };
    }

    private static bool ContainerExists(V1Pod pod, string containerName)
    {
        foreach (var container in pod.Spec?.Containers ?? Array.Empty<V1Container>())
        {
            if (string.Equals(container.Name, containerName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        foreach (var container in pod.Spec?.InitContainers ?? Array.Empty<V1Container>())
        {
            if (string.Equals(container.Name, containerName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static V1EphemeralContainer CreateDebugContainer(string image, string? targetContainerName)
    {
        V1EphemeralContainer container = new()
        {
            Name = $"debug-{Guid.NewGuid():N}"[..13],
            Image = image,
            Command = ["sh"],
            Stdin = true,
            StdinOnce = true,
            Tty = true,
        };

        if (!string.IsNullOrWhiteSpace(targetContainerName))
        {
            container.TargetContainerName = targetContainerName;
        }

        return container;
    }
}
