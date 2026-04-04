using System;
using System.Globalization;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.DataGridFiltering;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.List.Controls;
using KubeUI.Avalonia.Features.Resources.List.Views;
using KubeUI.Avalonia.Resources;
using Microsoft.VSDiagnostics;

namespace KubeUI.Benchmarks;

[SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 0, iterationCount: 1)]
[CPUUsageDiagnoser]
public class ResourceListFilterFlyoutFactoryBenchmarks
{
    private object _factory = null!;
    private object _resourceColumn = null!;
    private DataGridColumnDefinition _column = null!;
    private IFilteringModel _filteringModel = null!;
    private MethodInfo _create = null!;
    [GlobalSetup]
    public void Setup()
    {
        var assembly = typeof(ResourceListView).Assembly;
        var serviceType = assembly.GetType("KubeUI.Avalonia.Controls.DataGridFilters.DataGridColumnFilterService", throwOnError: true)!;
        var factoryType = assembly.GetType("KubeUI.Avalonia.Controls.DataGridFilters.DataGridColumnFilterFlyoutFactory", throwOnError: true)!;
        var service = Activator.CreateInstance(serviceType, nonPublic: true)!;
        _factory = Activator.CreateInstance(factoryType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, binder: null, args: [service], culture: CultureInfo.InvariantCulture)!;
        _create = factoryType.GetMethod("Create", BindingFlags.Instance | BindingFlags.Public)!;
        _resourceColumn = new BenchmarkColumn("name", "Name", typeof(string), typeof(ResourceTextCell));
        _column = new DataGridControlTemplateColumnDefinition
        {
            Header = "Name",
            ColumnKey = "name",
            Tag = _resourceColumn,
            ShowFilterButton = true,
            ValueType = typeof(string),
            Options = new DataGridColumnDefinitionOptions
            {
                IsSearchable = true
            }
        };
        _filteringModel = new FilteringModel
        {
            OwnsViewFilter = true
        };
    }

    [Benchmark(Baseline = true)]
    public int CreateTextFlyout()
    {
        var flyout = _create.Invoke(_factory, new object[] { _resourceColumn, _column, _filteringModel });
        GC.KeepAlive(flyout);
        return flyout is null ? 0 : 1;
    }

    private sealed class BenchmarkColumn : IResourceListColumn
    {
        public BenchmarkColumn(string key, string name, Type valueType, Type customControl)
        {
            Key = key;
            Name = name;
            ValueType = valueType;
            CustomControl = customControl;
            ValueAccessor = new BenchmarkValueAccessor();
            SortKey = static _ => null;
            DisplayValue = static _ => string.Empty;
        }

        public string Key { get; }
        public string Name { get; }
        public string? Width => null;
        public SortDirection Sort { get; set; }
        public Type CustomControl { get; }
        public Type ItemType => typeof(V1Pod);
        public Type ValueType { get; }
        public IDataGridColumnValueAccessor ValueAccessor { get; }
        public Func<object, IComparable?> SortKey { get; }
        public Func<object, string> DisplayValue { get; }
    }

    private sealed class BenchmarkValueAccessor : IDataGridColumnValueAccessor
    {
        public Type ItemType => typeof(V1Pod);
        public Type ValueType => typeof(string);
        public bool CanWrite => false;

        public object GetValue(object item)
        {
            return ((V1Pod)item).Metadata?.Name ?? string.Empty;
        }

        public void SetValue(object item, object value)
        {
            throw new NotSupportedException();
        }
    }
}
