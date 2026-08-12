using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Kubernetes.Resources.Relationships;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Visualization;

public sealed class VisualizationViewModelTests
{
    [Fact]
    public void selecting_another_namespace_removes_resources_from_previous_selection()
    {
        V1Pod podA = Pod("namespace-a", "pod-a");
        V1Pod podB = Pod("namespace-b", "pod-b");
        ResourceRelationshipGraph graph = Graph(podA, podB);

        ResourceRelationshipGraph selectedA = VisualizationViewModel.FilterToSelectedNamespaces(
            graph,
            new HashSet<string>(["namespace-a"], StringComparer.Ordinal));
        ResourceRelationshipGraph selectedB = VisualizationViewModel.FilterToSelectedNamespaces(
            graph,
            new HashSet<string>(["namespace-b"], StringComparer.Ordinal));

        selectedA.Resources.Select(resource => resource.Name()).ShouldBe(["pod-a"]);
        selectedB.Resources.Select(resource => resource.Name()).ShouldBe(["pod-b"]);
    }

    [Fact]
    public void clearing_namespace_selection_returns_empty_graph()
    {
        ResourceRelationshipGraph graph = Graph(Pod("namespace-a", "pod-a"));

        ResourceRelationshipGraph selected = VisualizationViewModel.FilterToSelectedNamespaces(
            graph,
            new HashSet<string>(StringComparer.Ordinal));

        selected.Resources.ShouldBeEmpty();
        selected.Relationships.ShouldBeEmpty();
    }

    [Fact]
    public void selecting_namespace_preserves_related_cluster_scoped_resource()
    {
        V1Pod pod = Pod("namespace-a", "pod-a");
        V1Node node = new()
        {
            ApiVersion = "v1",
            Kind = "Node",
            Metadata = new V1ObjectMeta { Name = "node-a", Uid = "node-uid" },
        };
        ResourceIdentity podIdentity = Identity(pod);
        ResourceIdentity nodeIdentity = Identity(node);
        ResourceRelationshipGraph graph = new(
            [pod, node],
            [new ResourceRelationship(nodeIdentity, podIdentity, ResourceRelationshipKind.Reference)]);

        ResourceRelationshipGraph selected = VisualizationViewModel.FilterToSelectedNamespaces(
            graph,
            new HashSet<string>(["namespace-a"], StringComparer.Ordinal));

        selected.Resources.Select(resource => resource.Name()).ShouldBe(["pod-a", "node-a"]);
        selected.Relationships.Count.ShouldBe(1);
    }

    private static ResourceRelationshipGraph Graph(params V1Pod[] pods)
        => new(pods, []);

    private static V1Pod Pod(string namespaceName, string name)
        => new()
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = namespaceName,
                Name = name,
                Uid = $"uid-{name}",
            },
        };

    private static ResourceIdentity Identity(IKubernetesObject<V1ObjectMeta> resource)
        => new(
            resource.ApiVersion!,
            resource.Kind!,
            resource.Namespace(),
            resource.Name()!,
            resource.Uid());
}

