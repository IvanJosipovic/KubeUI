using k8s.Models;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources.Workloads.v1.Pod.ViewModels;

/// <summary>
/// Covers the resource identity comparison used by the Pod log topology.
/// </summary>
public sealed class PodLogTopologyComparerTests
{
    [Fact]
    public void IsSameResource_matches_on_the_uid_when_both_are_present()
    {
        var current = CreatePod("pod-1", "uid-1");
        var next = CreatePod("pod-1", "uid-1");

        PodLogTopologyComparer.IsSameResource(current, next).ShouldBeTrue();
    }

    [Fact]
    public void IsSameResource_rejects_a_different_uid()
    {
        var current = CreatePod("pod-1", "uid-1");
        var next = CreatePod("pod-1", "uid-2");

        PodLogTopologyComparer.IsSameResource(current, next).ShouldBeFalse();
    }

    [Fact]
    public void IsSameResource_falls_back_to_the_name_when_the_uid_is_missing()
    {
        var current = CreatePod("pod-1", uid: null);
        var next = CreatePod("pod-1", uid: null);

        PodLogTopologyComparer.IsSameResource(current, next).ShouldBeTrue();
    }

    [Fact]
    public void IsSameResource_rejects_a_different_name_when_the_uid_is_missing()
    {
        var current = CreatePod("pod-1", uid: null);
        var next = CreatePod("pod-2", uid: null);

        PodLogTopologyComparer.IsSameResource(current, next).ShouldBeFalse();
    }

    [Fact]
    public void IsSameResource_matches_a_present_uid_against_a_missing_uid_only_by_name()
    {
        var current = CreatePod("pod-1", "uid-1");
        var next = CreatePod("pod-1", uid: null);

        // The uid cannot be compared, so the name decides the outcome.
        PodLogTopologyComparer.IsSameResource(current, next).ShouldBeTrue();
    }

    [Fact]
    public void IsSameResource_rejects_a_different_namespace()
    {
        var current = CreatePod("pod-1", uid: null, namespaceName: "default");
        var next = CreatePod("pod-1", uid: null, namespaceName: "other");

        PodLogTopologyComparer.IsSameResource(current, next).ShouldBeFalse();
    }

    [Fact]
    public void IsSameResource_treats_two_missing_resources_as_the_same()
    {
        PodLogTopologyComparer.IsSameResource(null, null).ShouldBeTrue();
    }

    [Fact]
    public void IsSameResource_rejects_a_missing_resource()
    {
        PodLogTopologyComparer.IsSameResource(CreatePod("pod-1", "uid-1"), null).ShouldBeFalse();
    }

    private static V1Pod CreatePod(string name, string? uid, string namespaceName = "default") => new()
    {
        Metadata = new V1ObjectMeta
        {
            Name = name,
            NamespaceProperty = namespaceName,
            Uid = uid,
        },
    };
}
