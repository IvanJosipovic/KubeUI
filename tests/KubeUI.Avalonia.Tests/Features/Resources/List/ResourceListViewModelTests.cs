using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentAvalonia.Core;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia;
using KubeUI.Avalonia.Controls.DataGridFilters;
using KubeUI.Avalonia.Features.Resources.List.Behaviors;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.List;

public class ResourceListViewModelTests : AvaloniaTestBase
{
    private readonly List<IDisposable> _disposables = [];
    private readonly List<Window> _windows = [];

    public override void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Content = null;
            window.Close();
        }

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        base.Dispose();
    }

    private Window CreateWindow(double width = 1200, double height = 800, object? content = null)
    {
        var window = new Window
        {
            Width = width,
            Height = height,
            Content = content,
        };

        _windows.Add(window);
        return window;
    }

    private async Task<ClusterWorkspaceViewModel> CreateClusterAsync()
    {
        var cluster = new TestCluster().CreateWorkspace();
        await cluster.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();
        _disposables.Add(cluster);
        return cluster;
    }

    private T GetRequiredService<T>() where T : class
    {
        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var service = services.GetRequiredService<T>();
        if (service is IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        return service;
    }

    private static V1Pod Pod(string ns, string name)
        => new()
        {
            ApiVersion = V1Pod.KubeApiVersion,
            Kind = V1Pod.KubeKind,
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = ns,
                Name = name,
                CreationTimestamp = DateTime.UtcNow,
            }
        };

    private static Corev1Event Event(string ns, string name)
    => new()
    {
        ApiVersion = Corev1Event.KubeApiVersion,
        Kind = Corev1Event.KubeKind,
        Metadata = new V1ObjectMeta
        {
            NamespaceProperty = ns,
            Name = name,
            CreationTimestamp = DateTime.UtcNow,
        },
        LastTimestamp = DateTime.UtcNow,
    };

    private static V1Namespace NamespaceResource(string name)
        => new()
        {
            Metadata = new V1ObjectMeta
            {
                Name = name
            }
        };

    private static async Task AddOrUpdateAsync<T>(ClusterWorkspaceViewModel cluster, T resource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await cluster.AddOrUpdateResource(resource);
        Dispatcher.UIThread.RunJobs();
    }

    private static IEnumerable<DataGridRow> GetAllRows(DataGrid grid)
    {
        var mi = grid.GetType().GetMethod("GetAllRows", BindingFlags.Instance | BindingFlags.NonPublic);
        mi.ShouldNotBeNull("ProDataGrid DataGrid should expose internal GetAllRows()");
        return (IEnumerable<DataGridRow>)mi!.Invoke(grid, null)!;
    }

    private static string? GetCellText(DataGrid grid, DataGridRow row, int columnIndex)
    {
        var content = grid.Columns[columnIndex].GetCellContent(row);
        if (content is TextBlock tb)
            return tb.Text;

        return content?.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault()?.Text;
    }

    private static string? GetFirstRowFirstColumnText(DataGrid grid, int row, int column)
    {
        // Make sure rows are generated.
        for (var i = 0; i < 5; i++)
        {
            grid.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        var rows = GetAllRows(grid).Where(x => x.IsVisible).ToList();

        var dataGridRow = rows[row];
        dataGridRow.ShouldNotBeNull();

        return GetCellText(grid, dataGridRow!, column);
    }


    [AvaloniaFact(DisplayName = "All select update middle")]
    public async Task all_select_update_middle_preserves_all_selected()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        // Seed 3 items
        await AddOrUpdateAsync(cluster, Pod("ns", "a"));
        await AddOrUpdateAsync(cluster, Pod("ns", "b"));
        await AddOrUpdateAsync(cluster, Pod("ns", "c"));

        // Select all 3
        vm.SelectionModel.Select(0);
        vm.SelectionModel.Select(1);
        vm.SelectionModel.Select(2);

        vm.SelectionModel.SelectedIndexes.ShouldBe([0, 1, 2]);

        // Replace 'b' with a new instance (same key)
        await AddOrUpdateAsync(cluster, Pod("ns", "b"));

        vm.SelectionModel.SelectedIndexes.ShouldBe([0, 1, 2]);

        vm.SelectedItems.Count.ShouldBe(3);

        vm.SelectedItems[0].Namespace().ShouldBe("ns");
        vm.SelectedItems[0].Name().ShouldBe("a");
        vm.SelectedItems[1].Namespace().ShouldBe("ns");
        vm.SelectedItems[1].Name().ShouldBe("b");
        vm.SelectedItems[2].Namespace().ShouldBe("ns");
        vm.SelectedItems[2].Name().ShouldBe("c");
    }

    [AvaloniaFact(DisplayName = "Single select update middle")]
    public async Task single_select_update__preserves_only_selected()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        // Seed 3 items
        await AddOrUpdateAsync(cluster, Pod("ns", "a"));
        await AddOrUpdateAsync(cluster, Pod("ns", "b"));
        await AddOrUpdateAsync(cluster, Pod("ns", "c"));

        // Select only middle
        vm.SelectionModel.Select(1);

        vm.SelectionModel.SelectedIndexes.ShouldBe([1]);

        // Replace 'b' with new instance (same key)
        await AddOrUpdateAsync(cluster, Pod("ns", "b"));

        vm.SelectionModel.SelectedIndexes.ShouldBe([1]);

        vm.SelectedItems.Count.ShouldBe(1);

        vm.SelectedItems[0].Namespace().ShouldBe("ns");
        vm.SelectedItems[0].Name().ShouldBe("b");

        vm.SelectedItem.Namespace().ShouldBe("ns");
        vm.SelectedItem.Name().ShouldBe("b");
    }

    [AvaloniaFact(DisplayName = "Single select with sort due to update")]
    public async Task single_select_with_sort_preserves_only_selected()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<Corev1Event>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        // Seed 3 items
        await AddOrUpdateAsync(cluster, Event("ns", "a"));
        await AddOrUpdateAsync(cluster, Event("ns", "b"));
        await AddOrUpdateAsync(cluster, Event("ns", "c"));

        vm.View.ElementAt(0).ShouldBeOfType<Corev1Event>().Name().ShouldBe("c");
        vm.View.ElementAt(1).ShouldBeOfType<Corev1Event>().Name().ShouldBe("b");
        vm.View.ElementAt(2).ShouldBeOfType<Corev1Event>().Name().ShouldBe("a");


        // Select only middle
        vm.SelectionModel.Select(1);

        vm.SelectionModel.SelectedIndexes.ShouldBe([1]);

        // Replace 'b' with new instance (same key)
        await AddOrUpdateAsync(cluster, Event("ns", "b"));
        Dispatcher.UIThread.RunJobs();

        vm.View.ElementAt(0).ShouldBeOfType<Corev1Event>().Name().ShouldBe("b");
        vm.View.ElementAt(1).ShouldBeOfType<Corev1Event>().Name().ShouldBe("c");
        vm.View.ElementAt(2).ShouldBeOfType<Corev1Event>().Name().ShouldBe("a");

        vm.SelectedItems.Count.ShouldBe(1);

        vm.SelectedItems[0].Namespace().ShouldBe("ns");
        vm.SelectedItems[0].Name().ShouldBe("b");


        vm.SelectedItem.Namespace().ShouldBe("ns");
        vm.SelectedItem.Name().ShouldBe("b");
    }

    [AvaloniaFact(DisplayName = "All select with sort due to update")]
    public async Task all_select_with_sort_preserves_all_selected()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<Corev1Event>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        // Seed 3 items
        await AddOrUpdateAsync(cluster, Event("ns", "a"));
        await AddOrUpdateAsync(cluster, Event("ns", "b"));
        await AddOrUpdateAsync(cluster, Event("ns", "c"));

        vm.View.ElementAt(0).ShouldBeOfType<Corev1Event>().Name().ShouldBe("c");
        vm.View.ElementAt(1).ShouldBeOfType<Corev1Event>().Name().ShouldBe("b");
        vm.View.ElementAt(2).ShouldBeOfType<Corev1Event>().Name().ShouldBe("a");


        // Select all 3
        vm.SelectionModel.Select(0);
        vm.SelectionModel.Select(1);
        vm.SelectionModel.Select(2);

        vm.SelectionModel.SelectedIndexes.ShouldBe([0, 1, 2]);

        // Replace 'b' with new instance (same key)
        await AddOrUpdateAsync(cluster, Event("ns", "b"));
        Dispatcher.UIThread.RunJobs();

        vm.View.ElementAt(0).ShouldBeOfType<Corev1Event>().Name().ShouldBe("b");
        vm.View.ElementAt(1).ShouldBeOfType<Corev1Event>().Name().ShouldBe("c");
        vm.View.ElementAt(2).ShouldBeOfType<Corev1Event>().Name().ShouldBe("a");

        vm.SelectionModel.SelectedIndexes.ShouldBe([0, 1, 2]);

        vm.SelectedItems.Count.ShouldBe(3);

        vm.SelectedItems[0].Namespace().ShouldBe("ns");
        vm.SelectedItems[0].Name().ShouldBe("b");


        vm.SelectedItem.Namespace().ShouldBe("ns");
        vm.SelectedItem.Name().ShouldBe("b");
    }

    [AvaloniaFact(DisplayName = "Update check DataGrid Text update")]
    public async Task UpdateResourceTextBox()
    {
        var window = CreateWindow();

        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();


        var pod = Pod("ns", "a");
        await AddOrUpdateAsync(cluster, pod);

        var before = GetFirstRowFirstColumnText(grid, 0, 0);
        before.ShouldNotBeNull();
        before.ShouldContain("a");

        // Mutate in place and trigger DynamicData refresh.
        pod.Metadata.Name = "b";
        await AddOrUpdateAsync(cluster, pod);

        var after = GetFirstRowFirstColumnText(grid, 0, 0);
        after.ShouldNotBeNull();
        after.ShouldContain("b");
    }

    [AvaloniaFact(DisplayName = "Update check DataGrid Text update2")]
    public async Task UpdateResourceTextBox2()
    {
        var window = CreateWindow();

        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Namespace>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();

        var ns = new V1Namespace()
        {
            Metadata = new()
            {
                Name = "a"
            }
        };

        await AddOrUpdateAsync(cluster, ns);

        var before = GetFirstRowFirstColumnText(grid, 0, 1);
        before.ShouldNotBeNull();
        before.ShouldBeEmpty();

        ns.Metadata.Labels = new Dictionary<string, string>()
        {
            {"test", "value" }
        };

        await AddOrUpdateAsync(cluster, ns);

        var after = GetFirstRowFirstColumnText(grid, 0, 1);
        after.ShouldNotBeNull();
        after.ShouldContain("test=value");
    }

    [AvaloniaFact(DisplayName = "Resource list columns expose filter buttons")]
    public async Task resource_list_columns_expose_filter_buttons()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();

        grid.Columns.ShouldNotBeEmpty();
        grid.Columns.All(column => column.ShowFilterButton == true).ShouldBeTrue();
        grid.Columns.All(column => column.FilterFlyout != null).ShouldBeTrue();
    }

    [AvaloniaFact(DisplayName = "Resource list filter flyout rows align editors")]
    public async Task resource_list_filter_flyout_rows_align_editors()
    {
        var flyoutFactory = GetRequiredService<DataGridColumnFilterFlyoutFactory>();

        var textCluster = await CreateClusterAsync();
        var textVm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        textVm.Initialize(textCluster);
        var textView = GetRequiredService<ResourceListView>();
        textView.DataContext = textVm;
        var textWindow = CreateWindow(content: textView);
        textWindow.Show();

        var textColumn = textVm.ColumnDefinitions.First(column => column.ValueType == typeof(string));
        var textFlyout = textColumn.FilterFlyout.ShouldBeOfType<Flyout>();
        textFlyout.ShowAt(textView);
        Dispatcher.UIThread.RunJobs();
        var textContent = textFlyout.Content.ShouldBeOfType<TextFilterFlyoutView>();
        var textPanel = textContent.Content.ShouldBeOfType<StackPanel>();
        var textRows = textPanel.Children.OfType<Grid>().ToList();
        textRows.Count.ShouldBeGreaterThanOrEqualTo(2);
        textRows[0].Children.OfType<TextBlock>().Single().Text.ShouldBe(KubeUI.Avalonia.Assets.Resources.DataGridFilterFlyout_Condition);
        textRows[1].Children.OfType<TextBlock>().Single().Text.ShouldBe(KubeUI.Avalonia.Assets.Resources.DataGridFilterFlyout_Value);
        textPanel.GetVisualDescendants().OfType<ComboBox>().First().HorizontalAlignment.ShouldBe(HorizontalAlignment.Stretch);
        textRows[1].Children.OfType<TextBox>().Single().HorizontalAlignment.ShouldBe(HorizontalAlignment.Stretch);

        var numericCluster = await CreateClusterAsync();
        var numericVm = GetRequiredService<ResourceListViewModel<Corev1Event>>();
        numericVm.Initialize(numericCluster);
        var numericView = GetRequiredService<ResourceListView>();
        numericView.DataContext = numericVm;
        var numericWindow = CreateWindow(content: numericView);
        numericWindow.Show();

        var numericColumn = numericVm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), "Count", StringComparison.Ordinal));
        var numericFlyout = numericColumn.FilterFlyout.ShouldBeOfType<Flyout>();
        numericFlyout.ShowAt(numericView);
        Dispatcher.UIThread.RunJobs();
        var numericContent = numericFlyout.Content.ShouldBeOfType<NumericFilterFlyoutView>();
        var numericRows = numericContent.Content.ShouldBeOfType<StackPanel>().Children.OfType<Grid>().ToList();

        numericRows.Count.ShouldBeGreaterThanOrEqualTo(3);

        var numericValueRow = numericRows[1];
        var numericRangeRow = numericRows[2];
        var numericValueInput = numericValueRow.Children.OfType<NumericUpDown>().Single();
        var numericRangeInput = numericRangeRow.Children.OfType<NumericUpDown>().Single();

        Grid.GetColumn(numericValueInput).ShouldBe(1);
        Grid.GetColumn(numericRangeInput).ShouldBe(1);
        numericValueInput.HorizontalAlignment.ShouldBe(HorizontalAlignment.Stretch);
        numericRangeInput.HorizontalAlignment.ShouldBe(HorizontalAlignment.Stretch);
        numericValueRow.Children.OfType<TextBlock>().Single().Width.ShouldBe(numericRangeRow.Children.OfType<TextBlock>().Single().Width);
        numericRows[0].Children.OfType<TextBlock>().Single().Text.ShouldBe(KubeUI.Avalonia.Assets.Resources.DataGridFilterFlyout_Condition);

        var dateCluster = await CreateClusterAsync();
        var dateVm = GetRequiredService<ResourceListViewModel<Corev1Event>>();
        dateVm.Initialize(dateCluster);
        var dateView = GetRequiredService<ResourceListView>();
        dateView.DataContext = dateVm;
        var dateWindow = CreateWindow(content: dateView);
        dateWindow.Show();

        var dateColumn = dateVm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), "Last Seen", StringComparison.Ordinal));
        var dateFlyout = dateColumn.FilterFlyout.ShouldBeOfType<Flyout>();
        dateFlyout.ShowAt(dateView);
        Dispatcher.UIThread.RunJobs();
        var dateContent = dateFlyout.Content.ShouldBeOfType<DateFilterFlyoutView>();
        var datePanel = dateContent.Content.ShouldBeOfType<StackPanel>();
        var dateRows = datePanel.Children.OfType<Grid>().ToList();
        dateRows[0].Children.OfType<TextBlock>().Single().Text.ShouldBe(KubeUI.Avalonia.Assets.Resources.DataGridFilterFlyout_Condition);
        dateRows[1].Children.OfType<TextBlock>().Single().Text.ShouldBe(KubeUI.Avalonia.Assets.Resources.DataGridFilterFlyout_Value);
        datePanel.GetVisualDescendants().OfType<NumericUpDown>().Count().ShouldBe(1);
        datePanel.GetVisualDescendants().OfType<ComboBox>().Count().ShouldBeGreaterThanOrEqualTo(2);
        datePanel.GetVisualDescendants().OfType<ComboBox>().All(combo => combo.HorizontalAlignment == HorizontalAlignment.Stretch).ShouldBeTrue();

        var enumColumnDefinition = new TestEnumColumnDefinition();
        var enumDataGridColumn = new DataGridControlTemplateColumnDefinition();
        var enumFlyout = flyoutFactory.Create(enumColumnDefinition, enumDataGridColumn, new FilteringModel()).ShouldBeOfType<Flyout>();
        var enumHost = new Button();
        var enumWindow = CreateWindow(content: enumHost);
        enumWindow.Show();
        enumFlyout.ShowAt(enumHost);
        Dispatcher.UIThread.RunJobs();
        var enumContent = enumFlyout.Content.ShouldBeOfType<EnumFilterFlyoutView>();
        var enumPanel = enumContent.Content.ShouldBeOfType<StackPanel>();
        var enumRows = enumPanel.Children.OfType<Grid>().ToList();
        enumRows.Count.ShouldBe(2);
        enumRows[0].Children.OfType<TextBlock>().Single().Text.ShouldBe(KubeUI.Avalonia.Assets.Resources.DataGridFilterFlyout_Condition);
        enumRows[1].Children.OfType<TextBlock>().Single().Text.ShouldBe(KubeUI.Avalonia.Assets.Resources.DataGridFilterFlyout_Value);
        enumPanel.GetVisualDescendants().OfType<ComboBox>().Count().ShouldBe(2);
        enumPanel.GetVisualDescendants().OfType<ComboBox>().All(combo => combo.HorizontalAlignment == HorizontalAlignment.Stretch).ShouldBeTrue();
    }

    [AvaloniaFact(DisplayName = "Resource list numeric and date filters support comparison operators")]
    public async Task resource_list_numeric_and_date_filters_support_comparison_operators()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();
        var filterService = GetRequiredService<DataGridColumnFilterService>();

        var vm = GetRequiredService<ResourceListViewModel<Corev1Event>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        var countColumn = vm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), "Count", StringComparison.Ordinal));
        var lastSeenColumn = vm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), "Last Seen", StringComparison.Ordinal));

        FilteringDescriptor GetDescriptorForColumn(DataGridColumnDefinition column)
            => vm.FilteringModel.Descriptors.First(descriptor =>
                ReferenceEquals(descriptor.ColumnId, column) || Equals(descriptor.ColumnId, column));

        filterService.ApplyNumericFilter(vm.FilteringModel, countColumn, GetNumericOperator(FilteringOperator.GreaterThan), 5d, null);
        vm.FilteringModel.Descriptors.Count(descriptor => ReferenceEquals(descriptor.ColumnId, countColumn) || Equals(descriptor.ColumnId, countColumn)).ShouldBe(1);
        var numericDescriptor = GetDescriptorForColumn(countColumn);
        numericDescriptor.Operator.ShouldBe(FilteringOperator.GreaterThan);
        numericDescriptor.Value.ShouldBe(5d);

        filterService.ApplyNumericFilter(vm.FilteringModel, countColumn, GetNumericOperator(FilteringOperator.Between), 2d, 8d);
        vm.FilteringModel.Descriptors.Count(descriptor => ReferenceEquals(descriptor.ColumnId, countColumn) || Equals(descriptor.ColumnId, countColumn)).ShouldBe(1);
        numericDescriptor = GetDescriptorForColumn(countColumn);
        numericDescriptor.Operator.ShouldBe(FilteringOperator.Between);
        numericDescriptor.Values.ShouldNotBeNull();
        numericDescriptor.Values.Count.ShouldBe(2);
        numericDescriptor.Values[0].ShouldBe(2d);
        numericDescriptor.Values[1].ShouldBe(8d);

        filterService.ApplyNumericFilter(
            vm.FilteringModel,
            countColumn,
            ResourceListFilterFlyoutOptions.NumericOperators.First(option => option.CustomId == FilterOperatorId.NumericNotBetween),
            2d,
            8d);
        vm.FilteringModel.Descriptors.Count(descriptor => ReferenceEquals(descriptor.ColumnId, countColumn) || Equals(descriptor.ColumnId, countColumn)).ShouldBe(1);
        numericDescriptor = GetDescriptorForColumn(countColumn);
        numericDescriptor.Operator.ShouldBe(FilteringOperator.Custom);
        numericDescriptor.PropertyPath.ShouldBe(FilterOperatorIdCatalog.GetDescriptorKey(FilterOperatorId.NumericNotBetween));
        numericDescriptor.Predicate.ShouldNotBeNull();
        numericDescriptor.Values.ShouldNotBeNull();
        numericDescriptor.Values.Count.ShouldBe(2);

        var beforeDateFilter = DateTimeOffset.UtcNow;
        var days = GetDateRelativeUnit(vm, 2);
        filterService.ApplyDateFilter(vm.FilteringModel, lastSeenColumn, lastSeenColumn.ValueType, GetDateOperator(FilteringOperator.GreaterThan), 5d, days);
        vm.FilteringModel.Descriptors.Count(descriptor => ReferenceEquals(descriptor.ColumnId, lastSeenColumn) || Equals(descriptor.ColumnId, lastSeenColumn)).ShouldBe(1);
        var dateDescriptor = GetDescriptorForColumn(lastSeenColumn);
        dateDescriptor.Operator.ShouldBe(FilteringOperator.GreaterThan);
        dateDescriptor.Value.ShouldNotBeNull();
        var expectedThreshold = beforeDateFilter.AddDays(-5);
        var actualThreshold = ToDateTimeOffset(dateDescriptor.Value!);
        Math.Abs((actualThreshold - expectedThreshold).TotalSeconds).ShouldBeLessThan(10);

        var hours = GetDateRelativeUnit(vm, 1);
        filterService.ApplyDateFilter(vm.FilteringModel, lastSeenColumn, lastSeenColumn.ValueType, GetDateOperator(FilteringOperator.LessThan), 12d, hours);
        vm.FilteringModel.Descriptors.Count(descriptor => ReferenceEquals(descriptor.ColumnId, lastSeenColumn) || Equals(descriptor.ColumnId, lastSeenColumn)).ShouldBe(1);
        dateDescriptor = GetDescriptorForColumn(lastSeenColumn);
        dateDescriptor.Operator.ShouldBe(FilteringOperator.LessThan);
        dateDescriptor.Value.ShouldNotBeNull();
        expectedThreshold = beforeDateFilter.AddHours(-12);
        actualThreshold = ToDateTimeOffset(dateDescriptor.Value!);
        Math.Abs((actualThreshold - expectedThreshold).TotalSeconds).ShouldBeLessThan(10);

        filterService.ApplyDateFilter(
            vm.FilteringModel,
            lastSeenColumn,
            lastSeenColumn.ValueType,
            ResourceListFilterFlyoutOptions.DateOperators.First(option => option.CustomId == FilterOperatorId.DateNotNewerThan),
            5d,
            days);
        vm.FilteringModel.Descriptors.Count(descriptor => ReferenceEquals(descriptor.ColumnId, lastSeenColumn) || Equals(descriptor.ColumnId, lastSeenColumn)).ShouldBe(1);
        dateDescriptor = GetDescriptorForColumn(lastSeenColumn);
        dateDescriptor.Operator.ShouldBe(FilteringOperator.Custom);
        dateDescriptor.PropertyPath.ShouldBe(FilterOperatorIdCatalog.GetDescriptorKey(FilterOperatorId.DateNotNewerThan));
        dateDescriptor.Predicate.ShouldNotBeNull();
    }

    [AvaloniaFact(DisplayName = "Resource list filters update the live view")]
    public async Task resource_list_filters_update_the_live_view()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();
        var filterService = GetRequiredService<DataGridColumnFilterService>();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        var pods = new[]
        {
            Pod("ns", "alpha"),
            Pod("ns", "beta"),
            Pod("ns", "gamma"),
        };

        foreach (var pod in pods)
        {
            await AddOrUpdateAsync(cluster, pod);
        }

        Dispatcher.UIThread.RunJobs();
        vm.View.Count.ShouldBe(3);

        var nameColumn = vm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), "Name", StringComparison.Ordinal));
        filterService.ApplyTextFilter(vm.FilteringModel, nameColumn, GetTextOperator(FilteringOperator.Contains), "alp");
        Dispatcher.UIThread.RunJobs();

        vm.View.Count.ShouldBe(1);
        ((V1Pod)vm.View[0]).Name().ShouldBe("alpha");

        filterService.ApplyTextFilter(
            vm.FilteringModel,
            nameColumn,
            ResourceListFilterFlyoutOptions.TextOperators.First(option => option.CustomId == FilterOperatorId.TextNotContains),
            "alp");
        Dispatcher.UIThread.RunJobs();

        vm.View.Count.ShouldBe(2);
        vm.View.OfType<V1Pod>().Select(pod => pod.Name()).ShouldBe(["beta", "gamma"]);

        var countCluster = await CreateClusterAsync();
        var countVm = GetRequiredService<ResourceListViewModel<Corev1Event>>();
        countVm.Initialize(countCluster);

        var countView = GetRequiredService<ResourceListView>();
        countView.DataContext = countVm;

        var countWindow = CreateWindow(content: countView);
        countWindow.Show();

        var older = new Corev1Event
        {
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = "ns",
                Name = "older",
                CreationTimestamp = DateTime.UtcNow.AddHours(-5)
            },
            Count = 1,
            LastTimestamp = DateTime.UtcNow.AddHours(-5)
        };

        var newer = new Corev1Event
        {
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = "ns",
                Name = "newer",
                CreationTimestamp = DateTime.UtcNow.AddMinutes(-10)
            },
            Count = 2,
            LastTimestamp = DateTime.UtcNow.AddMinutes(-10)
        };

        await AddOrUpdateAsync(countCluster, older);
        await AddOrUpdateAsync(countCluster, newer);
        Dispatcher.UIThread.RunJobs();
        countVm.View.Count.ShouldBe(2);

        var countColumn = countVm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), "Count", StringComparison.Ordinal));
        filterService.ApplyNumericFilter(countVm.FilteringModel, countColumn, GetNumericOperator(FilteringOperator.GreaterThan), 0d, null);
        Dispatcher.UIThread.RunJobs();
        countVm.View.Count.ShouldBe(2);

        var lastSeenColumn = countVm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), "Last Seen", StringComparison.Ordinal));
        var hours = GetDateRelativeUnit(countVm, 1);
        filterService.ApplyDateFilter(countVm.FilteringModel, lastSeenColumn, lastSeenColumn.ValueType, GetDateOperator(FilteringOperator.GreaterThan), 1d, hours);
        Dispatcher.UIThread.RunJobs();

        countVm.View.Count.ShouldBe(1);
        ((Corev1Event)countVm.View[0]).Name().ShouldBe("newer");
    }

    [AvaloniaFact(DisplayName = "Text filter flyout apply command updates the live view")]
    public async Task text_filter_flyout_apply_command_updates_the_live_view()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("ns", "alpha"));
        await AddOrUpdateAsync(cluster, Pod("ns", "beta"));
        await AddOrUpdateAsync(cluster, Pod("ns", "gamma"));
        Dispatcher.UIThread.RunJobs();

        var nameColumn = vm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), "Name", StringComparison.Ordinal));
        var flyout = (Flyout)nameColumn.FilterFlyout!;
        flyout.ShowAt(view);
        Dispatcher.UIThread.RunJobs();
        var flyoutContext = flyout.Content.ShouldBeOfType<TextFilterFlyoutView>().DataContext.ShouldBeOfType<TextFilterFlyoutContext>();

        flyoutContext.SelectedOperator = ResourceListFilterFlyoutOptions.TextOperators.First(option => option.Operator == FilteringOperator.Contains && (option.CustomId is null || !FilterOperatorIdCatalog.UsesCustomDescriptor(option.CustomId.Value)));
        flyoutContext.Query = "alp";
        flyoutContext.ApplyCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        vm.View.Count.ShouldBe(1);
        ((V1Pod)vm.View[0]).Name().ShouldBe("alpha");

        flyoutContext.SelectedOperator = ResourceListFilterFlyoutOptions.TextOperators.First(option => option.CustomId == FilterOperatorId.TextNotContains);
        flyoutContext.Query = "alp";
        flyoutContext.ApplyCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        vm.View.Count.ShouldBe(2);
        vm.View.OfType<V1Pod>().Select(pod => pod.Name()).ShouldBe(["beta", "gamma"]);
    }

    [AvaloniaFact(DisplayName = "Namespace filter preserves selection when included")]
    public async Task namespace_filter_preserves_selection_when_included()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("ns1", "a"));
        await AddOrUpdateAsync(cluster, Pod("ns2", "b"));

        vm.SelectionModel.Select(0);

        cluster.SelectedNamespaces.Add(NamespaceResource("ns1"));
        Dispatcher.UIThread.RunJobs();

        vm.SelectedItem.ShouldNotBeNull();
        vm.SelectedItem!.Namespace().ShouldBe("ns1");
        vm.SelectedItem.Name().ShouldBe("a");
    }

    [AvaloniaFact(DisplayName = "Namespace filter selects remaining item when selection filtered out")]
    public async Task namespace_filter_selects_remaining_item_when_selection_filtered_out()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("ns1", "a"));
        await AddOrUpdateAsync(cluster, Pod("ns2", "b"));
        await AddOrUpdateAsync(cluster, Pod("ns3", "c"));
        await AddOrUpdateAsync(cluster, Pod("ns4", "d"));
        await AddOrUpdateAsync(cluster, Pod("ns5", "e"));

        vm.SelectionModel.Select(1);
        vm.SelectedItem.ShouldNotBeNull();
        vm.SelectedItem!.Namespace().ShouldBe("ns2");

        cluster.SelectedNamespaces.Add(NamespaceResource("ns4"));
        Dispatcher.UIThread.RunJobs();

        vm.SelectionModel.SelectedIndexes.ShouldBe([0]);
        vm.SelectedItem.ShouldNotBeNull();
        vm.SelectedItem!.Namespace().ShouldBe("ns4");
        vm.SelectedItem.Name().ShouldBe("d");

        var menuItem = vm.ContextMenuItems.FirstOrDefault(x => x.Header == "View");
        menuItem.ShouldNotBeNull();

        var parameters = menuItem!.CommandParameter as IList;
        parameters.ShouldNotBeNull();
        parameters!.Count.ShouldBe(1);
        var selected = parameters[0].ShouldBeOfType<V1Pod>();
        selected.Namespace().ShouldBe("ns4");
        selected.Name().ShouldBe("d");
    }

    [AvaloniaFact(DisplayName = "Namespace filter updates context menu selection")]
    public async Task namespace_filter_updates_context_menu_selection()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        var podA = Pod("ns1", "a");
        podA.Spec = new V1PodSpec
        {
            Containers = [new V1Container { Name = "a-container" }]
        };
        var podB = Pod("ns2", "b");
        podB.Spec = new V1PodSpec
        {
            Containers = [new V1Container { Name = "b-container" }]
        };
        var podC = Pod("ns3", "c");
        podC.Spec = new V1PodSpec
        {
            Containers = [new V1Container { Name = "c-container" }]
        };
        var podD = Pod("ns4", "d");
        podD.Spec = new V1PodSpec
        {
            Containers = [new V1Container { Name = "d-container" }]
        };
        var podE = Pod("ns5", "e");
        podE.Spec = new V1PodSpec
        {
            Containers = [new V1Container { Name = "e-container" }]
        };

        await AddOrUpdateAsync(cluster, podA);
        await AddOrUpdateAsync(cluster, podB);
        await AddOrUpdateAsync(cluster, podC);
        await AddOrUpdateAsync(cluster, podD);
        await AddOrUpdateAsync(cluster, podE);

        vm.SelectionModel.Select(1);

        cluster.SelectedNamespaces.Add(NamespaceResource("ns4"));
        Dispatcher.UIThread.RunJobs();

        var portForwardMenu = vm.ContextMenuItems.FirstOrDefault(x => x.Header == "Port Forwarding");
        portForwardMenu.ShouldNotBeNull();

        var containers = portForwardMenu!.Items?.ToList();
        containers.Count.ShouldBe(1);
        containers[0].Header.ShouldBe("d-container");
    }

    [AvaloniaFact(DisplayName = "Resource list enum filters render a selector")]
    public async Task resource_list_enum_filters_render_a_selector()
    {
        var filterService = GetRequiredService<DataGridColumnFilterService>();
        var flyoutFactory = GetRequiredService<DataGridColumnFilterFlyoutFactory>();
        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        var cluster = await CreateClusterAsync();
        vm.Initialize(cluster);

        var column = new TestEnumColumnDefinition();
        var dataGridColumn = new DataGridControlTemplateColumnDefinition();

        var flyout = flyoutFactory.Create(column, dataGridColumn, vm.FilteringModel).ShouldBeOfType<Flyout>();
        var host = new Button();
        var window = CreateWindow(content: host);
        window.Show();
        flyout.ShowAt(host);
        Dispatcher.UIThread.RunJobs();
        var content = flyout.Content.ShouldBeOfType<EnumFilterFlyoutView>();

        var enumComboBoxes = content.Content.ShouldBeOfType<StackPanel>().GetVisualDescendants().OfType<ComboBox>().ToList();
        enumComboBoxes.Count.ShouldBe(2);
        enumComboBoxes[0].ItemsSource.ShouldNotBeNull();
        enumComboBoxes[0].ItemsSource.OfType<object>().Count().ShouldBe(2);
        enumComboBoxes[1].ItemsSource.ShouldNotBeNull();
        enumComboBoxes[1].ItemsSource.OfType<object>().Count().ShouldBe(4);

        filterService.ApplyEnumFilter(vm.FilteringModel, dataGridColumn, GetEnumOperator(FilteringOperator.Equals), TestFilterStatus.Running);

        vm.FilteringModel.Descriptors.Count.ShouldBe(1);
        var descriptor = vm.FilteringModel.Descriptors[0];
        descriptor.Operator.ShouldBe(FilteringOperator.Equals);
        descriptor.Value.ShouldBe(TestFilterStatus.Running);
        ReferenceEquals(descriptor.ColumnId, dataGridColumn).ShouldBeTrue();

        filterService.ApplyEnumFilter(vm.FilteringModel, dataGridColumn, GetEnumOperator(FilteringOperator.NotEquals), TestFilterStatus.Failed);
        vm.FilteringModel.Descriptors.Count.ShouldBe(1);
        descriptor = vm.FilteringModel.Descriptors[0];
        descriptor.Operator.ShouldBe(FilteringOperator.NotEquals);
        descriptor.Value.ShouldBe(TestFilterStatus.Failed);
    }

    [AvaloniaFact(DisplayName = "Namespace filter is linked to cluster by default")]
    public async Task namespace_filter_is_linked_to_cluster_by_default()
    {
        var cluster = await CreateClusterAsync();
        cluster.SelectedNamespaces.Add(NamespaceResource("team-a"));

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        vm.IsNamespaceSelectionLinked.ShouldBeTrue();
        ReferenceEquals(vm.SelectedNamespaces, cluster.SelectedNamespaces).ShouldBeTrue();

        cluster.SelectedNamespaces.Add(NamespaceResource("team-b"));
        Dispatcher.UIThread.RunJobs();

        vm.SelectedNamespaces.Select(x => x.Name()).ShouldBe(["team-a", "team-b"]);
        GetNamespaceFilterValues(vm).ShouldBe(["team-a", "team-b"]);
    }

    [AvaloniaFact(DisplayName = "Namespace filter can be decoupled from cluster selection")]
    public async Task namespace_filter_can_be_decoupled_from_cluster_selection()
    {
        var cluster = await CreateClusterAsync();
        cluster.SelectedNamespaces.Add(NamespaceResource("team-a"));

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        vm.IsNamespaceSelectionLinked = false;
        Dispatcher.UIThread.RunJobs();

        ReferenceEquals(vm.SelectedNamespaces, cluster.SelectedNamespaces).ShouldBeFalse();
        vm.SelectedNamespaces.Select(x => x.Name()).ShouldBe(["team-a"]);

        cluster.SelectedNamespaces.Add(NamespaceResource("team-b"));
        Dispatcher.UIThread.RunJobs();

        vm.SelectedNamespaces.Select(x => x.Name()).ShouldBe(["team-a"]);

        vm.SelectedNamespaces.Add(NamespaceResource("team-c"));
        Dispatcher.UIThread.RunJobs();

        cluster.SelectedNamespaces.Select(x => x.Name()).ShouldBe(["team-a", "team-b"]);
        GetNamespaceFilterValues(vm).ShouldBe(["team-a", "team-c"]);
    }

    [AvaloniaFact(DisplayName = "Namespace filter relinks back to cluster selection")]
    public async Task namespace_filter_relinks_back_to_cluster_selection()
    {
        var cluster = await CreateClusterAsync();
        cluster.SelectedNamespaces.Add(NamespaceResource("team-a"));

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);
        vm.IsNamespaceSelectionLinked = false;
        vm.SelectedNamespaces.Clear();
        vm.SelectedNamespaces.Add(NamespaceResource("team-local"));
        Dispatcher.UIThread.RunJobs();

        vm.IsNamespaceSelectionLinked = true;
        Dispatcher.UIThread.RunJobs();

        ReferenceEquals(vm.SelectedNamespaces, cluster.SelectedNamespaces).ShouldBeTrue();
        vm.SelectedNamespaces.Select(x => x.Name()).ShouldBe(["team-a"]);
        GetNamespaceFilterValues(vm).ShouldBe(["team-a"]);
    }

    [AvaloniaFact(DisplayName = "Clearing namespace column filter preserves namespace scope filter")]
    public async Task clearing_namespace_column_filter_preserves_namespace_scope_filter()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();
        var filterService = GetRequiredService<DataGridColumnFilterService>();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("ns1", "a"));
        await AddOrUpdateAsync(cluster, Pod("ns2", "b"));
        await AddOrUpdateAsync(cluster, Pod("ns3", "c"));

        cluster.SelectedNamespaces.Add(NamespaceResource("ns1"));
        cluster.SelectedNamespaces.Add(NamespaceResource("ns2"));
        Dispatcher.UIThread.RunJobs();

        vm.View.Count.ShouldBe(2);
        GetNamespaceFilterValues(vm).ShouldBe(["ns1", "ns2"]);

        var namespaceColumn = vm.ColumnDefinitions.First(column => string.Equals(column.ColumnKey?.ToString(), "namespace", StringComparison.OrdinalIgnoreCase));
        filterService.ApplyTextFilter(vm.FilteringModel, namespaceColumn, GetTextOperator(FilteringOperator.Contains), "ns1");
        Dispatcher.UIThread.RunJobs();

        vm.FilteringModel.Descriptors.Count.ShouldBe(2);
        vm.View.Count.ShouldBe(1);
        ((V1Pod)vm.View[0]).Namespace().ShouldBe("ns1");

        filterService.ClearColumnFilter(vm.FilteringModel, namespaceColumn);
        Dispatcher.UIThread.RunJobs();

        vm.FilteringModel.Descriptors.Count.ShouldBe(1);
        GetNamespaceFilterValues(vm).ShouldBe(["ns1", "ns2"]);
        vm.View.Count.ShouldBe(2);
    }

    [AvaloniaFact(DisplayName = "Pod-specific actions are hidden for multi-select")]
    public async Task pod_specific_actions_are_hidden_for_multi_select()
    {
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var podA = Pod("ns1", "a");
        podA.Spec = new V1PodSpec
        {
            Containers = [new V1Container { Name = "a-container" }]
        };
        await AddOrUpdateAsync(cluster, podA);

        var podB = Pod("ns2", "b");
        podB.Spec = new V1PodSpec
        {
            Containers = [new V1Container { Name = "b-container" }]
        };
        await AddOrUpdateAsync(cluster, podB);

        vm.SelectionModel.Select(0);
        vm.SelectionModel.Select(1);

        var headers = vm.ContextMenuItems.Select(x => x.Header).ToList();

        headers.ShouldNotContain("View Console");
        headers.ShouldNotContain("View Logs");
        headers.ShouldNotContain("Port Forwarding");
    }

    [AvaloniaFact(DisplayName = "Delete Resource")]
    public async Task delete_resource()
    {
        var window = CreateWindow();

        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("ns1", "a"));

        vm.View.Count().ShouldBe(1);

        await cluster.DeleteResource(Pod("ns1", "a"));
        Dispatcher.UIThread.RunJobs();

        vm.View.Count().ShouldBe(0);
    }

    [AvaloniaFact(DisplayName = "Reattach keeps only saved sort descriptors")]
    public async Task reattach_keeps_only_saved_sort_descriptors()
    {
        var factory = GetRequiredService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var dockControl = new DockControl
        {
            Layout = layout,
        };

        var window = CreateWindow(content: dockControl);
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Namespace>>();
        vm.Initialize(cluster);

        window.Show();

        var otherDockable = GetRequiredService<AboutViewModel>();
        otherDockable.Id = nameof(AboutViewModel);

        factory.AddToDocuments(vm);
        factory.AddToDocuments(otherDockable);

        var nsA = NamespaceResource("a");
        nsA.Metadata.Labels = new Dictionary<string, string> { { "env", "prod" } };
        var nsB = NamespaceResource("b");
        nsB.Metadata.Labels = new Dictionary<string, string> { { "env", "dev" } };
        var nsC = NamespaceResource("c");
        nsC.Metadata.Labels = new Dictionary<string, string> { { "env", "dev" } };

        await AddOrUpdateAsync(cluster, nsA);
        await AddOrUpdateAsync(cluster, nsB);
        await AddOrUpdateAsync(cluster, nsC);

        var labelsColumn = vm.ColumnDefinitions.First(x => Equals(x.ColumnKey, "name"));

        vm.SortingModel.Clear();

        vm.SortingModel.SetOrUpdate(new(labelsColumn, ListSortDirection.Descending, null, labelsColumn.CustomSortComparer));

        Dispatcher.UIThread.RunJobs();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        Dispatcher.UIThread.RunJobs();

        var view = WaitForValue(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        view.ShouldNotBeNull();

        vm.View.ElementAt(0).ShouldBeOfType<V1Namespace>().Name().ShouldBe("c");
        vm.View.ElementAt(1).ShouldBeOfType<V1Namespace>().Name().ShouldBe("b");
        vm.View.ElementAt(2).ShouldBeOfType<V1Namespace>().Name().ShouldBe("a");
        vm.SortingModel.Descriptors.Count.ShouldBe(1);
        ((DataGridControlTemplateColumnDefinition)(vm.SortingModel.Descriptors[0].ColumnId)).ColumnKey.ShouldBe("name");

        factory.SetActiveDockable(otherDockable);
        factory.SetFocusedDockable(documents, otherDockable);
        Dispatcher.UIThread.RunJobs();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        Dispatcher.UIThread.RunJobs();

        var restoredView = WaitForValue(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        restoredView.ShouldNotBeNull();

        vm.View.ElementAt(0).ShouldBeOfType<V1Namespace>().Name().ShouldBe("c");
        vm.View.ElementAt(1).ShouldBeOfType<V1Namespace>().Name().ShouldBe("b");
        vm.View.ElementAt(2).ShouldBeOfType<V1Namespace>().Name().ShouldBe("a");
        vm.SortingModel.Descriptors.Count.ShouldBe(1);
        ((DataGridControlTemplateColumnDefinition)(vm.SortingModel.Descriptors[0].ColumnId)).ColumnKey.ShouldBe("name");
    }

    [AvaloniaFact(DisplayName = "Reattach preserves DataGrid scroll offset")]
    public async Task reattach_preserves_datagrid_scroll_offset()
    {
        var factory = GetRequiredService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var dockControl = new DockControl
        {
            Layout = layout,
        };

        var window = CreateWindow(height: 900, content: dockControl);
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        window.Show();

        var otherDockable = GetRequiredService<AboutViewModel>();
        otherDockable.Id = nameof(AboutViewModel);

        factory.AddToDocuments(vm);
        factory.AddToDocuments(otherDockable);

        // Seed many items so vertical scrolling appears
        for (var i = 0; i < 400; i++)
        {
            await AddOrUpdateAsync(cluster, Pod("ns", i.ToString()));
        }

        Dispatcher.UIThread.RunJobs();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        Dispatcher.UIThread.RunJobs();

        var view = WaitForValue(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        view.ShouldNotBeNull();

        var grid = view!.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();

        var scrollViewer = grid.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        scrollViewer.ShouldNotBeNull();

        // Wait until content is scrollable
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < 3000)
        {
            Dispatcher.UIThread.RunJobs();
            if (scrollViewer.Extent.Height > scrollViewer.Viewport.Height)
                break;
            System.Threading.Thread.Sleep(10);
        }

        scrollViewer.Extent.Height.ShouldBeGreaterThan(scrollViewer.Viewport.Height);

        var targetOffset = new Vector(0, Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
        scrollViewer.Offset = targetOffset;
        Dispatcher.UIThread.RunJobs();

        // switch away to trigger capture
        factory.SetActiveDockable(otherDockable);
        factory.SetFocusedDockable(documents, otherDockable);
        Dispatcher.UIThread.RunJobs();

        vm.DataGridRuntimeState.ShouldNotBeNull();

        // switch back and ensure restore
        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        Dispatcher.UIThread.RunJobs();

        var restoredView = WaitForValue(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        restoredView.ShouldNotBeNull();

        var restoredGrid = restoredView!.FindControl<DataGrid>("PART_Grid");
        restoredGrid.ShouldNotBeNull();

        var restoredScrollViewer = restoredGrid.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        restoredScrollViewer.ShouldNotBeNull();

        // Wait until restored grid is scrollable
        sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < 3000)
        {
            Dispatcher.UIThread.RunJobs();
            if (restoredScrollViewer.Extent.Height > restoredScrollViewer.Viewport.Height)
                break;
            System.Threading.Thread.Sleep(10);
        }

        Dispatcher.UIThread.RunJobs();
        restoredScrollViewer.Offset.Y.ShouldBe(0);
        ReferenceEquals(grid, restoredGrid).ShouldBeFalse();
        vm.DataGridRuntimeState.ShouldNotBeNull();

    }

    [AvaloniaFact(DisplayName = "Reattach captures runtime state and restores on reattach")]
    public async Task reattach_captures_runtime_state_and_restores_on_reattach()
    {
        var factory = GetRequiredService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var dockControl = new DockControl
        {
            Layout = layout,
        };

        var window = CreateWindow(content: dockControl);
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Namespace>>();
        vm.Initialize(cluster);

        window.Show();

        var otherDockable = GetRequiredService<AboutViewModel>();
        otherDockable.Id = nameof(AboutViewModel);

        factory.AddToDocuments(vm);
        factory.AddToDocuments(otherDockable);

        var nsA = NamespaceResource("a");
        var nsB = NamespaceResource("b");
        var nsC = NamespaceResource("c");

        await AddOrUpdateAsync(cluster, nsA);
        await AddOrUpdateAsync(cluster, nsB);
        await AddOrUpdateAsync(cluster, nsC);

        var labelsColumn = vm.ColumnDefinitions.First(x => Equals(x.ColumnKey, "labels"));

        vm.SortingModel.Clear();

        vm.SortingModel.SetOrUpdate(new(labelsColumn, ListSortDirection.Descending, null, labelsColumn.CustomSortComparer));

        Dispatcher.UIThread.RunJobs();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        Dispatcher.UIThread.RunJobs();

        var view = WaitForValue(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        view.ShouldNotBeNull();

        // switch away to trigger capture
        factory.SetActiveDockable(otherDockable);
        factory.SetFocusedDockable(documents, otherDockable);
        Dispatcher.UIThread.RunJobs();

        // runtime snapshot should be captured on VM by behavior
        vm.DataGridRuntimeState.ShouldNotBeNull();

        // switch back and ensure restore
        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        Dispatcher.UIThread.RunJobs();

        var restoredView = WaitForValue(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        restoredView.ShouldNotBeNull();

        vm.View.ElementAt(0).ShouldBeOfType<V1Namespace>().Name().ShouldBe("a");
        vm.View.ElementAt(1).ShouldBeOfType<V1Namespace>().Name().ShouldBe("b");
        vm.View.ElementAt(2).ShouldBeOfType<V1Namespace>().Name().ShouldBe("c");
        vm.SortingModel.Descriptors.Count.ShouldBe(1);
        ((DataGridControlTemplateColumnDefinition)(vm.SortingModel.Descriptors[0].ColumnId)).ColumnKey.ShouldBe("labels");
    }

    [AvaloniaFact(DisplayName = "Namespace filter initializes from selected namespaces")]
    public async Task namespace_filter_initializes_from_selected_namespaces()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        cluster.SelectedNamespaces.Add(NamespaceResource("default"));

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.FilteringModel.Descriptors.Count.ShouldBe(1);
        var descriptor = vm.FilteringModel.Descriptors[0];
        descriptor.Values.Count.ShouldBe(1);
        descriptor.Values[0].ShouldBe("default");
    }

    [AvaloniaFact(DisplayName = "Search query is debounced before filtering view")]
    public async Task search_query_is_debounced_before_filtering_view()
    {
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        await AddOrUpdateAsync(cluster, Pod("ns", "alpha"));
        await AddOrUpdateAsync(cluster, Pod("ns", "beta"));

        vm.View.Count.ShouldBe(2);

        vm.SearchQuery = "alpha";
        Dispatcher.UIThread.RunJobs();

        vm.View.Count.ShouldBe(2);

        await WaitForAsync(() => vm.View.Count == 1);
        vm.View[0].ShouldBeOfType<V1Pod>().Name().ShouldBe("alpha");
    }

    [AvaloniaFact(DisplayName = "Double tap opens property view")]
    public void double_tap_opens_property_view()
    {
        var vm = new FakeDoubleTapResourceListViewModel(Pod("ns", "a"));
        var row = new DataGridRow();

        ResourceListDoubleTapBehavior.Execute(vm, row).ShouldBeTrue();
        vm.ViewInvocations.ShouldBe(1);
    }

    [AvaloniaFact(DisplayName = "Double tap on column header does not open property view")]
    public void double_tap_on_column_header_does_not_open_property_view()
    {
        var vm = new FakeDoubleTapResourceListViewModel(Pod("ns", "a"));

        ResourceListDoubleTapBehavior.Execute(vm, new DataGridColumnHeader()).ShouldBeFalse();
        vm.ViewInvocations.ShouldBe(0);
    }

    [AvaloniaFact(DisplayName = "Double tap on scrollbar does not open property view")]
    public void double_tap_on_scrollbar_does_not_open_property_view()
    {
        var vm = new FakeDoubleTapResourceListViewModel(Pod("ns", "a"));

        ResourceListDoubleTapBehavior.Execute(vm, new ScrollBar()).ShouldBeFalse();
        vm.ViewInvocations.ShouldBe(0);
    }

    private static TView? FindVisibleView<TView>(Visual root, object viewModel) where TView : Visual
    {
        return root.GetVisualDescendants()
            .OfType<TView>()
            .FirstOrDefault(view => view.IsVisible && ReferenceEquals((view as StyledElement)?.DataContext, viewModel));
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 1000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();
            if (predicate())
            {
                return;
            }

            await Task.Delay(25);
        }

        Dispatcher.UIThread.RunJobs();
        predicate().ShouldBeTrue();
    }

    private static T WaitForValue<T>(Func<T?> getter, int timeoutMs = 1000) where T : class
    {
        T? value = null;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            Dispatcher.UIThread.RunJobs();
            value = getter();
            if (value != null)
            {
                return value;
            }

            System.Threading.Thread.Sleep(10);
        }

        value.ShouldNotBeNull();
        return value;
    }

    private static IList<string> GetNamespaceFilterValues<T>(ResourceListViewModel<T> vm)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var descriptor = vm.FilteringModel.Descriptors.FirstOrDefault(x => Equals(x.ColumnId, ResourceListViewModel<T>.NamespaceScopeFilterId));
        descriptor.ShouldNotBeNull();
        descriptor!.Values.ShouldNotBeNull();
        return descriptor.Values.Cast<string>().ToList();
    }

    private static DateRelativeUnit GetDateRelativeUnit<T>(T viewModel, int index)
    {
        return ResourceListFilterFlyoutOptions.DateRelativeUnits[index].Unit;
    }

    private static DateTimeOffset ToDateTimeOffset(object value)
    {
        return value switch
        {
            DateTimeOffset dto => dto,
            DateTime dt => new DateTimeOffset(dt),
            _ => throw new InvalidOperationException($"Unsupported date value type: {value.GetType().FullName}")
        };
    }

    private static FilterOperatorChoice GetTextOperator(FilteringOperator filterOperator)
    {
        return ResourceListFilterFlyoutOptions.TextOperators.First(option => option.Operator == filterOperator && (option.CustomId is null || !FilterOperatorIdCatalog.UsesCustomDescriptor(option.CustomId.Value)));
    }

    private static FilterOperatorChoice GetNumericOperator(FilteringOperator filterOperator)
    {
        return ResourceListFilterFlyoutOptions.NumericOperators.First(option => option.Operator == filterOperator && (option.CustomId is null || !FilterOperatorIdCatalog.UsesCustomDescriptor(option.CustomId.Value)));
    }

    private static FilterOperatorChoice GetDateOperator(FilteringOperator filterOperator)
    {
        return ResourceListFilterFlyoutOptions.DateOperators.First(option => option.Operator == filterOperator && (option.CustomId is null || !FilterOperatorIdCatalog.UsesCustomDescriptor(option.CustomId.Value)));
    }

    private static FilterOperatorChoice GetEnumOperator(FilteringOperator filterOperator)
    {
        return ResourceListFilterFlyoutOptions.EnumOperators.First(option => option.Operator == filterOperator && (option.CustomId is null || !FilterOperatorIdCatalog.UsesCustomDescriptor(option.CustomId.Value)));
    }
}

internal enum TestFilterStatus
{
    Pending,
    Running,
    Failed,
}

internal sealed class TestEnumColumnDefinition : IResourceListColumn
{
    public string Key => "status";
    public string Name => "Status";
    public string? Width => null;
    public KubeUI.Avalonia.Resources.SortDirection Sort { get; set; } = KubeUI.Avalonia.Resources.SortDirection.None;
    public Type CustomControl => typeof(object);
    public Type ItemType => typeof(V1Pod);
    public Type ValueType => typeof(TestFilterStatus);
    public IDataGridColumnValueAccessor ValueAccessor { get; } = new TestEnumValueAccessor();
    public Func<object, IComparable?> SortKey => _ => null;
    public Func<object, string> DisplayValue => _ => string.Empty;

    private sealed class TestEnumValueAccessor : IDataGridColumnValueAccessor
    {
        public Type ItemType => typeof(V1Pod);
        public Type ValueType => typeof(TestFilterStatus);
        public bool CanWrite => false;
        public object GetValue(object item) => TestFilterStatus.Pending;
        public void SetValue(object item, object value) => throw new NotSupportedException();
    }
}

internal sealed class FakeDoubleTapResourceListViewModel : IResourceListViewModel
{
    public FakeDoubleTapResourceListViewModel(object selectedItem)
    {
        var selectionModel = new SelectionModel<object>();
        selectionModel.Source = new[] { selectedItem };
        selectionModel.Select(0);
        SelectionModel = selectionModel;
        ResourceConfig = new FakeDoubleTapResourceConfig(() => ViewInvocations++);
    }

    public int ViewInvocations { get; private set; }

    public ClusterWorkspaceViewModel Cluster { get; set; } = null!;
    public ObservableCollection<V1Namespace> SelectedNamespaces { get; } = [];
    public bool IsNamespaceSelectionLinked { get; set; } = true;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From<V1Pod>();
    public int ItemCount => View.Count;
    public string SearchQuery { get; set; } = string.Empty;
    public ISettingsService SettingsService => TestApp.CurrentServices?.GetRequiredService<ISettingsService>()
        ?? throw new InvalidOperationException("Test services are not initialized.");
    public IResourceConfig ResourceConfig { get; }
    public ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; } = [];
    public IDataGridSortingAdapterFactory SortingAdapterFactory => throw new NotImplementedException();
    public ISortingModel SortingModel { get; set; } = new SortingModel();
    public IDataGridFilteringAdapterFactory FilteringAdapterFactory => throw new NotImplementedException();
    public IFilteringModel FilteringModel { get; set; } = new FilteringModel();
    public ISelectionModel SelectionModel { get; }
    public Func<IList, object, int> ReferenceIndexResolver => (_, _) => -1;
    public IList View => Array.Empty<object>();
    public IEnumerable<MenuItemViewModel> ContextMenuItems => [];
    public ISearchModel SearchModel { get; set; } = new SearchModel();
    public IDataGridSearchAdapterFactory SearchAdapterFactory => throw new NotImplementedException();
    public global::Avalonia.Controls.DataGridState? DataGridRuntimeState { get; set; }
}

internal sealed class FakeDoubleTapResourceConfig : IResourceConfig
{
    private readonly Action _onView;

    public FakeDoubleTapResourceConfig(Action onView)
    {
        _onView = onView;
        ViewCommand = new RelayCommand<IList>(
            execute: items => _onView(),
            canExecute: items => items?.Count == 1);
    }

    public bool IsNamespaced => true;
    public bool CanListAndWatch => true;
    public bool PermissionsLoaded => true;
    public bool ShowNewResource => true;
    public bool IsCustomResource => false;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From<V1Pod>();
    public IList<IResourceListColumn> Columns() => Array.Empty<IResourceListColumn>();
    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public int Order => 0;
    public string Name => "Pods";
    public string? Category => null;
    public IStyle ListStyle() => new global::Avalonia.Styling.Style();
    public Task UpdatePermissions() => Task.CompletedTask;
    public Type Type => typeof(V1Pod);
    public IRelayCommand NewResourceCommand => new RelayCommand(() => { });
    public IRelayCommand<IList> ViewCommand { get; }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
    }
}

