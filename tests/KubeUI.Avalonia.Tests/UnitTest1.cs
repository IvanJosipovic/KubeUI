using AvaloniaEdit.Document;
using k8s.Models;
using Shouldly;
using KubeUI.Avalonia.ViewModels;
using KubeUI.Avalonia.Views;

namespace KubeUI.Avalonia.Tests;

public class UnitTest1
{
    [Fact]
    public void PathBuilder()
    {
        Utilities.PathBuilder<V1ContainerPort>(x => x.ContainerPort).ShouldBe("ContainerPort");
        Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Spec).ShouldBe("SelectedItem.Spec");
        Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Spec.Affinity).ShouldBe("SelectedItem.Spec.Affinity");
        Utilities.PathBuilder<ResourceListViewModel<V1Pod>>(x => x.SelectedItem.Spec.Containers).ShouldBe("SelectedItem.Spec.Containers");
    }
}

