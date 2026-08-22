using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Kubernetes;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Visualization;

public sealed class ResourceReadinessTests
{
    [Fact]
    public void Known_resource_with_false_condition_is_not_ready()
    {
        var pod = new V1Pod
        {
            Status = new V1PodStatus
            {
                Conditions = [new V1PodCondition { Status = "False" }],
            },
        };

        ResourceReadiness.IsNotReady(pod).ShouldBeTrue();
    }

    [Fact]
    public void Known_resource_without_false_condition_is_ready()
    {
        var pod = new V1Pod
        {
            Status = new V1PodStatus
            {
                Conditions = [new V1PodCondition { Status = "True" }],
            },
        };

        ResourceReadiness.IsNotReady(pod).ShouldBeFalse();
    }

    [Fact]
    public void Json_backed_custom_resource_status_is_checked_by_condition_name()
    {
        var resource = KubernetesJson.Deserialize<GenericKubernetesObject>("""
            {
              "apiVersion": "example.com/v1",
              "kind": "Widget",
              "metadata": { "name": "widget" },
              "status": { "conditions": [{ "status": "False" }] }
            }
            """)!;

        ResourceReadiness.IsNotReady(resource).ShouldBeTrue();
    }

    [Fact]
    public void Resource_without_status_is_ready()
    {
        ResourceReadiness.IsNotReady(KubernetesJson.Deserialize<GenericKubernetesObject>("""
            {
              "apiVersion": "example.com/v1",
              "kind": "Widget",
              "metadata": { "name": "widget" }
            }
            """)!).ShouldBeFalse();
    }
}
