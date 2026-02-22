using AvaloniaEdit.Document;
using Shouldly;
using k8s.Models;
using KubeUI.ViewModels;
using KubeUI.Views;
using KubernetesClient.Informer.Client;

namespace KubeUI.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        GroupApiVersionKind.From<V1Pod>().Kind.ShouldBe("Pod");
    }

    [Fact]
    public void PathBuilder()
    {
        Utilities.PathBuilder<V1ContainerPort>(x => x.ContainerPort).ShouldBe("ContainerPort");
        Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Spec).ShouldBe("SelectedItem.Spec");
        Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Spec.Affinity).ShouldBe("SelectedItem.Spec.Affinity");
        Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Spec.Containers).ShouldBe("SelectedItem.Spec.Containers");
    }
}
