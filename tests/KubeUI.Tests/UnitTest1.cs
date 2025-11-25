using AvaloniaEdit.Document;
using FluentAssertions;
using k8s.Models;
using KubeUI.Client;
using KubeUI.ViewModels;
using KubeUI.Views;
using Yarp.Kubernetes.Controller.Client;

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

    [Fact]
    public void YamlFoldingTest1()
    {
        var text = new TextDocument();
        text.Text = """
            prop1: val
            prop2:
              prop2Nested:
                prop2NestedProp1: val0
            """;


        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.Should().Be(2);
        foldings[0].Name.TrimEnd().Should().Be($"prop2:");
        foldings[1].Name.TrimEnd().Should().Be($"  prop2Nested:");
    }

    [Fact]
    public void YamlFoldingTest2()
    {
        var text = new TextDocument();
        text.Text = """
            prop1:
            - prop1Nested1:
            - prop2Nested1:
            - prop2Nested1:
            """;


        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.Should().Be(1);
        foldings[0].Name.TrimEnd().Should().Be($"prop1:");
    }

    [Fact]
    public void YamlFoldingTest3()
    {
        var text = new TextDocument();
        text.Text = """
            prop1:
            - prop1Nested1:
              - prop1Nested1Prop1: val0
              - prop1Nested1Prop2: val1
                prop1Nested1Prop2Nested: val2
            - prop2Nested1:
              - prop2Nested1Prop1: val3
              - prop2Nested1Prop2: val4
            - prop2Nested1:
            """;


        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.Should().Be(4);
        foldings[0].Name.TrimEnd().Should().Be($"prop1:");
        foldings[1].Name.TrimEnd().Should().Be($"- prop1Nested1:");
        foldings[2].Name.TrimEnd().Should().Be($"  - prop1Nested1Prop2: val1");

        foldings[3].Name.TrimEnd().Should().Be($"- prop2Nested1:");
    }
}
