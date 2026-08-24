using System.IO;
using System.Text;
using k8s.Models;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

/// <summary>Describes a container that can be selected for pod logs.</summary>
public sealed record PodLogContainerOption(string Name, string DisplayName, bool IsInitContainer, bool IsEphemeralContainer = false);

/// <summary>Describes a pod entry in the pod log selector.</summary>
public sealed record PodLogPodSelectionItem(V1Pod? Pod, string DisplayName, bool IsAll);

/// <summary>Describes a container entry in the pod log selector.</summary>
public sealed record PodLogContainerSelectionItem(string Name, string DisplayName, bool IsInitContainer, bool IsAll, bool IsEphemeralContainer = false);

internal readonly record struct PodLogOutputEntry(string PodName, string ContainerName, string Message);

internal readonly record struct PodLogContainerSelectionKey(string Name, bool IsInitContainer, bool IsEphemeralContainer);

internal enum PodLogDisplayMode
{
    None,
    Container,
    PodAndContainer,
}

internal enum PodLogSelectionNormalization
{
    None,
    SelectAll,
    RemoveAll,
}

internal static class PodLogFileNameExtensions
{
    public static string ReplaceInvalidFileNameChars(this string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        StringBuilder builder = new(value.Length);
        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];
            builder.Append(Array.IndexOf(invalidChars, character) >= 0 ? '_' : character);
        }

        return builder.ToString();
    }

    internal static V1OwnerReference? GetControllerReference(V1Pod pod)
    {
        var ownerReferences = pod.Metadata?.OwnerReferences;
        if (ownerReferences is null)
        {
            return null;
        }

        for (var i = 0; i < ownerReferences.Count; i++)
        {
            var ownerReference = ownerReferences[i];
            if (ownerReference.Controller == true)
            {
                return ownerReference;
            }
        }

        return ownerReferences.Count > 0 ? ownerReferences[0] : null;
    }
}
