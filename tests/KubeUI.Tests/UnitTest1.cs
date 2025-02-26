using FluentAssertions;
using k8s.Models;
using KubeUI.Client;
using KubeUI.ViewModels;

namespace KubeUI.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        GroupApiVersionKind.From<V1Pod>().Kind.Should().Be("Pod");
    }

    [Fact]
    public void PathBuilder()
    {
        Utilities.PathBuilder<V1ContainerPort>(x => x.ContainerPort).Should().Be("ContainerPort");
        Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Value.Spec).Should().Be("SelectedItem.Value.Spec");
        Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Value.Spec.Affinity).Should().Be("SelectedItem.Value.Spec.Affinity");
        Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Value.Spec.Containers).Should().Be("SelectedItem.Value.Spec.Containers");
    }
}
