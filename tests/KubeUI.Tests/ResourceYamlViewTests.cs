using AvaloniaEdit.Document;
using FluentAssertions;
using k8s.Models;
using KubeUI.ViewModels;
using KubeUI.Views;
using KubernetesClient.Informer.Client;

namespace KubeUI.Tests;

public class ResourceYamlViewTests
{
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
