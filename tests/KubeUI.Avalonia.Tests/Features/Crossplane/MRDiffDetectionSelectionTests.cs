using DynamicData;

namespace KubeUI.Avalonia.Tests.Features.Crossplane;

public sealed class MRDiffDetectionSelectionTests
{
    [Fact]
    public void Updating_a_diff_row_preserves_the_selected_object_identity()
    {
        using var source = new SourceCache<CrossplaneDiffRow, string>(row => row.Key);
        var selected = CreateRow("old");
        source.AddOrUpdate(selected);

        var replacement = CreateRow("new");
        selected.Apply(replacement);
        source.Refresh(selected);

        var current = source.Items.Single();
        Assert.Same(selected, current);
    }

    private static CrossplaneDiffRow CreateRow(string value)
        => new(new CrossplaneDiffRecord("uid", "name", "namespace", "example/v1", "Widget", "spec.value", value, value, false, false, false));
}
