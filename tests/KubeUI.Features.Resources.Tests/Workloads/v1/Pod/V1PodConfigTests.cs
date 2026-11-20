using KubeUI.Features.Resources.Workloads.v1.Pod;
using Shouldly;

namespace KubeUI.Features.Resources.Tests.Workloads.v1.Pod;

public sealed class V1PodConfigTests
{
    [Fact]
    public void Browser_ids_do_not_collide_across_component_boundaries()
    {
        var first = V1PodConfig.BuildBrowserId("cluster", "a-b", "c", "app");
        var second = V1PodConfig.BuildBrowserId("cluster", "a", "b-c", "app");

        first.ShouldNotBe(second);
        V1PodConfig.BuildBrowserId("cluster", "a-b", "c", "app").ShouldBe(first);
    }
}
