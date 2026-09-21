using Avalonia.Controls.DataGridFiltering;
using KubeUI.Avalonia.Infrastructure.DataGrid;
using KubeUI.Avalonia.Resources;

namespace KubeUI.Avalonia.Tests.Features.Crossplane;

public sealed class MRDiffDetectionFilteringTests
{
    [Fact]
    public void Filter_descriptor_targets_mr_diff_column_value()
    {
        var column = new DataGridValueColumn<CrossplaneDiffRow, string>
        {
            Key = "name",
            Name = "Name",
            Field = row => row.Name
        };
        var factory = new DynamicDataFilteringAdapterFactory<CrossplaneDiffRow>(
            new Dictionary<string, IResourceListColumn> { [column.Key] = column });
        var descriptor = new FilteringDescriptor(
            "name",
            FilteringOperator.Contains,
            null,
            "pipe",
            null,
            null,
            null,
            StringComparison.OrdinalIgnoreCase);

        factory.UpdateFilter([descriptor]);

        var matching = new CrossplaneDiffRow(new CrossplaneDiffRecord("uid", "pipeline", "ns", "example/v1", "Widget", "spec.value", "a", "b", false, false, false));
        var nonMatching = new CrossplaneDiffRow(new CrossplaneDiffRecord("uid-2", "database", "ns", "example/v1", "Widget", "spec.value", "a", "b", false, false, false));
        Assert.True(factory.FilterPredicate(matching));
        Assert.False(factory.FilterPredicate(nonMatching));
    }
}
