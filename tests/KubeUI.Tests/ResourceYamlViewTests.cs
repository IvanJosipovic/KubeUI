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
    public void YamlFolding_FoldsNestedMappings()
    {
        var text = new TextDocument();
        text.Text = """
            prop1: val
            prop2:
              prop2Nested:
                prop2NestedProp1: val0
            prop3:
            """;


        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.Should().Be(2);
        foldings[0].Name.TrimEnd().Should().Be($"prop2:");
        foldings[1].Name.TrimEnd().Should().Be($"prop2Nested:");
    }

    [Fact]
    public void YamlFolding_FoldsMappingWithSequenceChildren()
    {
        var text = new TextDocument();
        text.Text = """
            prop1:
            - prop1Nested1:
            - prop1Nested2:
            - prop1Nested3:
            """;


        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.Should().Be(1);
        foldings[0].Name.TrimEnd().Should().Be($"prop1:");
    }

    [Fact]
    public void YamlFolding_FoldsNestedSequences()
    {
        var text = new TextDocument();
        text.Text = """
            prop1:
            - prop1Nested1:
              - prop1Nested1Prop1: val0
              - prop1Nested1Prop2: val1
                prop1Nested1Prop2Nested: val2
            - prop1Nested2:
              - prop1Nested2Prop1: val3
              - prop1Nested2Prop2: val4
            - prop1Nested3:
            """;


        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.Should().Be(4);
        foldings[0].Name.TrimEnd().Should().Be($"prop1:");
        foldings[1].Name.TrimEnd().Should().Be($"- prop1Nested1:");
        foldings[2].Name.TrimEnd().Should().Be($"- prop1Nested1Prop2: val1");

        foldings[3].Name.TrimEnd().Should().Be($"- prop1Nested2:");
    }

    [Fact]
    public void YamlFolding_IgnoresBlankAndCommentLines()
    {
        var text = new TextDocument();
        text.Text = """
            # header

            prop1:
              # comment
              prop1Nested: val

            prop2: val
            """;

        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.Should().Be(1);
        foldings[0].Name.TrimEnd().Should().Be($"prop1:");
    }

    [Fact]
    public void YamlFolding_DoesNotFoldFlatMappings()
    {
        var text = new TextDocument();
        text.Text = """
            prop1: val
            prop2: val
            """;

        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.Should().Be(0);
    }

    [Fact]
    public void YamlFolding_DoesNotFoldListItemsWithoutChildren()
    {
        var text = new TextDocument();
        text.Text = """
            - item1
            - item2
            """;

        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.Should().Be(0);
    }

    [Fact]
    public void YamlFolding_FoldsListItemWithChildren()
    {
        var text = new TextDocument();
        text.Text = """
            - item1:
              child1: val
            - item2
            """;

        var foldings = YamlFoldingStrategy.CreateNewFoldings(text, out _).ToList();
        foldings.Count.Should().Be(1);
        foldings[0].Name.TrimEnd().Should().Be($"- item1:");
    }
}
