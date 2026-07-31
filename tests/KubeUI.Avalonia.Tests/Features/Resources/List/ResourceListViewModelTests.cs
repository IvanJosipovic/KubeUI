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
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
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
using KubeUI.Avalonia.Controls.DataGridFilters;
using KubeUI.Avalonia.Features.Resources.List.Behaviors;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Documents.About;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Testing;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.List;

public class ResourceListViewModelTests : AvaloniaTestBase, IDisposable
{
    private readonly List<IDisposable> _disposables = [];
    private readonly List<Window> _windows = [];

    public void Dispose()
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

    private async Task<ClusterWorkspace> CreateClusterAsync()
    {
        var scope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!);
        _disposables.Add(scope);
        var cluster = scope.Workspace;
        await cluster.Connect();
        Dispatcher.UIThread.RunJobs();
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

    private static V1Deployment Deployment(string ns, string name)
        => new()
        {
            ApiVersion = V1Deployment.KubeApiVersion,
            Kind = V1Deployment.KubeKind,
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = ns,
                Name = name,
                CreationTimestamp = DateTime.UtcNow,
            }
        };

    private static Corev1Event Event(string ns, string name, DateTime? timestamp = null, int? count = null)
    {
        var actualTimestamp = timestamp ?? DateTime.UtcNow;

        return new()
        {
            ApiVersion = Corev1Event.KubeApiVersion,
            Kind = Corev1Event.KubeKind,
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = ns,
                Name = name,
                CreationTimestamp = actualTimestamp,
            },
            LastTimestamp = actualTimestamp,
            Count = count,
        };
    }

    private static V1Namespace NamespaceResource(string name)
        => new()
        {
            Metadata = new V1ObjectMeta
            {
                Name = name
            }
        };

    private static async Task AddOrUpdateAsync<T>(ClusterWorkspace cluster, T resource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await cluster.Runtime.AddOrUpdateResource(resource);
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

        if (row >= rows.Count)
        {
            return null;
        }

        var dataGridRow = rows[row];
        return GetCellText(grid, dataGridRow, column);
    }

    private static string? GetResourceCellText<T>(DataGrid grid, string name, int column)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        grid.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        var row = GetAllRows(grid)
            .FirstOrDefault(item => item.IsVisible && (item.DataContext as T)?.Name() == name);
        return row is null ? null : GetCellText(grid, row, column);
    }

    private static Point GetRowCenterOnWindow(DataGridRow row, Window window)
    {
        var point = row.TranslatePoint(new Point(row.Bounds.Width / 2, row.Bounds.Height / 2), window);
        point.ShouldNotBeNull();
        return point!.Value;
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
        await WaitForAsync(() => vm.View.Count == 3, timeoutMs: 5000);

        // Select all 3
        vm.SelectionModel.Select(0);
        vm.SelectionModel.Select(1);
        vm.SelectionModel.Select(2);

        await WaitForAsync(
            () => vm.SelectionModel.SelectedIndexes.SequenceEqual([0, 1, 2]),
            timeoutMs: 5000);
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
        await WaitForAsync(() => vm.View.Count == 3, timeoutMs: 5000);

        // Select only middle
        vm.SelectionModel.Select(1);

        await WaitForAsync(
            () => vm.SelectionModel.SelectedIndexes.SequenceEqual([1]),
            timeoutMs: 5000);
        vm.SelectionModel.SelectedIndexes.ShouldBe([1]);

        // Replace 'b' with new instance (same key)
        V1Pod updatedPod = Pod("ns", "b");
        await AddOrUpdateAsync(cluster, updatedPod);

        await WaitForAsync(
            () => vm.View.Count == 3,
            timeoutMs: 5000);
        await WaitForAsync(() => vm.SelectionModel.SelectedIndexes.SequenceEqual([1]), timeoutMs: 5000);

        vm.SelectionModel.SelectedIndexes.ShouldBe([1]);

        vm.SelectedItems.Count.ShouldBe(1);

        vm.SelectedItems[0].Namespace().ShouldBe("ns");
        vm.SelectedItems[0].Name().ShouldBe("b");

        vm.SelectedItem.Namespace().ShouldBe("ns");
        vm.SelectedItem.Name().ShouldBe("b");
    }

    [AvaloniaFact(DisplayName = "Selected item right click populates context menu")]
    public async Task selected_item_right_click_populates_context_menu()
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
        pod.Spec = new V1PodSpec
        {
            Containers = [new V1Container { Name = "container-a" }]
        };

        await AddOrUpdateAsync(cluster, pod);
        await WaitForAsync(() => vm.View.Count == 1, timeoutMs: 5000);

        vm.SelectionModel.Select(0);
        await WaitForAsync(() => GetAllRows(grid).Any(x => x.IsVisible), timeoutMs: 5000);

        var contextMenu = grid!.ContextMenu;
        contextMenu.ShouldNotBeNull();

        var row = GetAllRows(grid).First(x => x.IsVisible);
        var clickPoint = GetRowCenterOnWindow(row, window);
        window.MouseDown(clickPoint, MouseButton.Right);
        window.MouseUp(clickPoint, MouseButton.Right);
        Dispatcher.UIThread.RunJobs();

        var items = contextMenu.ItemsSource as IEnumerable<MenuItemViewModel>;
        items.ShouldNotBeNull();

        var headers = items!.Select(x => x.Title).ToList();
        headers.ShouldContain("View");
    }

    [AvaloniaFact(DisplayName = "First right click enables context menu actions for the clicked row")]
    public async Task first_right_click_enables_context_menu_actions_for_the_clicked_row()
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
        pod.Spec = new V1PodSpec
        {
            Containers = [new V1Container { Name = "container-a" }]
        };

        await AddOrUpdateAsync(cluster, pod);

        var contextMenu = grid!.ContextMenu;
        contextMenu.ShouldNotBeNull();

        var row = GetAllRows(grid).First(x => x.IsVisible);
        var clickPoint = GetRowCenterOnWindow(row, window);

        window.MouseDown(clickPoint, MouseButton.Right);
        window.MouseUp(clickPoint, MouseButton.Right);

        await WaitForAsync(() => vm.SelectionModel.SelectedItems.Count == 1);

        var items = contextMenu.ItemsSource as IEnumerable<MenuItemViewModel>;
        items.ShouldNotBeNull();

            var viewItem = items!.First(item => item.Title == "View");
        var commandParameter = viewItem.CommandParameter as IList;

        commandParameter.ShouldNotBeNull();
        commandParameter!.Count.ShouldBe(1);
        viewItem.Command.ShouldNotBeNull();
        viewItem.Command!.CanExecute(commandParameter).ShouldBeTrue();
    }

    [AvaloniaFact(DisplayName = "Right click context menu targets the clicked row")]
    public void right_click_context_menu_targets_the_clicked_row()
    {
        var podA = Pod("ns", "a");
        var podB = Pod("ns", "b");

        var selectionModel = new SelectionModel<object>
        {
            Source = new object[] { podA, podB }
        };

        selectionModel.Select(0);

        var viewModel = new FakeContextMenuResourceListViewModel(selectionModel);

        var selectedA = ResourceListContextMenuBehavior.ResolveContextMenuItemsSource(viewModel, 0, podA)
            .Cast<V1Pod>()
            .Single();
        selectedA.Name().ShouldBe("a");

        var selectedB = ResourceListContextMenuBehavior.ResolveContextMenuItemsSource(viewModel, 1, podB)
            .Cast<V1Pod>()
            .Single();
        selectedB.Name().ShouldBe("b");
    }

    [AvaloniaFact(DisplayName = "Right click context menu follows the clicked row in the grid")]
    public async Task right_click_context_menu_follows_the_clicked_row_in_the_grid()
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

        var podA = Pod("ns", "a");
        podA.Spec = new V1PodSpec
        {
            Containers = [new V1Container { Name = "container-a" }]
        };

        var podB = Pod("ns", "b");
        podB.Spec = new V1PodSpec
        {
            Containers = [new V1Container { Name = "container-b" }]
        };

        await AddOrUpdateAsync(cluster, podA);
        await AddOrUpdateAsync(cluster, podB);
        await WaitForAsync(() => GetAllRows(grid).Count(row => row.IsVisible) >= 2);

        var contextMenu = grid!.ContextMenu;
        contextMenu.ShouldNotBeNull();

        var rows = GetAllRows(grid).Where(x => x.IsVisible).ToList();
        rows.Count.ShouldBeGreaterThanOrEqualTo(2);

        var rowA = rows.First(row => (row.DataContext as V1Pod)?.Name() == "a");
        var rowB = rows.First(row => (row.DataContext as V1Pod)?.Name() == "b");

        async Task AssertMenuTargetsRowAsync(DataGridRow row, string expectedName)
        {
            var clickPoint = GetRowCenterOnWindow(row, window);
            window.MouseDown(clickPoint, MouseButton.Right);
            window.MouseUp(clickPoint, MouseButton.Right);

            await WaitForAsync(() =>
            {
                var items = contextMenu.ItemsSource as IEnumerable<MenuItemViewModel>;
                if (items is null)
                {
                    return false;
                }

                var viewItem = items.FirstOrDefault(item => item.Title == "View");
                var commandParameter = viewItem?.CommandParameter as IList;
                if (commandParameter is null || commandParameter.Count != 1)
                {
                    return false;
                }

                return commandParameter[0] is V1Pod pod && pod.Name() == expectedName;
            });

            var menuItems = (contextMenu.ItemsSource as IEnumerable<MenuItemViewModel>)!.ToList();
            var viewMenuItem = menuItems.First(item => item.Title == "View");
            var commandItems = (viewMenuItem.CommandParameter as IList)!;

            commandItems.Count.ShouldBe(1);
            ((V1Pod)commandItems[0]!).Name().ShouldBe(expectedName);

            contextMenu.Close();
            Dispatcher.UIThread.RunJobs();
        }

        await AssertMenuTargetsRowAsync(rowA, "a");
        await AssertMenuTargetsRowAsync(rowB, "b");
    }

    [AvaloniaFact(DisplayName = "Multi select right click populates context menu")]
    public async Task multi_select_right_click_populates_context_menu()
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

        await AddOrUpdateAsync(cluster, Pod("ns", "a"));
        await AddOrUpdateAsync(cluster, Pod("ns", "b"));

        vm.SelectionModel.Select(0);
        vm.SelectionModel.Select(1);

        var contextMenu = grid!.ContextMenu;
        contextMenu.ShouldNotBeNull();

        var row = GetAllRows(grid).First(x => x.IsVisible);
        var clickPoint = GetRowCenterOnWindow(row, window);
        window.MouseDown(clickPoint, MouseButton.Right);
        window.MouseUp(clickPoint, MouseButton.Right);
        Dispatcher.UIThread.RunJobs();

        var items = contextMenu.ItemsSource as IEnumerable<MenuItemViewModel>;
        items.ShouldNotBeNull();

            var headers = items!.Select(item => item.Title).ToList();
        headers.ShouldContain("View");
        headers.ShouldContain("Delete");
    }

    [AvaloniaFact(DisplayName = "Multi select right click uses the full selection")]
    public async Task multi_select_right_click_uses_the_full_selection()
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

        var podA = Pod("ns", "a");
        podA.Spec = new V1PodSpec
        {
            Containers = [new V1Container { Name = "container-a" }]
        };

        var podB = Pod("ns", "b");
        podB.Spec = new V1PodSpec
        {
            Containers = [new V1Container { Name = "container-b" }]
        };

        await AddOrUpdateAsync(cluster, podA);
        await AddOrUpdateAsync(cluster, podB);
        await WaitForAsync(() => GetAllRows(grid).Count(row => row.IsVisible) == 2 &&
            GetAllRows(grid).Any(row => row.IsVisible && (row.DataContext as V1Pod)?.Name() == "a"));

        vm.SelectionModel.Select(0);
        vm.SelectionModel.Select(1);

        var contextMenu = grid!.ContextMenu;
        contextMenu.ShouldNotBeNull();

        var row = GetAllRows(grid).First(x => x.IsVisible && (x.DataContext as V1Pod)?.Name() == "a");
        var clickPoint = GetRowCenterOnWindow(row, window);
        window.MouseDown(clickPoint, MouseButton.Right);
        window.MouseUp(clickPoint, MouseButton.Right);
        Dispatcher.UIThread.RunJobs();

        var items = contextMenu.ItemsSource as IEnumerable<MenuItemViewModel>;
        items.ShouldNotBeNull();

            var deleteItem = items!.First(item => item.Title == "Delete");
        var commandParameter = deleteItem.CommandParameter as IList;

        commandParameter.ShouldNotBeNull();
        commandParameter!.Count.ShouldBe(2);
        ((V1Pod)commandParameter[0]!).Name().ShouldBe("a");
        ((V1Pod)commandParameter[1]!).Name().ShouldBe("b");
        deleteItem.Command.ShouldNotBeNull();
        deleteItem.Command!.CanExecute(commandParameter).ShouldBeTrue();

            var viewItem = items.First(item => item.Title == "View");
        viewItem.Command.ShouldNotBeNull();
        viewItem.Command!.CanExecute(commandParameter).ShouldBeFalse();

        contextMenu.Close();
        Dispatcher.UIThread.RunJobs();

        row = GetAllRows(grid).First(x => x.IsVisible && (x.DataContext as V1Pod)?.Name() == "b");
        clickPoint = GetRowCenterOnWindow(row, window);
        window.MouseDown(clickPoint, MouseButton.Right);
        window.MouseUp(clickPoint, MouseButton.Right);
        Dispatcher.UIThread.RunJobs();

        items = contextMenu.ItemsSource as IEnumerable<MenuItemViewModel>;
        items.ShouldNotBeNull();

            deleteItem = items!.First(item => item.Title == "Delete");
        commandParameter = deleteItem.CommandParameter as IList;

        commandParameter.ShouldNotBeNull();
        commandParameter!.Count.ShouldBe(2);
        ((V1Pod)commandParameter[0]!).Name().ShouldBe("a");
        ((V1Pod)commandParameter[1]!).Name().ShouldBe("b");
        deleteItem.Command.ShouldNotBeNull();
        deleteItem.Command!.CanExecute(commandParameter).ShouldBeTrue();

            viewItem = items.First(item => item.Title == "View");
        viewItem.Command.ShouldNotBeNull();
        viewItem.Command!.CanExecute(commandParameter).ShouldBeFalse();
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
        await WaitForAsync(() => vm.View.Count == 3);

        vm.View[0].ShouldBeOfType<Corev1Event>().Name().ShouldBe("c");
        vm.View[1].ShouldBeOfType<Corev1Event>().Name().ShouldBe("b");
        vm.View[2].ShouldBeOfType<Corev1Event>().Name().ShouldBe("a");


        // Select only middle
        vm.SelectionModel.Select(1);

        vm.SelectionModel.SelectedIndexes.ShouldBe([1]);

        // Replace 'b' with new instance (same key)
        await AddOrUpdateAsync(cluster, Event("ns", "b"));
        await WaitForAsync(() => vm.View.Count == 3 && vm.View[0].ShouldBeOfType<Corev1Event>().Name() == "b");

        vm.View[0].ShouldBeOfType<Corev1Event>().Name().ShouldBe("b");
        vm.View[1].ShouldBeOfType<Corev1Event>().Name().ShouldBe("c");
        vm.View[2].ShouldBeOfType<Corev1Event>().Name().ShouldBe("a");

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
        await WaitForAsync(() => vm.View.Count == 3);

        vm.View[0].ShouldBeOfType<Corev1Event>().Name().ShouldBe("c");
        vm.View[1].ShouldBeOfType<Corev1Event>().Name().ShouldBe("b");
        vm.View[2].ShouldBeOfType<Corev1Event>().Name().ShouldBe("a");


        // Select all 3
        vm.SelectionModel.Select(0);
        vm.SelectionModel.Select(1);
        vm.SelectionModel.Select(2);

        vm.SelectionModel.SelectedIndexes.ShouldBe([0, 1, 2]);

        // Replace 'b' with new instance (same key)
        await AddOrUpdateAsync(cluster, Event("ns", "b"));
        await WaitForAsync(() => vm.View.Count == 3 && vm.View[0].ShouldBeOfType<Corev1Event>().Name() == "b");

        vm.View[0].ShouldBeOfType<Corev1Event>().Name().ShouldBe("b");
        vm.View[1].ShouldBeOfType<Corev1Event>().Name().ShouldBe("c");
        vm.View[2].ShouldBeOfType<Corev1Event>().Name().ShouldBe("a");

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
        await WaitForAsync(() => vm.View.Count == 1 && GetFirstRowFirstColumnText(grid, 0, 0) is not null, timeoutMs: 5000);

        var before = GetFirstRowFirstColumnText(grid, 0, 0);
        before.ShouldNotBeNull();
        before.ShouldContain("a");

        // Mutate in place and trigger DynamicData refresh.
        pod.Metadata.Name = "b";
        await AddOrUpdateAsync(cluster, pod);
        await WaitForAsync(() => GetFirstRowFirstColumnText(grid, 0, 0)?.Contains("b", StringComparison.OrdinalIgnoreCase) == true);

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
        await cluster.Runtime.SeedResource<V1Namespace>(true);

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

        await WaitForAsync(() => vm.View.Cast<V1Namespace>().Any(item => item.Name() == "a"));
        var before = GetResourceCellText<V1Namespace>(grid, "a", 1);
        before.ShouldNotBeNull();
        before.ShouldBeEmpty();

        var updatedNamespace = new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = "a",
                Labels = new Dictionary<string, string> { ["test"] = "value" },
            }
        };

        await AddOrUpdateAsync(cluster, updatedNamespace);
        await WaitForAsync(() =>
        {
            var namespaceIndex = vm.View.Cast<V1Namespace>().ToList().FindIndex(item =>
                item.Name() == "a" &&
                item.Metadata.Labels?.TryGetValue("test", out string? value) == true &&
                value == "value");
            return namespaceIndex >= 0 && GetResourceCellText<V1Namespace>(grid, "a", 1)?.Contains("test=value", StringComparison.OrdinalIgnoreCase) == true;
        });

        var after = GetResourceCellText<V1Namespace>(grid, "a", 1);
        after.ShouldNotBeNull();
        after.ShouldContain("test=value");
    }

    [AvaloniaFact(DisplayName = "Mutable sort updates keep the resource list live")]
    public async Task mutable_sort_updates_keep_the_resource_list_live()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<Corev1Event>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        await WaitForAsync(() => view.FindControl<DataGrid>("PART_Grid")?.Columns.Count > 0, 5000);
        vm.SortingModel.Descriptors.Count.ShouldBe(1);
        ((DataGridControlTemplateColumnDefinition)vm.SortingModel.Descriptors[0].ColumnId).ColumnKey.ShouldBe("last-seen");

        var baseTimestamp = DateTime.UtcNow.AddHours(-2);
        for (var i = 0; i < 200; i++)
        {
            await AddOrUpdateAsync(cluster, Event("ns", $"seed-{i}", baseTimestamp.AddMinutes(i), i));
        }

        Dispatcher.UIThread.RunJobs();
        await WaitForAsync(() => vm.View.Count == 200, 5000);
        vm.View.Count.ShouldBe(200);
        await WaitForAsync(() => vm.ItemCount == 200, 5000);

        var left = Event("ns", "left", baseTimestamp.AddHours(5), 1);
        var right = Event("ns", "right", baseTimestamp.AddHours(5).AddMinutes(1), 2);

        await AddOrUpdateAsync(cluster, left);
        await AddOrUpdateAsync(cluster, right);

        await WaitForAsync(
            () => vm.View.Count == 202,
            timeoutMs: 5000);
        await WaitForAsync(() => vm.ItemCount == 202, 5000);
        await WaitForAsync(() => vm.View[0].ShouldBeOfType<Corev1Event>().Name() == "right", 5000);
        vm.View[0].ShouldBeOfType<Corev1Event>().Name().ShouldBe("right");

        for (var i = 0; i < 50; i++)
        {
            left.LastTimestamp = baseTimestamp.AddHours(6 + (i * 2));
            left.Count = i + 10;
            await AddOrUpdateAsync(cluster, left);

            right.LastTimestamp = baseTimestamp.AddHours(6 + (i * 2) + 1);
            right.Count = i + 20;
            await AddOrUpdateAsync(cluster, right);

            Dispatcher.UIThread.RunJobs();

            vm.View.Count.ShouldBe(202);
            await WaitForAsync(
                () => vm.View.Count >= 2
                    && vm.View[0].ShouldBeOfType<Corev1Event>().Name() == "right"
                    && vm.View[1].ShouldBeOfType<Corev1Event>().Name() == "left",
                5000);
        }

        await AddOrUpdateAsync(cluster, Event("ns", "tail", baseTimestamp.AddHours(200), 999));
        Dispatcher.UIThread.RunJobs();

        await WaitForAsync(() => vm.View.Count == 203, 5000);
        vm.View.Count.ShouldBe(203);
        await WaitForAsync(() => vm.ItemCount == 203, 5000);
        vm.View[0].ShouldBeOfType<Corev1Event>().Name().ShouldBe("tail");
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

        var dateColumn = dateVm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), KubeUI.Avalonia.Assets.Resources.V1EventConfig_Last_Seen, StringComparison.Ordinal));
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
        var lastSeenColumn = vm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), KubeUI.Avalonia.Assets.Resources.V1EventConfig_Last_Seen, StringComparison.Ordinal));

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
        var days = GetDateRelativeUnit<ResourceListViewModel<Corev1Event>>(2);
        filterService.ApplyDateFilter(vm.FilteringModel, lastSeenColumn, lastSeenColumn.ValueType, GetDateOperator(FilteringOperator.GreaterThan), 5d, days);
        vm.FilteringModel.Descriptors.Count(descriptor => ReferenceEquals(descriptor.ColumnId, lastSeenColumn) || Equals(descriptor.ColumnId, lastSeenColumn)).ShouldBe(1);
        var dateDescriptor = GetDescriptorForColumn(lastSeenColumn);
        dateDescriptor.Operator.ShouldBe(FilteringOperator.GreaterThan);
        dateDescriptor.Value.ShouldNotBeNull();
        var expectedThreshold = beforeDateFilter.AddDays(-5);
        var actualThreshold = ToDateTimeOffset(dateDescriptor.Value!);
        Math.Abs((actualThreshold - expectedThreshold).TotalSeconds).ShouldBeLessThan(10);

        var hours = GetDateRelativeUnit<ResourceListViewModel<Corev1Event>>(1);
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
        await WaitForAsync(() => vm.View.Count == 3);
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
        await WaitForAsync(() => countVm.View.Count == 2);
        countVm.View.Count.ShouldBe(2);

        var countColumn = countVm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), "Count", StringComparison.Ordinal));
        filterService.ApplyNumericFilter(countVm.FilteringModel, countColumn, GetNumericOperator(FilteringOperator.GreaterThan), 0d, null);
        Dispatcher.UIThread.RunJobs();
        countVm.View.Count.ShouldBe(2);

        var lastSeenColumn = countVm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), KubeUI.Avalonia.Assets.Resources.V1EventConfig_Last_Seen, StringComparison.Ordinal));
        var hours = GetDateRelativeUnit<ResourceListViewModel<Corev1Event>>(1);
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

    [AvaloniaFact(DisplayName = "Namespace filter applies when opening another resource list")]
    public async Task namespace_filter_applies_when_opening_another_resource_list()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var podVm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        podVm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = podVm;
        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("ns1", "pod-a"));
        await AddOrUpdateAsync(cluster, Pod("ns2", "pod-b"));

        podVm.IsNamespaceSelectionLinked = false;
        podVm.SelectedNamespaces.Add(NamespaceResource("ns1"));
        Dispatcher.UIThread.RunJobs();

        var deploymentVm = GetRequiredService<ResourceListViewModel<V1Deployment>>();
        deploymentVm.Initialize(cluster);
        deploymentVm.IsNamespaceSelectionLinked = podVm.IsNamespaceSelectionLinked;
        deploymentVm.SelectedNamespaces.Add(podVm.SelectedNamespaces[0]);
        view.DataContext = deploymentVm;

        await AddOrUpdateAsync(cluster, Deployment("ns1", "deployment-a"));
        await AddOrUpdateAsync(cluster, Deployment("ns2", "deployment-b"));

        Dispatcher.UIThread.RunJobs();
        await WaitForAsync(() => deploymentVm.View.Count == 1);

        deploymentVm.View.Count.ShouldBe(1);
        deploymentVm.View[0].ShouldBeOfType<V1Deployment>().Namespace().ShouldBe("ns1");
    }

    [AvaloniaFact(DisplayName = "Reopening a list does not restore stale managed filters")]
    public async Task reopening_list_does_not_restore_stale_managed_filters()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var podVm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        podVm.Initialize(cluster);
        var podView = GetRequiredService<ResourceListView>();
        podView.DataContext = podVm;

        window.Content = podView;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("a", "pod-a"));
        await AddOrUpdateAsync(cluster, Pod("b", "pod-b"));

        cluster.SelectedNamespaces.Add(NamespaceResource("a"));
        Dispatcher.UIThread.RunJobs();
        podVm.View.Count.ShouldBe(1);
        GetNamespaceFilterValues(podVm).ShouldBe(["a"]);

        var filterService = GetRequiredService<DataGridColumnFilterService>();
        var nameColumn = podVm.ColumnDefinitions.First(column => Equals(column.ColumnKey, "name"));
        filterService.ApplyTextFilter(podVm.FilteringModel, nameColumn, GetTextOperator(FilteringOperator.Contains), "pod-a");
        podVm.SearchQuery = "pod-a";
        Dispatcher.UIThread.RunJobs();

        var deploymentVm = GetRequiredService<ResourceListViewModel<V1Deployment>>();
        deploymentVm.Initialize(cluster);
        await cluster.Runtime.SeedResource<V1Deployment>(true);
        var deploymentView = GetRequiredService<ResourceListView>();
        deploymentView.DataContext = deploymentVm;

        window.Content = deploymentView;
        Dispatcher.UIThread.RunJobs();

        cluster.SelectedNamespaces.Clear();
        Dispatcher.UIThread.RunJobs();
        podVm.SelectedNamespaces.ShouldBeEmpty();
        podVm.FilteringModel.Descriptors.ShouldNotContain(descriptor => Equals(descriptor.ColumnId, ResourceListViewModel<V1Pod>.NamespaceScopeFilterId));
        podVm.FilteringModel.Descriptors.ShouldContain(descriptor => Equals(descriptor.ColumnId, nameColumn));

        podVm.SearchQuery = string.Empty;
        podVm.FilteringModel.Clear();
        Dispatcher.UIThread.RunJobs();
        window.Content = podView;
        Dispatcher.UIThread.RunJobs();

        podVm.SelectedNamespaces.ShouldBeEmpty();
        podVm.FilteringModel.Descriptors.ShouldNotContain(descriptor => Equals(descriptor.ColumnId, ResourceListViewModel<V1Pod>.NamespaceScopeFilterId));
        podVm.FilteringModel.Descriptors.ShouldContain(descriptor => Equals(descriptor.ColumnId, nameColumn));
        podVm.SearchModel.Descriptors.ShouldBeEmpty();
        podVm.View.Count.ShouldBe(1);
    }

    [AvaloniaFact(DisplayName = "Reattaching a list preserves the current namespace scope filter")]
    public async Task reattaching_list_preserves_current_namespace_scope_filter()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);
        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("a", "pod-a"));
        await AddOrUpdateAsync(cluster, Pod("b", "pod-b"));

        cluster.SelectedNamespaces.Add(NamespaceResource("a"));
        Dispatcher.UIThread.RunJobs();
        vm.View.Count.ShouldBe(1);
        GetNamespaceFilterValues(vm).ShouldBe(["a"]);

        window.Content = null;
        Dispatcher.UIThread.RunJobs();
        window.Content = view;
        Dispatcher.UIThread.RunJobs();

        vm.SelectedNamespaces.Select(namespaceResource => namespaceResource.Name()).ShouldBe(["a"]);
        GetNamespaceFilterValues(vm).ShouldBe(["a"]);
        vm.View.Count.ShouldBe(1);
    }

    [AvaloniaFact(DisplayName = "Namespace filter clears item when selection filtered out")]
    public async Task namespace_filter_clears_item_when_selection_filtered_out()
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

        await WaitForAsync(() => vm.View.Count == 5);
        vm.SelectionModel.Select(1);
        await WaitForAsync(() => vm.SelectedItem is not null);
        vm.SelectedItem.ShouldNotBeNull();
        vm.SelectedItem!.Namespace().ShouldBe("ns2");

        cluster.SelectedNamespaces.Add(NamespaceResource("ns4"));
        Dispatcher.UIThread.RunJobs();

        vm.SelectionModel.SelectedIndexes.ShouldBeEmpty();
        vm.SelectedItem?.ShouldBeNull();
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

        var portForwardMenu = vm.GetContextMenuItems(vm.SelectionModel.SelectedItems).FirstOrDefault(x => x.Title == "Port Forwarding");
        portForwardMenu.ShouldBeNull();
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

        var headers = vm.GetContextMenuItems(vm.SelectionModel.SelectedItems).Select(x => x.Title).ToList();

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
        await WaitForAsync(() => vm.View.Count == 1, timeoutMs: 5000);

        vm.View.Count.ShouldBe(1);

        await cluster.Runtime.DeleteResource(Pod("ns1", "a"));
        Dispatcher.UIThread.RunJobs();
        await WaitForAsync(() => vm.View.Count == 0);

        vm.View.Count.ShouldBe(0);
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
        await cluster.Runtime.DeleteResource(NamespaceResource("default"));

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
        await WaitForAsync(() => vm.View.Count == 3);

        var labelsColumn = vm.ColumnDefinitions.First(x => Equals(x.ColumnKey, "name"));

        vm.SortingModel.Clear();

        vm.SortingModel.SetOrUpdate(new(labelsColumn, ListSortDirection.Descending, null, labelsColumn.CustomSortComparer));

        Dispatcher.UIThread.RunJobs();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        Dispatcher.UIThread.RunJobs();

        var view = await WaitForValueAsync(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        view.ShouldNotBeNull();

        vm.View[0].ShouldBeOfType<V1Namespace>().Name().ShouldBe("c");
        vm.View[1].ShouldBeOfType<V1Namespace>().Name().ShouldBe("b");
        vm.View[2].ShouldBeOfType<V1Namespace>().Name().ShouldBe("a");
        vm.SortingModel.Descriptors.Count.ShouldBe(1);
        ((DataGridControlTemplateColumnDefinition)(vm.SortingModel.Descriptors[0].ColumnId)).ColumnKey.ShouldBe("name");

        factory.SetActiveDockable(otherDockable);
        factory.SetFocusedDockable(documents, otherDockable);
        Dispatcher.UIThread.RunJobs();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        Dispatcher.UIThread.RunJobs();

        var restoredView = await WaitForValueAsync(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        restoredView.ShouldNotBeNull();

        vm.View[0].ShouldBeOfType<V1Namespace>().Name().ShouldBe("c");
        vm.View[1].ShouldBeOfType<V1Namespace>().Name().ShouldBe("b");
        vm.View[2].ShouldBeOfType<V1Namespace>().Name().ShouldBe("a");
        vm.SortingModel.Descriptors.Count.ShouldBe(1);
        ((DataGridControlTemplateColumnDefinition)(vm.SortingModel.Descriptors[0].ColumnId)).ColumnKey.ShouldBe("name");
    }

    [AvaloniaFact(DisplayName = "Switching document tabs preserves DataGrid scroll offset")]
    public async Task switching_document_tabs_preserves_datagrid_scroll_offset()
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

        var view = await WaitForValueAsync(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        view.ShouldNotBeNull();

        var grid = view!.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();

        var scrollViewer = grid.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        scrollViewer.ShouldNotBeNull();

        // Wait until content is scrollable
        await WaitForAsync(() => scrollViewer.Extent.Height > scrollViewer.Viewport.Height, 3000);

        scrollViewer.Extent.Height.ShouldBeGreaterThan(scrollViewer.Viewport.Height);

        var targetOffset = new Vector(0, Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
        scrollViewer.Offset = targetOffset;
        Dispatcher.UIThread.RunJobs();

        // switch away to trigger capture
        factory.SetActiveDockable(otherDockable);
        factory.SetFocusedDockable(documents, otherDockable);
        Dispatcher.UIThread.RunJobs();

        vm.DataGridRuntimeState.ShouldNotBeNull();
        vm.DataGridRuntimeState!.Scroll.ShouldNotBeNull();
        vm.DataGridRuntimeState.Scroll!.VerticalOffset.ShouldBe(targetOffset.Y);

        // switch back and ensure restore
        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        Dispatcher.UIThread.RunJobs();

        var restoredView = await WaitForValueAsync(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        restoredView.ShouldNotBeNull();

        var restoredGrid = restoredView!.FindControl<DataGrid>("PART_Grid");
        restoredGrid.ShouldNotBeNull();

        var restoredScrollViewer = restoredGrid.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        restoredScrollViewer.ShouldNotBeNull();

        // Wait until restored grid is scrollable
        await WaitForAsync(() => restoredScrollViewer.Extent.Height > restoredScrollViewer.Viewport.Height, 3000);

        Dispatcher.UIThread.RunJobs();
        restoredScrollViewer.Offset.Y.ShouldBe(targetOffset.Y);
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
        await cluster.Runtime.DeleteResource(NamespaceResource("default"));

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

        var view = await WaitForValueAsync(() => FindVisibleView<ResourceListView>(window, vm), 3000);
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

        var restoredView = await WaitForValueAsync(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        restoredView.ShouldNotBeNull();
        vm.View[0].ShouldBeOfType<V1Namespace>().Name().ShouldBe("a");
        vm.View[1].ShouldBeOfType<V1Namespace>().Name().ShouldBe("b");
        vm.View[2].ShouldBeOfType<V1Namespace>().Name().ShouldBe("c");
        vm.SortingModel.Descriptors.Count.ShouldBe(1);
        ((DataGridControlTemplateColumnDefinition)(vm.SortingModel.Descriptors[0].ColumnId)).ColumnKey.ShouldBe("labels");
    }

    [AvaloniaFact(DisplayName = "Restoring DataGrid state preserves column widths")]
    public async Task restoring_datagrid_state_preserves_column_widths()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();
        grid.Columns.Count.ShouldBeGreaterThan(1);

        var columns = grid.Columns.Take(2).ToList();
        foreach (var (column, width) in columns.Zip([180d, 240d]))
        {
            column.Width = new DataGridLength(width);
        }

        grid.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var widths = columns.ToDictionary(
            column => column.ColumnKey ?? column.Header!,
            column => column.Width.DisplayValue);

        window.Content = null;
        Dispatcher.UIThread.RunJobs();

        var restoredView = GetRequiredService<ResourceListView>();
        restoredView.DataContext = vm;
        window.Content = restoredView;
        Dispatcher.UIThread.RunJobs();

        var restoredGrid = restoredView.FindControl<DataGrid>("PART_Grid");
        restoredGrid.ShouldNotBeNull();

        foreach (var column in restoredGrid.Columns.Take(2))
        {
            var key = column.ColumnKey ?? column.Header!;
            column.Width.DisplayValue.ShouldBe(widths[key], tolerance: 0.1);
        }
    }

    [AvaloniaFact(DisplayName = "Restoring DataGrid state enforces the grid minimum column width")]
    public async Task restoring_datagrid_state_enforces_grid_minimum_column_width()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();
        grid.Columns.First().MinWidth.ShouldBe(90);
        window.Content = null;
        Dispatcher.UIThread.RunJobs();

        vm.DataGridRuntimeState.ShouldNotBeNull();
        vm.DataGridRuntimeState!.Columns.ShouldNotBeNull();
        var columns = vm.DataGridRuntimeState.Columns.Columns.ToList();
        columns[0].Width = new DataGridLength(20);
        vm.DataGridRuntimeState.Columns = new DataGridColumnLayoutState
        {
            Columns = columns,
            FrozenColumnCount = vm.DataGridRuntimeState.Columns.FrozenColumnCount,
            FrozenColumnCountRight = vm.DataGridRuntimeState.Columns.FrozenColumnCountRight
        };

        var restoredView = GetRequiredService<ResourceListView>();
        restoredView.DataContext = vm;
        window.Content = restoredView;
        Dispatcher.UIThread.RunJobs();

        var restoredGrid = restoredView.FindControl<DataGrid>("PART_Grid");
        restoredGrid.ShouldNotBeNull();
        var restoredColumn = restoredGrid.Columns.First();
        restoredColumn.Width.DisplayValue.ShouldBeGreaterThanOrEqualTo(90);

        restoredColumn.Width = new DataGridLength(20);
        restoredColumn.Width.DisplayValue.ShouldBeGreaterThanOrEqualTo(90);
    }

    [AvaloniaFact(DisplayName = "Restoring DataGrid state handles DataContext assigned after attachment")]
    public async Task restoring_datagrid_state_handles_datacontext_assigned_after_attachment()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();
        var column = grid.Columns.First();
        column.Width = new DataGridLength(180);
        grid.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        var originalWidth = column.Width.DisplayValue;

        window.Content = null;
        Dispatcher.UIThread.RunJobs();
        vm.DataGridRuntimeState.ShouldNotBeNull();

        var replacementVm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        replacementVm.Initialize(cluster);
        replacementVm.DataGridRuntimeState = vm.DataGridRuntimeState;

        var restoredView = GetRequiredService<ResourceListView>();
        restoredView.DataContext = vm;
        window.Content = restoredView;
        Dispatcher.UIThread.RunJobs();

        restoredView.DataContext = replacementVm;
        Dispatcher.UIThread.RunJobs();

        var restoredGrid = restoredView.FindControl<DataGrid>("PART_Grid");
        restoredGrid.ShouldNotBeNull();
        restoredGrid.Columns.First().Width.DisplayValue.ShouldBe(originalWidth, tolerance: 0.1);
    }

    [AvaloniaFact(DisplayName = "Saving DataGrid state preserves column width changes when scroll state is unavailable")]
    public async Task saving_datagrid_state_preserves_column_width_changes_when_scroll_state_is_unavailable()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();
        var column = grid.Columns.First();
        column.Width = new DataGridLength(180);
        grid.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        window.Content = null;
        Dispatcher.UIThread.RunJobs();
        vm.DataGridRuntimeState.ShouldNotBeNull();
        vm.DataGridRuntimeState!.Scroll = new DataGridScrollState();

        var changedView = GetRequiredService<ResourceListView>();
        changedView.DataContext = vm;
        window.Content = changedView;
        Dispatcher.UIThread.RunJobs();

        var changedGrid = changedView.FindControl<DataGrid>("PART_Grid");
        changedGrid.ShouldNotBeNull();
        changedGrid.Columns.First().Width = new DataGridLength(240);
        changedGrid.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        window.Content = null;
        Dispatcher.UIThread.RunJobs();

        var restoredView = GetRequiredService<ResourceListView>();
        restoredView.DataContext = vm;
        window.Content = restoredView;
        Dispatcher.UIThread.RunJobs();

        var restoredGrid = restoredView.FindControl<DataGrid>("PART_Grid");
        restoredGrid.ShouldNotBeNull();
        restoredGrid.Columns.First().Width.DisplayValue.ShouldBe(240, tolerance: 0.1);
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

    [AvaloniaFact(DisplayName = "Namespace selector filters the resource list")]
    public async Task namespace_selector_filters_the_resource_list()
    {
        var window = CreateWindow();
        var cluster = await CreateClusterAsync();

        await AddOrUpdateAsync(cluster, NamespaceResource("ns1"));
        await AddOrUpdateAsync(cluster, NamespaceResource("ns2"));

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("ns1", "a"));
        await AddOrUpdateAsync(cluster, Pod("ns2", "b"));
        Dispatcher.UIThread.RunJobs();
        await WaitForAsync(() => vm.View.Count == 2);

        vm.View.Count.ShouldBe(2);

        var selector = view.GetVisualDescendants().OfType<Ursa.Controls.MultiComboBox>().Single();
        selector.SelectedItems.ShouldBeSameAs(vm.SelectedNamespaces);
        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();

        var ns1 = cluster.Runtime.Namespaces.Single(x => x.Name() == "ns1");
        selector.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();

        var item = selector.ContainerFromItem(ns1).ShouldBeOfType<Ursa.Controls.MultiComboBoxItem>();
        item.IsSelected = true;
        selector.IsDropDownOpen = false;
        Dispatcher.UIThread.RunJobs();

        vm.SelectedNamespaces.Select(x => x.Name()).ShouldBe(["ns1"]);
        vm.View.Count.ShouldBe(1);
        vm.View[0].ShouldBeOfType<V1Pod>().Namespace().ShouldBe("ns1");
        grid!.ItemsSource.ShouldBeSameAs(vm.View);

        for (var i = 0; i < 5; i++)
        {
            grid.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        var allRows = GetAllRows(grid).ToList();
        allRows.Count.ShouldBeGreaterThan(0);
        allRows.Select(x => (x.DataContext as V1Pod)?.Namespace()).ShouldContain("ns1");
        var rows = allRows.Where(x => x.IsVisible).ToList();
        rows.Count.ShouldBe(1);
        rows[0].DataContext.ShouldBeOfType<V1Pod>().Namespace().ShouldBe("ns1");
    }

    [AvaloniaFact(DisplayName = "Search query is debounced before filtering view")]
    public async Task search_query_is_debounced_before_filtering_view()
    {
        var cluster = await CreateClusterAsync();

        var vm = GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        await AddOrUpdateAsync(cluster, Pod("ns", "alpha"));
        await AddOrUpdateAsync(cluster, Pod("ns", "beta"));
        await WaitForAsync(() => vm.View.Count == 2);

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

            await TestWait.NextPollAsync(TimeSpan.FromMilliseconds(25), TestContext.Current.CancellationToken);
        }

        Dispatcher.UIThread.RunJobs();
        predicate().ShouldBeTrue();
    }

    private static async Task<T> WaitForValueAsync<T>(Func<T?> getter, int timeoutMs = 1000) where T : class
    {
        T? value = null;
        DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.RunJobs();
            value = getter();
            if (value != null)
            {
                return value;
            }

            await TestWait.NextPollAsync(TimeSpan.FromMilliseconds(25), TestContext.Current.CancellationToken);
        }

        Dispatcher.UIThread.RunJobs();
        value.ShouldNotBeNull();
        return value!;
    }

    private static IList<string> GetNamespaceFilterValues<T>(ResourceListViewModel<T> vm)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var descriptor = vm.FilteringModel.Descriptors.FirstOrDefault(x => Equals(x.ColumnId, ResourceListViewModel<T>.NamespaceScopeFilterId));
        descriptor.ShouldNotBeNull();
        descriptor!.Values.ShouldNotBeNull();
        return descriptor.Values.Cast<string>().ToList();
    }

    private static DateRelativeUnit GetDateRelativeUnit<T>(int index)
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
    public double MinWidth => 90;
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

    public ClusterWorkspace Cluster { get; set; } = null!;
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
    public IEnumerable<MenuItemViewModel> GetContextMenuItems(IEnumerable? selectedItems) => [];
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

    public ClusterWorkspace? Cluster { get; private set; }
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
    public Style[] ListStyle() => [];
    public IEnumerable<(Verb verb, string? subresource)> Permissions() => [];
    public Task EvaluateListWatchAccessAsync() => Task.CompletedTask;
    public Type Type => typeof(V1Pod);
    public IRelayCommand NewResourceCommand => new RelayCommand(() => { });
    public IRelayCommand<IList> ViewCommand { get; }
    public IAsyncRelayCommand<IList> DeleteCommand => throw new NotImplementedException();

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
    }
}

internal sealed class FakeContextMenuResourceListViewModel : IResourceListViewModel
{
    public FakeContextMenuResourceListViewModel(ISelectionModel selectionModel)
    {
        SelectionModel = selectionModel;
        ResourceConfig = new FakeDoubleTapResourceConfig(() => { });
    }

    public ClusterWorkspace Cluster { get; set; } = null!;
    public ObservableCollection<V1Namespace> SelectedNamespaces { get; } = [];
    public bool IsNamespaceSelectionLinked { get; set; } = true;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From<V1Pod>();
    public int ItemCount => 0;
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
    public IEnumerable<MenuItemViewModel> GetContextMenuItems(IEnumerable? selectedItems) => [];
    public ISearchModel SearchModel { get; set; } = new SearchModel();
    public IDataGridSearchAdapterFactory SearchAdapterFactory => throw new NotImplementedException();
    public global::Avalonia.Controls.DataGridState? DataGridRuntimeState { get; set; }
}
