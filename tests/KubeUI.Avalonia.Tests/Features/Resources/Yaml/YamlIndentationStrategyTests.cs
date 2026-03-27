using AvaloniaEdit.Document;
using KubeUI.Avalonia.ViewModels;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Yaml;

public class YamlIndentationStrategyTests
{
    private readonly YamlIndentationStrategy _strategy = new();

    [Fact]
    public void IndentLine_IncreasesIndentAfterMappingKey()
    {
        var document = new TextDocument("spec:\n");
        var line = document.GetLineByNumber(2);

        _strategy.IndentLine(document, line);

        document.Text.ShouldBe("spec:\n  ");
    }

    [Fact]
    public void IndentLine_IncreasesIndentAfterSequenceMappingEntry()
    {
        var document = new TextDocument("  - name: demo\n");
        var line = document.GetLineByNumber(2);

        _strategy.IndentLine(document, line);

        document.Text.ShouldBe("  - name: demo\n    ");
    }

    [Fact]
    public void IndentLine_KeepsIndentAfterScalarSequenceEntry()
    {
        var document = new TextDocument("  - sleep\n");
        var line = document.GetLineByNumber(2);

        _strategy.IndentLine(document, line);

        document.Text.ShouldBe("  - sleep\n  ");
    }

    [Fact]
    public void IndentLine_IncreasesIndentAfterBareSequenceEntry()
    {
        var document = new TextDocument("  - \n");
        var line = document.GetLineByNumber(2);

        _strategy.IndentLine(document, line);

        document.Text.ShouldBe("  - \n    ");
    }

    [Fact]
    public void IndentLine_KeepsIndentAfterScalarMappingValue()
    {
        var document = new TextDocument("    image: ubuntu:latest\n");
        var line = document.GetLineByNumber(2);

        _strategy.IndentLine(document, line);

        document.Text.ShouldBe("    image: ubuntu:latest\n    ");
    }

    [Fact]
    public void IndentLine_SkipsBlankLinesWhenFindingContext()
    {
        var document = new TextDocument("spec:\n\n");
        var line = document.GetLineByNumber(3);

        _strategy.IndentLine(document, line);

        document.Text.ShouldBe("spec:\n\n  ");
    }

    [Fact]
    public void IndentLine_SkipsCommentLinesWhenFindingContext()
    {
        var document = new TextDocument("spec:\n  # comment\n");
        var line = document.GetLineByNumber(3);

        _strategy.IndentLine(document, line);

        document.Text.ShouldBe("spec:\n  # comment\n  ");
    }

    [Fact]
    public void IndentLine_IncreasesIndentAfterMappingKeyWithInlineComment()
    {
        var document = new TextDocument("metadata: # comment\n");
        var line = document.GetLineByNumber(2);

        _strategy.IndentLine(document, line);

        document.Text.ShouldBe("metadata: # comment\n  ");
    }

    [Fact]
    public void IndentLine_DoesNotTreatQuotedHashAsComment()
    {
        var document = new TextDocument("value: \"abc#123\"\n");
        var line = document.GetLineByNumber(2);

        _strategy.IndentLine(document, line);

        document.Text.ShouldBe("value: \"abc#123\"\n");
    }

    [Fact]
    public void IndentLine_ReplacesExistingIndentation()
    {
        var document = new TextDocument("spec:\n      ");
        var line = document.GetLineByNumber(2);

        _strategy.IndentLine(document, line);

        document.Text.ShouldBe("spec:\n  ");
    }

    [Fact]
    public void IndentLines_IndentsAllLinesInRange()
    {
        var document = new TextDocument("spec:\n\n");

        _strategy.IndentLines(document, 2, 3);

        document.Text.ShouldBe("spec:\n  \n  ");
    }
}
