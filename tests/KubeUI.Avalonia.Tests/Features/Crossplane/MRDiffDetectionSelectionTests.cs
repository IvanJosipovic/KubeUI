using DynamicData;
using KubeUI.Avalonia.Infrastructure.DataGrid;

namespace KubeUI.Avalonia.Tests.Features.Crossplane;

public sealed class MRDiffDetectionSelectionTests
{
    [Fact]
    public void Updating_a_diff_row_replaces_the_cached_snapshot_by_key()
    {
        using var source = new SourceCache<CrossplaneDiffRow, string>(row => row.Key);
        var original = CreateRow("old");
        source.AddOrUpdate(original);

        var replacement = CreateRow("new");
        source.AddOrUpdate(replacement);

        var current = source.Items.Single();
        Assert.Same(replacement, current);
        Assert.Equal("old", original.OldValue);
    }

    [Fact]
    public void Selection_model_restores_selection_by_diff_key_after_view_updates()
    {
        var selected = CreateRow("old");
        var replacement = CreateRow("new");
        List<CrossplaneDiffRow> source = [selected];

        using var model = new IdentityPreservingSelectionModel<CrossplaneDiffRow, string>(row => row.Key)
        {
            Source = source
        };
        model.SetIdentitySource(source);
        model.Select(0);

        source[0] = replacement;
        model.SetIdentitySource(source);

        Assert.Same(replacement, model.SelectedItem);
    }

    private static CrossplaneDiffRow CreateRow(string value)
        => new(new CrossplaneDiffRecord("uid", "name", "namespace", "example/v1", "Widget", "spec.value", value, value, false, false, false));
}
