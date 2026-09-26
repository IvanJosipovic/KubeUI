using Avalonia.VisualTree;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Documentation.Tests.Infra;

namespace KubeUI.Documentation.Tests.Features.Resources.Visualization;

public sealed class RecordingTests
{
    [DocumentationVideoTheory]
    public Task Explore_resource_relationships_video(string theme)
    {
        return KubeUIWalkthrough.Create(theme, "resource-relationships")
            .StartAt<VisualizationView, VisualizationViewModel>(
                x => x.ViewModel.Initialize(x.Workspace, x.DemoResources.DefaultNamespace))
            .Speak(
                "With the default namespace selected, the graph shows its resources. Select a resource to trace the pod back to its owning workload and related Kubernetes objects.",
                root => root is VisualizationView view
                    && view.ViewModel.SelectedNamespaces.Any(
                        namespaceResource => namespaceResource.Metadata?.Name == "default")
                    && view.ViewModel.Graph is { } graph
                    && graph.Resources.Count(resource =>
                        resource is V1Pod pod && pod.Metadata?.NamespaceProperty == "default") == 10
                    && view.GetVisualDescendants().OfType<ResourceGraphControl>().Any(
                        graphControl => graphControl.IsViewportStable
                            && graphControl.Area.VertexList.Count == graph.Resources.Count))
            .DelayBeforeNextAction(TimeSpan.FromSeconds(2))
            .ClickRelationshipSurface()
            .RecordAsync();
    }
}
