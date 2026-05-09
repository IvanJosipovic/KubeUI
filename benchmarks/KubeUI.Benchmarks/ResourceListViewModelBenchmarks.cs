#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSorting;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.List.ViewModels;
using KubeUI.Avalonia.Resources;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class ResourceListViewModelBenchmarks
{
    private IReadOnlyDictionary<string, IResourceListColumn> _columnsByKey = null!;
    private IReadOnlyList<IResourceListColumn> _orderedColumns = null!;
    private IReadOnlyList<SortingDescriptor> _sortDescriptors = null!;
    private IReadOnlyList<FilteringDescriptor> _filterDescriptors = null!;
    private IReadOnlyList<SearchDescriptor> _searchAllDescriptors = null!;
    private IReadOnlyList<SearchDescriptor> _searchExplicitDescriptors = null!;
    private IReadOnlyList<V1Pod> _items = null!;

    private DynamicDataSortingAdapterFactory<V1Pod> _sortingFactory = null!;
    private DynamicDataFilteringAdapterFactory<V1Pod> _filteringFactory = null!;
    private DynamicDataSearchAdapterFactory<V1Pod> _searchFactoryAll = null!;
    private DynamicDataSearchAdapterFactory<V1Pod> _searchFactoryExplicit = null!;

    [Params(3, 6, 8)]
    public int ColumnCount { get; set; }

    [Params(1, 3)]
    public int DescriptorCount { get; set; }

    [Params(100, 1000)]
    public int ItemCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var columns = BuildColumns(ColumnCount);
        _orderedColumns = columns;
        var columnsByKey = new Dictionary<string, IResourceListColumn>(columns.Count, StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            columnsByKey[column.Key] = column;
        }

        _columnsByKey = columnsByKey;

        _sortingFactory = new DynamicDataSortingAdapterFactory<V1Pod>(_columnsByKey);
        _filteringFactory = new DynamicDataFilteringAdapterFactory<V1Pod>(_columnsByKey);
        _searchFactoryAll = new DynamicDataSearchAdapterFactory<V1Pod>(_columnsByKey);
        _searchFactoryExplicit = new DynamicDataSearchAdapterFactory<V1Pod>(_columnsByKey);

        _sortDescriptors = BuildSortingDescriptors(columns, DescriptorCount);
        _filterDescriptors = BuildFilteringDescriptors(columns, DescriptorCount);
        _searchAllDescriptors = [new SearchDescriptor("pod-1", scope: SearchScope.AllColumns, comparison: StringComparison.OrdinalIgnoreCase)];
        _searchExplicitDescriptors = [new SearchDescriptor(
            "pod-1",
            scope: SearchScope.ExplicitColumns,
            columnIds: BuildExplicitColumnIds(columns, DescriptorCount),
            comparison: StringComparison.OrdinalIgnoreCase)];

        _items = BuildItems(ItemCount);

        _sortingFactory.UpdateComparer(_sortDescriptors);
        _filteringFactory.UpdateFilter(_filterDescriptors);
        _searchFactoryAll.UpdatePredicate(_searchAllDescriptors);
        _searchFactoryExplicit.UpdatePredicate(_searchExplicitDescriptors);
    }

    [Benchmark(Baseline = true)]
    public void UpdateSortComparer()
    {
        _sortingFactory.UpdateComparer(_sortDescriptors);
    }

    [Benchmark]
    public void UpdateFilterPredicate()
    {
        _filteringFactory.UpdateFilter(_filterDescriptors);
    }

    [Benchmark]
    public void UpdateSearchPredicate_AllColumns()
    {
        _searchFactoryAll.UpdatePredicate(_searchAllDescriptors);
    }

    [Benchmark]
    public void UpdateSearchPredicate_ExplicitColumns()
    {
        _searchFactoryExplicit.UpdatePredicate(_searchExplicitDescriptors);
    }

    [Benchmark]
    public int CountFilterMatches()
    {
        var predicate = _filteringFactory.FilterPredicate;
        var count = 0;
        for (var i = 0; i < _items.Count; i++)
        {
            if (predicate(_items[i]))
            {
                count++;
            }
        }

        return count;
    }

    [Benchmark]
    public int CountSearchMatches_AllColumns()
    {
        var predicate = _searchFactoryAll.SearchPredicate;
        var count = 0;
        for (var i = 0; i < _items.Count; i++)
        {
            if (predicate(_items[i]))
            {
                count++;
            }
        }

        return count;
    }

    [Benchmark]
    public int CountSearchMatches_ExplicitColumns()
    {
        var predicate = _searchFactoryExplicit.SearchPredicate;
        var count = 0;
        for (var i = 0; i < _items.Count; i++)
        {
            if (predicate(_items[i]))
            {
                count++;
            }
        }

        return count;
    }

    [Benchmark]
    public int CompareAdjacentItems()
    {
        var comparer = _sortingFactory.SortComparer;
        var checksum = 0;
        for (var i = 1; i < _items.Count; i++)
        {
            checksum += comparer.Compare(_items[i - 1], _items[i]);
        }

        return checksum;
    }

    private static IReadOnlyList<IResourceListColumn> BuildColumns(int columnCount)
    {
        IResourceListColumn[] columns =
        [
            new BenchmarkColumn<string>("name", "Name", static pod => pod.Metadata?.Name ?? string.Empty),
            new BenchmarkColumn<string>("namespace", "Namespace", static pod => pod.Metadata?.NamespaceProperty ?? string.Empty),
            new BenchmarkColumn<string>("key", "Key", static pod => $"{pod.Metadata?.NamespaceProperty}/{pod.Metadata?.Name}"),
            new BenchmarkColumn<int>("name-length", "Name Length", static pod => pod.Metadata?.Name?.Length ?? 0),
            new BenchmarkColumn<int>("namespace-length", "Namespace Length", static pod => pod.Metadata?.NamespaceProperty?.Length ?? 0),
            new BenchmarkColumn<string>("kind", "Kind", static pod => pod.Kind ?? string.Empty),
            new BenchmarkColumn<DateTime>("created", "Created", static pod => pod.Metadata?.CreationTimestamp ?? DateTime.UnixEpoch),
            new BenchmarkColumn<string>("resource-version", "Resource Version", static pod => pod.Metadata?.ResourceVersion ?? string.Empty)
        ];

        var count = Math.Min(columnCount, columns.Length);
        var list = new List<IResourceListColumn>(count);
        for (var i = 0; i < count; i++)
        {
            list.Add(columns[i]);
        }

        return list;
    }

    private static IReadOnlyList<SortingDescriptor> BuildSortingDescriptors(IReadOnlyList<IResourceListColumn> columns, int descriptorCount)
    {
        var count = Math.Min(descriptorCount, columns.Count);
        var descriptors = new SortingDescriptor[count];
        for (var i = 0; i < count; i++)
        {
            var direction = i % 2 == 0 ? ListSortDirection.Ascending : ListSortDirection.Descending;
            descriptors[i] = new SortingDescriptor(columns[i].Key, direction);
        }

        return descriptors;
    }

    private static IReadOnlyList<FilteringDescriptor> BuildFilteringDescriptors(IReadOnlyList<IResourceListColumn> columns, int descriptorCount)
    {
        var count = Math.Min(descriptorCount, columns.Count);
        var descriptors = new FilteringDescriptor[count];
        for (var i = 0; i < count; i++)
        {
            descriptors[i] = new FilteringDescriptor(
                columns[i].Key,
                FilteringOperator.Contains,
                value: i == 0 ? "pod-1" : "ns-1");
        }

        return descriptors;
    }

    private static IReadOnlyList<object> BuildExplicitColumnIds(IReadOnlyList<IResourceListColumn> columns, int descriptorCount)
    {
        var count = Math.Min(descriptorCount, columns.Count);
        var columnIds = new object[count];
        for (var i = 0; i < count; i++)
        {
            columnIds[i] = columns[i].Key;
        }

        return columnIds;
    }

    private static IReadOnlyList<V1Pod> BuildItems(int itemCount)
    {
        var items = new List<V1Pod>(itemCount);
        for (var i = 0; i < itemCount; i++)
        {
            items.Add(new V1Pod
            {
                ApiVersion = V1Pod.KubeApiVersion,
                Kind = V1Pod.KubeKind,
                Metadata = new V1ObjectMeta
                {
                    Name = $"pod-{i:D4}",
                    NamespaceProperty = $"ns-{i % 8}",
                    CreationTimestamp = DateTime.UtcNow.AddMinutes(-i),
                    ResourceVersion = i.ToString(CultureInfo.InvariantCulture)
                }
            });
        }

        return items;
    }

    private sealed class BenchmarkColumn<TValue> : IResourceListColumn where TValue : IComparable
    {
        private readonly Func<V1Pod, TValue> _getter;

        public BenchmarkColumn(string key, string name, Func<V1Pod, TValue> getter)
        {
            Key = key;
            Name = name;
            _getter = getter;
            ValueAccessor = new BenchmarkColumnValueAccessor(getter);
            SortKey = resource => _getter((V1Pod)resource);
            DisplayValue = resource => _getter((V1Pod)resource).ToString() ?? string.Empty;
        }

        public string Key { get; }

        public string Name { get; }

        public string? Width { get; } = null;

        public SortDirection Sort { get; set; } = SortDirection.None;

        public Type CustomControl { get; } = typeof(object);

        public Type ItemType { get; } = typeof(V1Pod);

        public Type ValueType { get; } = typeof(TValue);

        public IDataGridColumnValueAccessor ValueAccessor { get; }

        public Func<object, IComparable?> SortKey { get; }

        public Func<object, string> DisplayValue { get; }

        private sealed class BenchmarkColumnValueAccessor : IDataGridColumnValueAccessor
        {
            private readonly Func<V1Pod, TValue> _getter;

            public BenchmarkColumnValueAccessor(Func<V1Pod, TValue> getter)
            {
                _getter = getter;
            }

            public Type ItemType => typeof(V1Pod);

            public Type ValueType => typeof(TValue);

            public bool CanWrite => false;

            public object GetValue(object item)
            {
                return _getter((V1Pod)item)!;
            }

            public void SetValue(object item, object value)
            {
                throw new NotSupportedException();
            }
        }
    }
}
