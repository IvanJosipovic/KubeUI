using KubeUI.Avalonia.Features.Resources.List;
using KubeUI.Documentation.Tests.Infra;
using k8s.Models;

namespace KubeUI.Documentation.Tests.Features.Resources.List;

public sealed class RecordingTests
{
    [DocumentationVideoTheory]
    public Task Browse_pods_video(string theme)
    {
        return KubeUIWalkthrough.Create(theme, "browse-pods")
            .StartAt<ResourceListView, ResourceListViewModel<V1Pod>>()
            .Speak("The Pods page lists workloads running in this cluster. Ill select the web pod to highlight its row.")
            .SelectPod("web-7c9f8d6f54-2k4m8")
            .RecordAsync();
    }
}
