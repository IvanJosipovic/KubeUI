using Avalonia.VisualTree;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Documentation.Tests.Infra;

namespace KubeUI.Documentation.Tests.Features.Resources.Visualization;

public sealed class RecordingTests
{
    [DocumentationVideoTheory]
    [Obsolete]
    public Task Explore_resource_relationships_video(string theme)
    {
        return KubeUIWalkthrough.Create(theme, "resource-relationships")
            .Intro(
                "Visualize resource relationships",
                "I'll follow the web workload from Deployment to ReplicaSet to Pod.")
            .LoadView<VisualizationView, VisualizationViewModel>(
                x => x.ViewModel.Initialize(x.Workspace, x.DemoResources.DefaultNamespace))
            .Speak(
                "The default namespace graph is ready. I'll zoom into the web Deployment and follow its child resources.",
                root => root is VisualizationView view
                    && view.ViewModel.SelectedNamespaces.Any(
                        namespaceResource => namespaceResource.Metadata?.Name == "default")
                    && view.ViewModel.Graph is { } graph
                    && graph.Resources.Count(resource =>
                        resource is V1Pod pod && pod.Metadata?.NamespaceProperty == "default") >= 10
                    && graph.Resources.Any(resource =>
                        resource is V1Deployment deployment && deployment.Metadata?.Name == "web")
                    && graph.Resources.Any(resource =>
                        resource is V1ReplicaSet replicaSet && replicaSet.Metadata?.Name == "web-7c9f8d6f54")
                    && graph.Resources.Any(resource =>
                        resource is V1Pod pod && pod.Metadata?.Name == WalkthroughDemoResources.FeaturedPodName)
                    && view.GetVisualDescendants().OfType<ResourceGraphControl>().Any(
                        graphControl => graphControl.IsViewportStable
                            && graphControl.Area.VertexList.Count == graph.Resources.Count))
            .DelayBeforeNextAction(TimeSpan.FromSeconds(2))
            .ZoomRelationshipTo(V1Deployment.KubeKind, "web")
            .Speak("The Deployment manages a ReplicaSet below it. I'll pan down to inspect that ReplicaSet.")
            .PanRelationshipTo(V1ReplicaSet.KubeKind, "web-7c9f8d6f54")
            .Speak("The ReplicaSet manages the web Pods. I'll pan down again to inspect one Pod.")
            .PanRelationshipTo(V1Pod.KubeKind, WalkthroughDemoResources.FeaturedPodName)
            .Speak("This follows the ownership chain from the Deployment through its ReplicaSet to a Pod.")
            .RecordAsync();
    }
}
