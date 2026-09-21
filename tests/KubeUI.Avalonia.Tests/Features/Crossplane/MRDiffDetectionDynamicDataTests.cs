using System.Reactive.Subjects;
using DynamicData;

namespace KubeUI.Avalonia.Tests.Features.Crossplane;

public sealed class MRDiffDetectionDynamicDataTests
{
    [Fact]
    public void Initial_filter_predicate_is_replayed_to_a_late_dynamic_data_subscription()
    {
        using var source = new SourceCache<TestRow, string>(row => row.Key);
        using var predicate = new BehaviorSubject<Func<TestRow, bool>>(_ => true);
        var seen = 0;

        using var subscription = source.Connect()
            .Filter(predicate)
            .Subscribe(_ => seen++);

        source.AddOrUpdate(new TestRow("row-1"));

        Assert.True(seen > 0);
    }

    private sealed record TestRow(string Key);
}
