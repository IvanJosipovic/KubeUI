using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Visualization;
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
    public void Custom_resource_status_is_checked_by_condition_name()
    {
        var resource = new CustomResource
        {
            Status = new CustomStatus
            {
                Conditions = [new CustomCondition { Status = "False" }],
            },
        };

        ResourceReadiness.IsNotReady(resource).ShouldBeTrue();
    }

    [Fact]
    public void Resource_without_status_is_ready()
    {
        ResourceReadiness.IsNotReady(new CustomResource()).ShouldBeFalse();
    }

    private sealed class CustomResource : IKubernetesObject<V1ObjectMeta>
    {
        public string? ApiVersion { get; set; }

        public string? Kind { get; set; }

        public V1ObjectMeta Metadata { get; set; } = new();

        public CustomStatus? Status { get; set; }
    }

    private sealed class CustomStatus
    {
        public IReadOnlyList<CustomCondition>? Conditions { get; set; }
    }

    private sealed class CustomCondition
    {
        public string? Status { get; set; }
    }
}
