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
        Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.Source.RowSelection.SelectedItem.Spec).Should().Be("Source.RowSelection.SelectedItem.Spec");
        Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.Source.RowSelection.SelectedItem.Spec.Affinity).Should().Be("Source.RowSelection.SelectedItem.Spec.Affinity");
        Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.Source.RowSelection.SelectedItem.Spec.Containers).Should().Be("Source.RowSelection.SelectedItem.Spec.Containers");
    }
}
