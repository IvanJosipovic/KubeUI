using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Resources.Relationships;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Visualization;

public sealed class VisualizationPrimitiveTests
{
    [Theory]
    [MemberData(nameof(KnownResourcesWithFalseConditions))]
    public void Known_resource_false_conditions_are_not_ready(IKubernetesObject<V1ObjectMeta> resource)
        => ResourceReadiness.IsNotReady(resource).ShouldBeTrue();

    public static IEnumerable<object[]> KnownResourcesWithFalseConditions()
    {
        const string json = """
            {
              "metadata": { "name": "resource" },
              "status": { "conditions": [{ "status": "False" }] }
            }
            """;

        yield return [KubernetesJson.Deserialize<V1DaemonSet>(json)];
        yield return [KubernetesJson.Deserialize<V1Deployment>(json)];
        yield return [KubernetesJson.Deserialize<V1FlowSchema>(json)];
        yield return [KubernetesJson.Deserialize<V1Job>(json)];
        yield return [KubernetesJson.Deserialize<V1Namespace>(json)];
        yield return [KubernetesJson.Deserialize<V1Node>(json)];
        yield return [KubernetesJson.Deserialize<V1PersistentVolumeClaim>(json)];
        yield return [KubernetesJson.Deserialize<V1Pod>(json)];
        yield return [KubernetesJson.Deserialize<V1PodDisruptionBudget>(json)];
        yield return [KubernetesJson.Deserialize<V1PriorityLevelConfiguration>(json)];
        yield return [KubernetesJson.Deserialize<V1ReplicaSet>(json)];
        yield return [KubernetesJson.Deserialize<V1ReplicationController>(json)];
        yield return [KubernetesJson.Deserialize<V1Service>(json)];
        yield return [KubernetesJson.Deserialize<V1ServiceCIDR>(json)];
        yield return [KubernetesJson.Deserialize<V1StatefulSet>(json)];
        yield return [KubernetesJson.Deserialize<V1ValidatingAdmissionPolicy>(json)];
        yield return [KubernetesJson.Deserialize<V2HorizontalPodAutoscaler>(json)];
        yield return [KubernetesJson.Deserialize<V1APIService>(json)];
        yield return [KubernetesJson.Deserialize<V1CertificateSigningRequest>(json)];
        yield return [KubernetesJson.Deserialize<V1CustomResourceDefinition>(json)];
    }

    [Fact]
    public void Generic_resource_status_without_conditions_is_ready()
    {
        var resources = new[]
        {
            KubernetesJson.Deserialize<GenericKubernetesObject>("""
                { "apiVersion": "example.io/v1", "kind": "Widget", "metadata": { "name": "widget" }, "status": null }
                """),
            KubernetesJson.Deserialize<GenericKubernetesObject>("""
                { "apiVersion": "example.io/v1", "kind": "Widget", "metadata": { "name": "widget" }, "status": { "conditions": {} } }
                """),
            KubernetesJson.Deserialize<GenericKubernetesObject>("""
                { "apiVersion": "example.io/v1", "kind": "Widget", "metadata": { "name": "widget" }, "status": { "conditions": [null, { "status": 1 }, { "status": "True" }] } }
                """),
        };

        foreach (var resource in resources)
        {
            ResourceReadiness.IsNotReady(resource!).ShouldBeFalse();
        }

        ResourceReadiness.IsNotReady(new V1Secret()).ShouldBeFalse();
        ResourceReadiness.IsNotReady(KubernetesJson.Deserialize<GenericKubernetesObject>("""
            { "apiVersion": "example.io/v1", "kind": "Widget", "metadata": { "name": "widget" }, "status": { "conditions": [{ "status": "False" }] } }
            """)!).ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(RelationshipKinds))]
    public void Graph_edge_uses_the_expected_theme_class(ResourceRelationshipKind kind, string themeClass)
    {
        ResourceNodeViewModel sourceNode = Node("source");
        ResourceNodeViewModel targetNode = Node("target");
        ResourceGraphVertex source = Vertex(sourceNode);
        ResourceGraphVertex target = Vertex(targetNode);
        ResourceGraphEdge edge = new(
            source,
            target,
            new ResourceRelationship(Identity(sourceNode.Resource), Identity(targetNode.Resource), kind));

        edge.RelationshipName.ShouldBe(kind.ToString());
        edge.ToString().ShouldBe(kind.ToString());
        edge.ThemeClass.ShouldBe(themeClass);
    }

    public static IEnumerable<object[]> RelationshipKinds()
    {
        yield return [ResourceRelationshipKind.Owner, "RelationshipOwner"];
        yield return [ResourceRelationshipKind.Reference, "RelationshipReference"];
        yield return [ResourceRelationshipKind.Selector, "RelationshipSelector"];
        yield return [ResourceRelationshipKind.Label, "RelationshipLabel"];
        yield return [ResourceRelationshipKind.Storage, "RelationshipStorage"];
        yield return [ResourceRelationshipKind.Identity, "RelationshipIdentity"];
        yield return [ResourceRelationshipKind.Rbac, "RelationshipRbac"];
        yield return [ResourceRelationshipKind.Event, "RelationshipEvent"];
        yield return [ResourceRelationshipKind.GitOps, "RelationshipGitOps"];
        yield return [((ResourceRelationshipKind)999), "RelationshipDefault"];
    }

    [Fact]
    public void Graph_edge_includes_relationship_label()
    {
        ResourceNodeViewModel sourceNode = Node("source");
        ResourceNodeViewModel targetNode = Node("target");
        ResourceGraphEdge edge = new(
            Vertex(sourceNode),
            Vertex(targetNode),
            new ResourceRelationship(Identity(sourceNode.Resource), Identity(targetNode.Resource), ResourceRelationshipKind.Reference, "uses"));

        edge.ToString().ShouldBe("Reference: uses");
    }

    [Fact]
    public void Graph_vertex_describes_kind_and_name()
    {
        ResourceNodeViewModel node = Node("widget");
        ResourceGraphVertex vertex = Vertex(node);

        vertex.ToString().ShouldBe("Pod/widget");
    }

    private static ResourceNodeViewModel Node(string name)
        => new() { Resource = new V1Pod { ApiVersion = "v1", Kind = "Pod", Metadata = new() { Name = name, NamespaceProperty = "default" } }, Icon = null! };

    private static ResourceGraphVertex Vertex(ResourceNodeViewModel node)
        => new() { Node = node, Identity = Identity(node.Resource) };

    private static ResourceIdentity Identity(IKubernetesObject<V1ObjectMeta> resource)
        => new(resource.ApiVersion!, resource.Kind!, resource.Namespace(), resource.Name()!, resource.Uid());
}
