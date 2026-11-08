using AvaloniaEdit.Document;
using KubeUI.Controls.Editors;
using Shouldly;

namespace KubeUI.Controls.Tests;

public sealed class YamlIndentationFoldingStrategyTests
{
    [Fact]
    public void Creates_generic_nested_mapping_and_sequence_folds_without_field_policy()
    {
        var document = new TextDocument("""
            metadata:
              annotations:
                example.test/keep: value
              managedFields:
                - manager:
                    name: controller
            """.ReplaceLineEndings("\n"));

        var folds = YamlIndentationFoldingStrategy.CreateNewFoldings(document, out var firstErrorOffset).ToArray();

        firstErrorOffset.ShouldBe(-1);
        folds.Select(fold => fold.Name.Trim()).ShouldBe([
            "metadata:",
            "annotations:",
            "managedFields:",
            "- manager:"
        ]);
    }
}
