using System.IO;
using System.Text;
using k8s.Models;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

public sealed record PodLogContainerOption(string Name, string DisplayName, bool IsInitContainer);

public sealed record PodLogPodSelectionItem(V1Pod? Pod, string DisplayName, bool IsAll);

public sealed record PodLogContainerSelectionItem(string Name, string DisplayName, bool IsInitContainer, bool IsAll);

internal readonly record struct PodLogOutputEntry(string PodName, string ContainerName, string Message);

internal readonly record struct PodLogContainerSelectionKey(string Name, bool IsInitContainer);

internal enum PodLogDisplayMode
{
    None,
    Container,
    PodAndContainer,
}

internal static class PodLogFileNameExtensions
{
    public static string ReplaceInvalidFileNameChars(this string value)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        StringBuilder builder = new(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            char character = value[i];
            builder.Append(Array.IndexOf(invalidChars, character) >= 0 ? '_' : character);
        }

        return builder.ToString();
    }

    internal static V1OwnerReference? GetControllerReference(V1Pod pod)
    {
        IList<V1OwnerReference>? ownerReferences = pod.Metadata?.OwnerReferences;
        if (ownerReferences is null)
        {
            return null;
        }

        for (int i = 0; i < ownerReferences.Count; i++)
        {
            V1OwnerReference ownerReference = ownerReferences[i];
            if (ownerReference.Controller == true)
            {
                return ownerReference;
            }
        }

        return ownerReferences.Count > 0 ? ownerReferences[0] : null;
    }
}

