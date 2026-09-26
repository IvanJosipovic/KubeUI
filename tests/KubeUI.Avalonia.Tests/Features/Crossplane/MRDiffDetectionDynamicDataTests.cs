using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using KubeUI.Avalonia.Infrastructure.Threading;

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

    [AvaloniaFact]
    public async Task Source_cache_changes_off_ui_thread_and_bound_list_changes_on_ui_thread()
    {
        using var source = new SourceCache<TestRow, string>(row => row.Key);
        using var comparer = new BehaviorSubject<IComparer<TestRow>>(Comparer<TestRow>.Create(
            static (left, right) => string.Compare(left.Key, right.Key, StringComparison.Ordinal)));
        ReadOnlyObservableCollection<TestRow>? view = null;
        using var subscription = source.Connect()
            .ObserveOn(TaskPoolScheduler.Default)
            .SortAndBind(out view, comparer, new()
            {
                ResetOnFirstTimeLoad = true,
                UseReplaceForUpdates = true,
                Scheduler = AvaloniaScheduler.Instance
            })
            .Subscribe();
        var collectionUpdate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        ((INotifyCollectionChanged)view!).CollectionChanged += (_, _) => collectionUpdate.TrySetResult(Dispatcher.UIThread.CheckAccess());

        var cacheUpdateWasOnUiThread = await Task.Run(() =>
        {
            var isOnUiThread = Dispatcher.UIThread.CheckAccess();
            source.AddOrUpdate(new TestRow("row-1"));
            return isOnUiThread;
        });
        var boundCollectionUpdateWasOnUiThread = await collectionUpdate.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.False(cacheUpdateWasOnUiThread);
        Assert.True(boundCollectionUpdateWasOnUiThread);
    }

    private sealed record TestRow(string Key);
}
