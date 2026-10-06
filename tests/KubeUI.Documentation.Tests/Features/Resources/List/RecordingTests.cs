using Avalonia.VisualTree;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Documentation.Tests.Infra;

namespace KubeUI.Documentation.Tests.Features.Resources.List;

public sealed class RecordingTests
{
    [DocumentationVideoTheory]
    [Obsolete]
    public Task Browse_pods_video(string theme)
    {
        return KubeUIWalkthrough.Create(theme, "browse-pods")
            .Intro(
                "Browse Kubernetes resources",
                "I'll open Pods, find a workload, and inspect its manifest.")
            .FakeCluster("demo-cluster")
            .LoadView<MainView, MainViewModel>(
                x => x.ViewModel.Initialize(),
                connectToCluster: false)
            .Speak("First, I'll connect to the cluster so KubeUI can load its resources.")
            .OpenCluster("demo-cluster")
            .Speak("The cluster is connected. Now I'll open Pods to browse the workloads.")
            .SelectNavigation("Pods")
            .Speak("The Pods are sorted by name. I'll filter the list for the web workload.")
            .SearchPods(WalkthroughDemoResources.FeaturedPodName)
            .Speak("The matching web Pod is ready. I'll select it.")
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
