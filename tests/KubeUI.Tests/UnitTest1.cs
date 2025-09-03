using AvaloniaEdit.Document;
using FluentAssertions;
using k8s.Models;
using KubeUI.Client;
using KubeUI.ViewModels;
using KubeUI.Views;

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
        foldings.Count().Should().Be(2);
        foldings[0].Name.Should().Be($"prop2:{Environment.NewLine}");
        foldings[1].Name.Should().Be($"  prop2Nested:{Environment.NewLine}");
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
        foldings.Count().Should().Be(1);
        foldings[0].Name.Should().Be($"prop1:{Environment.NewLine}");
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
        foldings.Count().Should().Be(4);
        foldings[0].Name.Should().Be($"prop1:{Environment.NewLine}");
        foldings[1].Name.Should().Be($"- prop1Nested1:{Environment.NewLine}");
        foldings[2].Name.Should().Be($"  - prop1Nested1Prop2: val1{Environment.NewLine}");

        foldings[3].Name.Should().Be($"- prop2Nested1:{Environment.NewLine}");
    }
}
