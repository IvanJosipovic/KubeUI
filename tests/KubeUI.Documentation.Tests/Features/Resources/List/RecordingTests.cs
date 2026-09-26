using Avalonia.VisualTree;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Documentation.Tests.Infra;

namespace KubeUI.Documentation.Tests.Features.Resources.List;

public sealed class RecordingTests
{
    [DocumentationVideoTheory]
    public Task Browse_pods_video(string theme)
    {
        return KubeUIWalkthrough.Create(theme, "browse-pods")
            .Intro(
                "Browse Kubernetes resources",
                "Let's explore the Pods in this cluster and choose one to inspect.")
            .FakeCluster("demo-cluster")
            .LoadView<MainView, MainViewModel>(
                x => x.ViewModel.Initialize(),
                connectToCluster: false)
            .Speak("I'll open Pods to see the workloads in this cluster.")
            .OpenCluster("demo-cluster")
            .Speak("The Pods list is ready. I'll browse the workloads by name.")
            .SelectNavigation("Pods")
            .Speak("The Pods are sorted by name. I'll select the featured web Pod.")
            .SelectPod("web-7c9f8d6f54-2k4m8")
            .Speak("I'll right-click the selected Pod and choose View YAML to inspect its manifest.")
            .RightClickPod("web-7c9f8d6f54-2k4m8")
            .SelectContextMenuItem(
                "View YAML",
                root => root.GetVisualDescendants().OfType<ResourceYamlView>().Any(view => view.IsVisible))
            .Speak(
                "The selected Pod's manifest is open in the YAML viewer.",
                root => root.GetVisualDescendants().OfType<ResourceYamlView>().Any(view => view.IsVisible))
            .RecordAsync();
    }
}
