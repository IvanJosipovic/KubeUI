#nullable enable

using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSorting;
using BenchmarkDotNet.Attributes;
using DynamicData;
using DynamicData.Binding;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.List;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Resources;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
[BenchmarkCategory("ResourceList", "DynamicData")]
public class ResourceListPipelineBenchmarks : IDisposable
{
    private SourceCache<V1Pod, ResourceCacheKey> _cache = null!;
    private BehaviorSubject<Func<V1Pod, bool>> _filterSubject = null!;
    private BehaviorSubject<IComparer<V1Pod>> _sortSubject = null!;
    private IDisposable _subscription = null!;
    private IReadOnlyList<V1Pod> _items = null!;
    private V1Pod _replacement = null!;
    private ReadOnlyObservableCollection<V1Pod> _view = null!;
    private Func<V1Pod, bool> _matchingFilter = null!;
    private Func<V1Pod, bool> _nonMatchingFilter = null!;
    private IComparer<V1Pod> _ascendingComparer = null!;
    private IComparer<V1Pod> _descendingComparer = null!;

    [Params(100, 1_000, 10_000)]
    public int ItemCount { get; set; }

    [Params(1, 25)]
    public int BindingResetThreshold { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _items = BuildItems(ItemCount);
        var replacementIndex = ItemCount / 2;
        _replacement = CreatePod(replacementIndex, $"pod-{replacementIndex:D4}");
        _replacement.Metadata!.ResourceVersion = "replacement";

        _matchingFilter = static pod => pod.Metadata?.Name?.Contains("pod-0001", StringComparison.Ordinal) == true;
        _nonMatchingFilter = static _ => true;
        _ascendingComparer = Comparer<V1Pod>.Create(static (left, right) => string.CompareOrdinal(left.Name(), right.Name()));
        _descendingComparer = Comparer<V1Pod>.Create(static (left, right) => string.CompareOrdinal(right.Name(), left.Name()));

        _cache = new SourceCache<V1Pod, ResourceCacheKey>(ResourceCacheKey.From);
        _filterSubject = new(_nonMatchingFilter);
        _sortSubject = new(_ascendingComparer);
        _subscription = _cache.Connect()
            .Filter(_filterSubject)
            .Sort(_sortSubject)
            .Bind(out _view, new()
            {
                ResetThreshold = BindingResetThreshold
            })
            .Subscribe();

        ResetCache();
    }

    [IterationSetup]
    public void Reset()
    {
        ResetCache();
    }

    [Benchmark]
    public int ReplaceOneResource()
    {
        _cache.AddOrUpdate(_replacement);
        return _view.Count;
    }

    [Benchmark]
    public int ApplyFilterChange()
    {
        _filterSubject.OnNext(_matchingFilter);
        return _view.Count;
    }

    [Benchmark]
    public string ApplySortChange()
    {
        _sortSubject.OnNext(_descendingComparer);
        return _view[0].Name();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _subscription.Dispose();
        _sortSubject.Dispose();
        _filterSubject.Dispose();
        _cache.Dispose();
    }

    public void Dispose() => Cleanup();

    private void ResetCache()
    {
        _filterSubject.OnNext(_nonMatchingFilter);
        _sortSubject.OnNext(_ascendingComparer);
        _cache.Edit(updater =>
        {
            updater.Clear();
            updater.AddOrUpdate(_items);
        });
    }

    private static V1Pod[] BuildItems(int count)
    {
        var items = new V1Pod[count];
        for (var i = 0; i < count; i++)
        {
            items[i] = CreatePod(i, $"pod-{i:D4}");
        }

        return items;
    }

    private static V1Pod CreatePod(int index, string name)
    {
        return new V1Pod
        {
            ApiVersion = V1Pod.KubeApiVersion,
            Kind = V1Pod.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = name,
                NamespaceProperty = $"ns-{index % 8}",
                ResourceVersion = index.ToString()
            }
        };
    }
}
