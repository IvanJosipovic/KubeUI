using AvaloniaEdit.Document;
using FluentAssertions;
using k8s.Models;
using KubeUI.ViewModels;
using KubeUI.Views;
using Kubernetes.Controller.Client;

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
        Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Spec).Should().Be("SelectedItem.Spec");
        Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Spec.Affinity).Should().Be("SelectedItem.Spec.Affinity");
        Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Spec.Containers).Should().Be("SelectedItem.Spec.Containers");
    }
}
