using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Reflection;
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
using KubeUI.Avalonia.Features.Resources.List.Controls;
using KubeUI.Avalonia.Features.AI;
using KubeUI.Avalonia.Features.Resources.List.Behaviors;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Documents.About;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.List;

public class ResourceListViewModelTests
{
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
        if (resource.Metadata.Uid is null)
        {
            resource.Metadata.Uid = cluster.Runtime.GetResource<T>(resource.Namespace(), resource.Name())?.Uid();
        }

        await cluster.Runtime.AddOrUpdateResource(resource);
        await TestApplicationExtensions.WaitForUiAsync();
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

    [AvaloniaFact(DisplayName = "Shift arrow selection contracts when moving back")]
    public async Task shift_arrow_selection_contracts_when_moving_back()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        for (var index = 0; index < 6; index++)
        {
            await AddOrUpdateAsync(cluster, Pod("ns", $"pod-{index}"));
        }

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();
        await WaitForAsync(() => GetAllRows(grid!).Count(row => row.IsVisible) == 6, timeoutMs: 5000);

        var firstRow = GetAllRows(grid!).Single(row => row.IsVisible && row.DataContext is V1Pod pod && pod.Name() == "pod-0");
        var firstRowPoint = GetRowCenterOnWindow(firstRow, window);
        window.MouseDown(firstRowPoint, MouseButton.Left);
        window.MouseUp(firstRowPoint, MouseButton.Left);

        for (var index = 0; index < 4; index++)
        {
            window.KeyPress(Key.Down, RawInputModifiers.Shift, PhysicalKey.ArrowDown, null);
        }

        vm.SelectionModel.SelectedIndexes.ShouldBe([0, 1, 2, 3, 4]);

        window.KeyPress(Key.Up, RawInputModifiers.Shift, PhysicalKey.ArrowUp, null);

        vm.SelectionModel.SelectedIndexes.ShouldBe([0, 1, 2, 3]);
    }


    [AvaloniaFact(DisplayName = "All select update middle")]
    public async Task all_select_update_middle_preserves_all_selected()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
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

    [AvaloniaFact(DisplayName = "Selection publishes lightweight agent context")]
    public async Task selection_publishes_lightweight_agent_context()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        var contextService = Application.Current.GetRequiredTestService<IAgentContextService>();
        vm.Initialize(cluster);
        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("ns", "api"));
        await AddOrUpdateAsync(cluster, Pod("ns", "worker"));
        await WaitForAsync(() => vm.ItemCount == 2, timeoutMs: 5000);

        vm.SelectionModel.Select(0);
        vm.SelectionModel.Select(1);
        await WaitForAsync(() => contextService.Context?.SelectedResources.Count == 2, timeoutMs: 5000);

        contextService.Context!.Namespace.ShouldBe("ns");
        contextService.Context.SelectedResources.ShouldBe([
            new KubeUI.AI.Agents.KubernetesResourceReference("v1", "Pod", "api", "ns"),
            new KubeUI.AI.Agents.KubernetesResourceReference("v1", "Pod", "worker", "ns")]);
    }

    [AvaloniaFact(DisplayName = "Single select update middle")]
    public async Task single_select_update__preserves_only_selected()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
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
        var updatedPod = Pod("ns", "b");
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
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
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
        await TestApplicationExtensions.WaitForUiAsync();

        var items = contextMenu.ItemsSource as IEnumerable<MenuItemViewModel>;
        items.ShouldNotBeNull();

        var headers = items!.Select(x => x.Title).ToList();
        headers.ShouldContain("View");
    }

    [AvaloniaFact(DisplayName = "First right click enables context menu actions for the clicked row")]
    public async Task first_right_click_enables_context_menu_actions_for_the_clicked_row()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
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

        await WaitForAsync(() => GetAllRows(grid).Any(x => x.IsVisible));
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
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
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
            await TestApplicationExtensions.WaitForUiAsync();
        }

        await AssertMenuTargetsRowAsync(rowA, "a");
        await AssertMenuTargetsRowAsync(rowB, "b");
    }

    [AvaloniaFact(DisplayName = "Multi select right click populates context menu")]
    public async Task multi_select_right_click_populates_context_menu()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();

        await AddOrUpdateAsync(cluster, Pod("ns", "a"));
        await AddOrUpdateAsync(cluster, Pod("ns", "b"));
        await WaitForAsync(() => GetAllRows(grid).Count(row => row.IsVisible) == 2);

        vm.SelectionModel.Select(0);
        vm.SelectionModel.Select(1);

        var contextMenu = grid!.ContextMenu;
        contextMenu.ShouldNotBeNull();

        var row = GetAllRows(grid).First(x => x.IsVisible);
        var clickPoint = GetRowCenterOnWindow(row, window);
        window.MouseDown(clickPoint, MouseButton.Right);
        window.MouseUp(clickPoint, MouseButton.Right);
        await TestApplicationExtensions.WaitForUiAsync();

        var items = contextMenu.ItemsSource as IEnumerable<MenuItemViewModel>;
        items.ShouldNotBeNull();

        var headers = items!.Select(item => item.Title).ToList();
        headers.ShouldContain("View");
        headers.ShouldContain("Delete");
    }

    [AvaloniaFact(DisplayName = "Multi select right click uses the full selection")]
    public async Task multi_select_right_click_uses_the_full_selection()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
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
        await TestApplicationExtensions.WaitForUiAsync();

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
        await TestApplicationExtensions.WaitForUiAsync();

        row = GetAllRows(grid).First(x => x.IsVisible && (x.DataContext as V1Pod)?.Name() == "b");
        clickPoint = GetRowCenterOnWindow(row, window);
        window.MouseDown(clickPoint, MouseButton.Right);
        window.MouseUp(clickPoint, MouseButton.Right);
        await TestApplicationExtensions.WaitForUiAsync();

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
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<Corev1Event>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
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
        await WaitForAsync(() => vm.SelectionModel.SelectedIndexes.SequenceEqual([0]));

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
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<Corev1Event>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
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
        await WaitForAsync(() => vm.SelectionModel.SelectedIndexes.SequenceEqual([0, 1, 2]));

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

    [AvaloniaFact(DisplayName = "Large resource list preserves selection during incremental sorted update")]
    public async Task large_resource_list_preserves_selection_during_incremental_sorted_update()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<Corev1Event>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        var baseTimestamp = DateTime.UtcNow.AddHours(-1);
        var events = Enumerable.Range(0, 400)
            .Select(index => Event("ns", $"event-{index:D3}", baseTimestamp.AddMinutes(index), index))
            .Select(item =>
            {
                item.Metadata.Uid = item.Name();
                return item;
            })
            .ToArray();

        cluster.Runtime.GetResourceSourceCache<Corev1Event>().Edit(updater => updater.AddOrUpdate(events));
        await WaitForAsync(() => vm.View.Count == 400, timeoutMs: 5000);

        vm.View[0].ShouldBeOfType<Corev1Event>().Name().ShouldBe("event-399");
        vm.View[1].ShouldBeOfType<Corev1Event>().Name().ShouldBe("event-398");
        vm.View[2].ShouldBeOfType<Corev1Event>().Name().ShouldBe("event-397");

        vm.SelectionModel.Select(0);
        vm.SelectionModel.Select(1);
        vm.SelectionModel.Select(2);

        await AddOrUpdateAsync(
            cluster,
            Event("ns", "event-398", baseTimestamp.AddMinutes(1000), 1000));

        await WaitForAsync(
            () => vm.View.Count == 400
                && vm.View[0].ShouldBeOfType<Corev1Event>().Name() == "event-398",
            timeoutMs: 5000);
        await WaitForAsync(() => vm.SelectionModel.SelectedIndexes.Count == 3);

        vm.SelectionModel.SelectedIndexes.Count.ShouldBe(3);
        vm.SelectionModel.SelectedItems
            .Cast<Corev1Event>()
            .Select(item => item.Name())
            .ShouldBe(["event-398", "event-399", "event-397"]);
    }

    [AvaloniaFact(DisplayName = "Update check DataGrid Text update")]
    public async Task UpdateResourceTextBox()
    {
        using var window = Application.Current.CreateTestWindow();

        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();


        var pod = Pod("ns", "a");
        pod.Spec = new V1PodSpec { NodeName = "node-a" };
        await AddOrUpdateAsync(cluster, pod);
        var nodeColumn = vm.ColumnDefinitions
            .Select((column, index) => (column, index))
            .Single(x => string.Equals(x.column.ColumnKey?.ToString(), "node", StringComparison.Ordinal))
            .index;
        await WaitForAsync(() => vm.View.Count == 1 && GetResourceCellText<V1Pod>(grid, "a", nodeColumn)?.Contains("node-a", StringComparison.OrdinalIgnoreCase) == true, timeoutMs: 5000);

        var before = GetResourceCellText<V1Pod>(grid, "a", nodeColumn);
        before.ShouldNotBeNull();
        before.ShouldContain("node-a");

        // Mutate a displayed resource field and trigger DynamicData refresh.
        pod.Spec.NodeName = "node-b";
        await AddOrUpdateAsync(cluster, pod);
        await WaitForAsync(() => GetResourceCellText<V1Pod>(grid, "a", nodeColumn)?.Contains("node-b", StringComparison.OrdinalIgnoreCase) == true);

        var after = GetResourceCellText<V1Pod>(grid, "a", nodeColumn);
        after.ShouldNotBeNull();
        after.ShouldContain("node-b");
    }

    [AvaloniaFact(DisplayName = "Default text column updates when a resource is replaced")]
    public async Task default_text_column_updates_when_resource_is_replaced()
    {
        using var window = Application.Current.CreateTestWindow();

        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Namespace>>();
        vm.Initialize(cluster);
        await cluster.Runtime.SeedResource<V1Namespace>(true);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();
        grid!.Columns[1].ShouldBeOfType<DataGridTextColumn>();

        var ns = new V1Namespace()
        {
            Metadata = new()
            {
                Name = "a"
            }
        };

        await AddOrUpdateAsync(cluster, ns);

        await WaitForAsync(() => vm.View.Cast<V1Namespace>().Any(item => item.Name() == "a"));
        await WaitForAsync(() => GetAllRows(grid).Any(row => row.IsVisible && (row.DataContext as V1Namespace)?.Name() == "a"), timeoutMs: 5000);
        var before = GetResourceCellText<V1Namespace>(grid, "a", 1);
        (before ?? string.Empty).ShouldBeEmpty();

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
                item.Metadata.Labels?.TryGetValue("test", out var value) == true &&
                value == "value");
            return namespaceIndex >= 0;
        }, timeoutMs: 5000);

        await WaitForAsync(
            () => GetResourceCellText<V1Namespace>(grid, "a", 1)?.Contains("test=value", StringComparison.OrdinalIgnoreCase) == true,
            timeoutMs: 5000);

        var after = GetResourceCellText<V1Namespace>(grid, "a", 1);
        after.ShouldNotBeNull();
        after.ShouldContain("test=value");
    }

    [AvaloniaFact(DisplayName = "Relative-time cells match ProDataGrid text-cell presentation")]
    public async Task relative_time_cells_match_prodatagrid_text_cell_presentation()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();

        var nameColumn = vm.ColumnDefinitions
            .Select((column, index) => (column, index))
            .Single(item => string.Equals(item.column.ColumnKey?.ToString(), "name", StringComparison.Ordinal))
            .index;
        var ageColumn = vm.ColumnDefinitions
            .Select((column, index) => (column, index))
            .Single(item => string.Equals(item.column.ColumnKey?.ToString(), "age", StringComparison.Ordinal))
            .index;

        await AddOrUpdateAsync(cluster, Pod("ns", "presentation-test"));
        await WaitForAsync(
            () => GetAllRows(grid!).Any(row => row.IsVisible && (row.DataContext as V1Pod)?.Name() == "presentation-test"),
            timeoutMs: 5000);

        var row = GetAllRows(grid!)
            .Single(item => item.IsVisible && (item.DataContext as V1Pod)?.Name() == "presentation-test");
        var generatedTextCell = grid!.Columns[nameColumn].GetCellContent(row);
        generatedTextCell.ShouldNotBeNull();
        generatedTextCell.ShouldBeAssignableTo<TextBlock>();
        generatedTextCell!.GetType().BaseType.ShouldBe(typeof(TextBlock));
        var generatedTextBlock = (TextBlock)generatedTextCell;
        var relativeTimeCell = grid.Columns[ageColumn].GetCellContent(row).ShouldBeOfType<AgeCell>();
        await WaitForAsync(() => generatedTextBlock.GetValue(ToolTip.TipProperty) as string == generatedTextBlock.Text);
        await WaitForAsync(() => relativeTimeCell.GetValue(ToolTip.TipProperty) as string == relativeTimeCell.Text);
        generatedTextBlock.GetValue(ToolTip.TipProperty).ShouldBe(generatedTextBlock.Text);
        relativeTimeCell.GetValue(ToolTip.TipProperty).ShouldBe(relativeTimeCell.Text);

        relativeTimeCell.Name.ShouldBe(generatedTextBlock.Name);
        relativeTimeCell.Margin.ShouldBe(generatedTextBlock.Margin);
        relativeTimeCell.HorizontalAlignment.ShouldBe(generatedTextBlock.HorizontalAlignment);
        relativeTimeCell.VerticalAlignment.ShouldBe(generatedTextBlock.VerticalAlignment);
        relativeTimeCell.Width.ShouldBe(generatedTextBlock.Width);
        relativeTimeCell.Height.ShouldBe(generatedTextBlock.Height);
        relativeTimeCell.MinWidth.ShouldBe(generatedTextBlock.MinWidth);
        relativeTimeCell.MaxWidth.ShouldBe(generatedTextBlock.MaxWidth);
        relativeTimeCell.MinHeight.ShouldBe(generatedTextBlock.MinHeight);
        relativeTimeCell.MaxHeight.ShouldBe(generatedTextBlock.MaxHeight);
        relativeTimeCell.FontFamily.ShouldBe(generatedTextBlock.FontFamily);
        relativeTimeCell.FontSize.ShouldBe(generatedTextBlock.FontSize);
        relativeTimeCell.FontStyle.ShouldBe(generatedTextBlock.FontStyle);
        relativeTimeCell.FontStretch.ShouldBe(generatedTextBlock.FontStretch);
        relativeTimeCell.FontWeight.ShouldBe(generatedTextBlock.FontWeight);
        relativeTimeCell.Foreground.ShouldBe(generatedTextBlock.Foreground);
        relativeTimeCell.TextWrapping.ShouldBe(generatedTextBlock.TextWrapping);
        relativeTimeCell.TextTrimming.ShouldBe(generatedTextBlock.TextTrimming);
        relativeTimeCell.TextAlignment.ShouldBe(generatedTextBlock.TextAlignment);
        relativeTimeCell.MaxLines.ShouldBe(generatedTextBlock.MaxLines);
        relativeTimeCell.LineHeight.ShouldBe(generatedTextBlock.LineHeight);
        relativeTimeCell.LineSpacing.ShouldBe(generatedTextBlock.LineSpacing);

        var generatedCell = generatedTextBlock.GetVisualAncestors().OfType<DataGridCell>().Single();
        var relativeTimeDataGridCell = relativeTimeCell.GetVisualAncestors().OfType<DataGridCell>().Single();
        relativeTimeDataGridCell.Padding.ShouldBe(generatedCell.Padding);
        relativeTimeDataGridCell.Margin.ShouldBe(generatedCell.Margin);
        relativeTimeDataGridCell.HorizontalContentAlignment.ShouldBe(generatedCell.HorizontalContentAlignment);
        relativeTimeDataGridCell.VerticalContentAlignment.ShouldBe(generatedCell.VerticalContentAlignment);
    }

    [AvaloniaFact(DisplayName = "Mutable sort updates keep the resource list live")]
    public async Task mutable_sort_updates_keep_the_resource_list_live()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<Corev1Event>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
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

        await TestApplicationExtensions.WaitForUiAsync();
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

            await TestApplicationExtensions.WaitForUiAsync();

            vm.View.Count.ShouldBe(202);
            await WaitForAsync(
                () => vm.View.Count >= 2
                    && vm.View[0].ShouldBeOfType<Corev1Event>().Name() == "right"
                    && vm.View[1].ShouldBeOfType<Corev1Event>().Name() == "left",
                5000);
        }

        await AddOrUpdateAsync(cluster, Event("ns", "tail", baseTimestamp.AddHours(200), 999));
        await TestApplicationExtensions.WaitForUiAsync();

        await WaitForAsync(() => vm.View.Count == 203, 5000);
        vm.View.Count.ShouldBe(203);
        await WaitForAsync(() => vm.ItemCount == 203, 5000);
        vm.View[0].ShouldBeOfType<Corev1Event>().Name().ShouldBe("tail");
    }

    [AvaloniaFact(DisplayName = "Initial resource list count includes all unfiltered resources")]
    public async Task initial_resource_list_count_includes_all_unfiltered_resources()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();
        var events = Enumerable.Range(0, 126)
            .Select(index => Event($"ns-{index % 3}", $"event-{index:D3}", DateTime.UtcNow.AddMinutes(index), index))
            .Select(item =>
            {
                item.Metadata.Uid = item.Name();
                return item;
            })
            .ToArray();

        cluster.Runtime.GetResourceSourceCache<Corev1Event>().Edit(updater => updater.AddOrUpdate(events));

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<Corev1Event>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await WaitForAsync(() => vm.View.Count == 126, timeoutMs: 5000);
        await WaitForAsync(() => vm.ItemCount == 126, timeoutMs: 5000);
        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();
        await WaitForAsync(() => grid!.ItemsSource is IList items && items.Count == 126, timeoutMs: 5000);
        ((IList)grid!.ItemsSource!).Count.ShouldBe(126);
        vm.FilteringModel.Descriptors.ShouldBeEmpty();
        vm.SearchModel.Descriptors.ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task source_cache_updates_publish_the_complete_view_on_the_ui_thread()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<Corev1Event>>();
        vm.Initialize(cluster);

        var publishedOnUiThread = new ConcurrentBag<bool>();
        ((INotifyCollectionChanged)vm.View).CollectionChanged += (_, _) =>
            publishedOnUiThread.Add(Dispatcher.UIThread.CheckAccess());

        var events = Enumerable.Range(0, 125)
            .Select(index => Event("default", $"event-{index}"))
            .Select(item =>
            {
                item.Metadata.Uid = item.Name();
                return item;
            })
            .ToArray();

        await Task.Run(
            () => cluster.Runtime.GetResourceSourceCache<Corev1Event>().Edit(updater => updater.AddOrUpdate(events)),
            TestContext.Current.CancellationToken);

        await WaitForAsync(() => vm.View.Count == 125, timeoutMs: 5000);
        await WaitForAsync(() => vm.ItemCount == 125, timeoutMs: 5000);

        publishedOnUiThread.ShouldNotBeEmpty();
        publishedOnUiThread.ShouldAllBe(value => value);
    }

    [AvaloniaFact(DisplayName = "Resource list columns expose filter buttons")]
    public async Task resource_list_columns_expose_filter_buttons()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();

        grid.Columns.ShouldNotBeEmpty();
        grid.Columns.All(column => column.ShowFilterButton == true).ShouldBeTrue();
        grid.Columns.All(column => column.FilterFlyout != null).ShouldBeTrue();
    }

    [AvaloniaFact]
    public void text_column_binding_returns_empty_for_missing_metadata()
    {
        var column = new ResourceListColumn<V1Pod, string>
        {
            Key = "name",
            Name = "Name",
            Field = pod => pod.Metadata!.Name,
        };

        var createColumn = typeof(ResourceListViewModel<V1Pod>).GetMethod(
            "CreateTextColumnDefinition",
            BindingFlags.Static | BindingFlags.NonPublic);
        createColumn.ShouldNotBeNull();

        var definition = createColumn!.Invoke(null, [column, new DataGridLengthConverter()])
            .ShouldBeOfType<DataGridTextColumnDefinition>();
        definition.Binding.Converter.ShouldNotBeNull();
        definition.Binding.Converter!.Convert(new V1Pod(), typeof(string), null, System.Globalization.CultureInfo.InvariantCulture)
            .ShouldBe(string.Empty);
    }

    [AvaloniaFact(DisplayName = "Resource list filter flyout rows align editors")]
    public async Task resource_list_filter_flyout_rows_align_editors()
    {
        var flyoutFactory = Application.Current.GetRequiredTestService<DataGridColumnFilterFlyoutFactory>();

        var textCluster = await Application.Current.CreateClusterAsync();
        var textVm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        textVm.Initialize(textCluster);
        var textView = Application.Current.GetRequiredTestService<ResourceListView>();
        textView.DataContext = textVm;
        using var textWindow = Application.Current.CreateTestWindow(content: textView);
        textWindow.Show();

        var textColumn = textVm.ColumnDefinitions.First(column => column.ValueType == typeof(string));
        var textFlyout = textColumn.FilterFlyout.ShouldBeOfType<Flyout>();
        textFlyout.ShowAt(textView);
        await TestApplicationExtensions.WaitForUiAsync();
        var textContent = textFlyout.Content.ShouldBeOfType<TextFilterFlyoutView>();
        var textPanel = textContent.Content.ShouldBeOfType<StackPanel>();
        var textRows = textPanel.Children.OfType<Grid>().ToList();
        textRows.Count.ShouldBeGreaterThanOrEqualTo(2);
        textRows[0].Children.OfType<TextBlock>().Single().Text.ShouldBe(Assets.Resources.DataGridFilterFlyout_Condition);
        textRows[1].Children.OfType<TextBlock>().Single().Text.ShouldBe(Assets.Resources.DataGridFilterFlyout_Value);
        textPanel.GetVisualDescendants().OfType<ComboBox>().First().HorizontalAlignment.ShouldBe(HorizontalAlignment.Stretch);
        textRows[1].Children.OfType<TextBox>().Single().HorizontalAlignment.ShouldBe(HorizontalAlignment.Stretch);

        var numericCluster = await Application.Current.CreateClusterAsync();
        var numericVm = Application.Current.GetRequiredTestService<ResourceListViewModel<Corev1Event>>();
        numericVm.Initialize(numericCluster);
        var numericView = Application.Current.GetRequiredTestService<ResourceListView>();
        numericView.DataContext = numericVm;
        using var numericWindow = Application.Current.CreateTestWindow(content: numericView);
        numericWindow.Show();

        var numericColumn = numericVm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), "Count", StringComparison.Ordinal));
        var numericFlyout = numericColumn.FilterFlyout.ShouldBeOfType<Flyout>();
        numericFlyout.ShowAt(numericView);
        await TestApplicationExtensions.WaitForUiAsync();
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
        numericRows[0].Children.OfType<TextBlock>().Single().Text.ShouldBe(Assets.Resources.DataGridFilterFlyout_Condition);

        var dateCluster = await Application.Current.CreateClusterAsync();
        var dateVm = Application.Current.GetRequiredTestService<ResourceListViewModel<Corev1Event>>();
        dateVm.Initialize(dateCluster);
        var dateView = Application.Current.GetRequiredTestService<ResourceListView>();
        dateView.DataContext = dateVm;
        using var dateWindow = Application.Current.CreateTestWindow(content: dateView);
        dateWindow.Show();

        var dateColumn = dateVm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), Assets.Resources.V1EventConfig_Last_Seen, StringComparison.Ordinal));
        var dateFlyout = dateColumn.FilterFlyout.ShouldBeOfType<Flyout>();
        dateFlyout.ShowAt(dateView);
        await TestApplicationExtensions.WaitForUiAsync();
        var dateContent = dateFlyout.Content.ShouldBeOfType<DateFilterFlyoutView>();
        var datePanel = dateContent.Content.ShouldBeOfType<StackPanel>();
        var dateRows = datePanel.Children.OfType<Grid>().ToList();
        dateRows[0].Children.OfType<TextBlock>().Single().Text.ShouldBe(Assets.Resources.DataGridFilterFlyout_Condition);
        dateRows[1].Children.OfType<TextBlock>().Single().Text.ShouldBe(Assets.Resources.DataGridFilterFlyout_Value);
        datePanel.GetVisualDescendants().OfType<NumericUpDown>().Count().ShouldBe(1);
        datePanel.GetVisualDescendants().OfType<ComboBox>().Count().ShouldBeGreaterThanOrEqualTo(2);
        datePanel.GetVisualDescendants().OfType<ComboBox>().All(combo => combo.HorizontalAlignment == HorizontalAlignment.Stretch).ShouldBeTrue();

        var enumColumnDefinition = new TestEnumColumnDefinition();
        var enumDataGridColumn = new DataGridControlTemplateColumnDefinition();
        var enumFlyout = flyoutFactory.Create(enumColumnDefinition, enumDataGridColumn, new FilteringModel()).ShouldBeOfType<Flyout>();
        var enumHost = new Button();
        using var enumWindow = Application.Current.CreateTestWindow(content: enumHost);
        enumWindow.Show();
        enumFlyout.ShowAt(enumHost);
        await TestApplicationExtensions.WaitForUiAsync();
        var enumContent = enumFlyout.Content.ShouldBeOfType<EnumFilterFlyoutView>();
        var enumPanel = enumContent.Content.ShouldBeOfType<StackPanel>();
        var enumRows = enumPanel.Children.OfType<Grid>().ToList();
        enumRows.Count.ShouldBe(2);
        enumRows[0].Children.OfType<TextBlock>().Single().Text.ShouldBe(Assets.Resources.DataGridFilterFlyout_Condition);
        enumRows[1].Children.OfType<TextBlock>().Single().Text.ShouldBe(Assets.Resources.DataGridFilterFlyout_Value);
        enumPanel.GetVisualDescendants().OfType<ComboBox>().Count().ShouldBe(2);
        enumPanel.GetVisualDescendants().OfType<ComboBox>().All(combo => combo.HorizontalAlignment == HorizontalAlignment.Stretch).ShouldBeTrue();
    }

    [AvaloniaFact(DisplayName = "Resource list numeric and date filters support comparison operators")]
    public async Task resource_list_numeric_and_date_filters_support_comparison_operators()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();
        var filterService = Application.Current.GetRequiredTestService<DataGridColumnFilterService>();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<Corev1Event>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        var countColumn = vm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), "Count", StringComparison.Ordinal));
        var lastSeenColumn = vm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), Assets.Resources.V1EventConfig_Last_Seen, StringComparison.Ordinal));

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

        var beforeDateFilter = DateTimeOffset.UnixEpoch;
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
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();
        var filterService = Application.Current.GetRequiredTestService<DataGridColumnFilterService>();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
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

        await TestApplicationExtensions.WaitForUiAsync();
        await WaitForAsync(() => vm.View.Count == 3);
        vm.View.Count.ShouldBe(3);

        var nameColumn = vm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), "Name", StringComparison.Ordinal));
        filterService.ApplyTextFilter(vm.FilteringModel, nameColumn, GetTextOperator(FilteringOperator.Contains), "alp");
        await WaitForAsync(() => vm.View.Count == 1);

        ((V1Pod)vm.View[0]).Name().ShouldBe("alpha");

        filterService.ApplyTextFilter(
            vm.FilteringModel,
            nameColumn,
            ResourceListFilterFlyoutOptions.TextOperators.First(option => option.CustomId == FilterOperatorId.TextNotContains),
            "alp");
        await WaitForAsync(() => vm.View.Count == 2);

        vm.View.OfType<V1Pod>().Select(pod => pod.Name()).ShouldBe(["beta", "gamma"]);

        var countCluster = await Application.Current.CreateClusterAsync();
        var countVm = Application.Current.GetRequiredTestService<ResourceListViewModel<Corev1Event>>();
        countVm.Initialize(countCluster);

        var countView = Application.Current.GetRequiredTestService<ResourceListView>();
        countView.DataContext = countVm;

        using var countWindow = Application.Current.CreateTestWindow(content: countView);
        countWindow.Show();

        var older = new Corev1Event
        {
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = "ns",
                Name = "older",
                CreationTimestamp = DateTime.UnixEpoch.AddHours(-5)
            },
            Count = 1,
            LastTimestamp = DateTime.UnixEpoch.AddHours(-5)
        };

        var newer = new Corev1Event
        {
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = "ns",
                Name = "newer",
                CreationTimestamp = DateTime.UnixEpoch.AddMinutes(-10)
            },
            Count = 2,
            LastTimestamp = DateTime.UnixEpoch.AddMinutes(-10)
        };

        await AddOrUpdateAsync(countCluster, older);
        await AddOrUpdateAsync(countCluster, newer);
        await TestApplicationExtensions.WaitForUiAsync();
        await WaitForAsync(() => countVm.View.Count == 2);
        countVm.View.Count.ShouldBe(2);

        var countColumn = countVm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), "Count", StringComparison.Ordinal));
        filterService.ApplyNumericFilter(countVm.FilteringModel, countColumn, GetNumericOperator(FilteringOperator.GreaterThan), 0d, null);
        await TestApplicationExtensions.WaitForUiAsync();
        countVm.View.Count.ShouldBe(2);

        var lastSeenColumn = countVm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), Assets.Resources.V1EventConfig_Last_Seen, StringComparison.Ordinal));
        var hours = GetDateRelativeUnit<ResourceListViewModel<Corev1Event>>(1);
        filterService.ApplyDateFilter(countVm.FilteringModel, lastSeenColumn, lastSeenColumn.ValueType, GetDateOperator(FilteringOperator.GreaterThan), 1d, hours);
        await TestApplicationExtensions.WaitForUiAsync();
        await WaitForAsync(() => countVm.View.Count == 1);
        ((Corev1Event)countVm.View[0]).Name().ShouldBe("newer");
    }

    [AvaloniaFact(DisplayName = "Text filter flyout apply command updates the live view")]
    public async Task text_filter_flyout_apply_command_updates_the_live_view()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("ns", "alpha"));
        await AddOrUpdateAsync(cluster, Pod("ns", "beta"));
        await AddOrUpdateAsync(cluster, Pod("ns", "gamma"));
        await TestApplicationExtensions.WaitForUiAsync();

        var nameColumn = vm.ColumnDefinitions.First(column => string.Equals(column.Header?.ToString(), "Name", StringComparison.Ordinal));
        var flyout = (Flyout)nameColumn.FilterFlyout!;
        flyout.ShowAt(view);
        await TestApplicationExtensions.WaitForUiAsync();
        var flyoutContext = flyout.Content.ShouldBeOfType<TextFilterFlyoutView>().DataContext.ShouldBeOfType<TextFilterFlyoutContext>();

        flyoutContext.SelectedOperator = ResourceListFilterFlyoutOptions.TextOperators.First(option => option.Operator == FilteringOperator.Contains && (option.CustomId is null || !FilterOperatorIdCatalog.UsesCustomDescriptor(option.CustomId.Value)));
        flyoutContext.Query = "alp";
        flyoutContext.ApplyCommand.Execute(null);
        await WaitForAsync(() => vm.View.Count == 1);

        ((V1Pod)vm.View[0]).Name().ShouldBe("alpha");

        flyoutContext.SelectedOperator = ResourceListFilterFlyoutOptions.TextOperators.First(option => option.CustomId == FilterOperatorId.TextNotContains);
        flyoutContext.Query = "alp";
        flyoutContext.ApplyCommand.Execute(null);
        await WaitForAsync(() => vm.View.Count == 2);

        vm.View.OfType<V1Pod>().Select(pod => pod.Name()).ShouldBe(["beta", "gamma"]);
    }

    [AvaloniaFact(DisplayName = "Namespace filter preserves selection when included")]
    public async Task namespace_filter_preserves_selection_when_included()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("ns1", "a"));
        await AddOrUpdateAsync(cluster, Pod("ns2", "b"));

        vm.SelectionModel.Select(0);

        cluster.SelectedNamespaces.Add(NamespaceResource("ns1"));
        await TestApplicationExtensions.WaitForUiAsync();

        vm.SelectedItem.ShouldNotBeNull();
        vm.SelectedItem!.Namespace().ShouldBe("ns1");
        vm.SelectedItem.Name().ShouldBe("a");
    }

    [AvaloniaFact(DisplayName = "Namespace filter applies when opening another resource list")]
    public async Task namespace_filter_applies_when_opening_another_resource_list()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var podVm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        podVm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = podVm;
        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("ns1", "pod-a"));
        await AddOrUpdateAsync(cluster, Pod("ns2", "pod-b"));

        podVm.IsNamespaceSelectionLinked = false;
        podVm.SelectedNamespaces.Add(NamespaceResource("ns1"));
        await TestApplicationExtensions.WaitForUiAsync();

        var deploymentVm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Deployment>>();
        deploymentVm.Initialize(cluster);
        deploymentVm.IsNamespaceSelectionLinked = podVm.IsNamespaceSelectionLinked;
        deploymentVm.SelectedNamespaces.Add(podVm.SelectedNamespaces[0]);
        view.DataContext = deploymentVm;

        await AddOrUpdateAsync(cluster, Deployment("ns1", "deployment-a"));
        await AddOrUpdateAsync(cluster, Deployment("ns2", "deployment-b"));

        await TestApplicationExtensions.WaitForUiAsync();
        await WaitForAsync(() => deploymentVm.View.Count == 1);

        deploymentVm.View.Count.ShouldBe(1);
        deploymentVm.View[0].ShouldBeOfType<V1Deployment>().Namespace().ShouldBe("ns1");
    }

    [AvaloniaFact(DisplayName = "Reopening a list does not restore stale managed filters")]
    public async Task reopening_list_does_not_restore_stale_managed_filters()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var podVm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        podVm.Initialize(cluster);
        var podView = Application.Current.GetRequiredTestService<ResourceListView>();
        podView.DataContext = podVm;

        window.Content = podView;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("a", "pod-a"));
        await AddOrUpdateAsync(cluster, Pod("b", "pod-b"));

        cluster.SelectedNamespaces.Add(NamespaceResource("a"));
        await WaitForAsync(() => podVm.View.Count == 1);
        GetNamespaceFilterValues(podVm).ShouldBe(["a"]);

        var filterService = Application.Current.GetRequiredTestService<DataGridColumnFilterService>();
        var nameColumn = podVm.ColumnDefinitions.First(column => Equals(column.ColumnKey, "name"));
        filterService.ApplyTextFilter(podVm.FilteringModel, nameColumn, GetTextOperator(FilteringOperator.Contains), "pod-a");
        podVm.SearchQuery = "pod-a";
        await TestApplicationExtensions.WaitForUiAsync();

        var deploymentVm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Deployment>>();
        deploymentVm.Initialize(cluster);
        await cluster.Runtime.SeedResource<V1Deployment>(true);
        var deploymentView = Application.Current.GetRequiredTestService<ResourceListView>();
        deploymentView.DataContext = deploymentVm;

        window.Content = deploymentView;
        await TestApplicationExtensions.WaitForUiAsync();

        cluster.SelectedNamespaces.Clear();
        await TestApplicationExtensions.WaitForUiAsync();
        podVm.SelectedNamespaces.ShouldBeEmpty();
        podVm.FilteringModel.Descriptors.ShouldNotContain(descriptor => Equals(descriptor.ColumnId, ResourceListViewModel<V1Pod>.NamespaceScopeFilterId));
        podVm.FilteringModel.Descriptors.ShouldContain(descriptor => Equals(descriptor.ColumnId, nameColumn));

        podVm.SearchQuery = string.Empty;
        podVm.FilteringModel.Clear();
        await TestApplicationExtensions.WaitForUiAsync();
        window.Content = podView;
        await TestApplicationExtensions.WaitForUiAsync();

        podVm.SelectedNamespaces.ShouldBeEmpty();
        podVm.FilteringModel.Descriptors.ShouldNotContain(descriptor => Equals(descriptor.ColumnId, ResourceListViewModel<V1Pod>.NamespaceScopeFilterId));
        podVm.FilteringModel.Descriptors.ShouldContain(descriptor => Equals(descriptor.ColumnId, nameColumn));
        podVm.SearchModel.Descriptors.ShouldBeEmpty();
        podVm.View.Count.ShouldBe(1);
    }

    [AvaloniaFact(DisplayName = "Reattaching a list preserves the current namespace scope filter")]
    public async Task reattaching_list_preserves_current_namespace_scope_filter()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);
        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("a", "pod-a"));
        await AddOrUpdateAsync(cluster, Pod("b", "pod-b"));

        cluster.SelectedNamespaces.Add(NamespaceResource("a"));
        await WaitForAsync(() => vm.View.Count == 1);
        GetNamespaceFilterValues(vm).ShouldBe(["a"]);

        window.Content = null;
        await TestApplicationExtensions.WaitForUiAsync();
        window.Content = view;
        await TestApplicationExtensions.WaitForUiAsync();

        vm.SelectedNamespaces.Select(namespaceResource => namespaceResource.Name()).ShouldBe(["a"]);
        GetNamespaceFilterValues(vm).ShouldBe(["a"]);
        vm.View.Count.ShouldBe(1);
    }

    [AvaloniaFact(DisplayName = "Namespace filter clears item when selection filtered out")]
    public async Task namespace_filter_clears_item_when_selection_filtered_out()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
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
        await TestApplicationExtensions.WaitForUiAsync();

        vm.SelectionModel.SelectedIndexes.ShouldBeEmpty();
        vm.SelectedItem?.ShouldBeNull();
    }

    [AvaloniaFact(DisplayName = "Namespace filter updates context menu selection")]
    public async Task namespace_filter_updates_context_menu_selection()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
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
        await TestApplicationExtensions.WaitForUiAsync();

        var portForwardMenu = vm.GetContextMenuItems(vm.SelectionModel.SelectedItems).FirstOrDefault(x => x.Title == "Port Forwarding");
        portForwardMenu.ShouldBeNull();
    }

    [AvaloniaFact(DisplayName = "Resource list enum filters render a selector")]
    public async Task resource_list_enum_filters_render_a_selector()
    {
        var filterService = Application.Current.GetRequiredTestService<DataGridColumnFilterService>();
        var flyoutFactory = Application.Current.GetRequiredTestService<DataGridColumnFilterFlyoutFactory>();
        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        var cluster = await Application.Current.CreateClusterAsync();
        vm.Initialize(cluster);

        var column = new TestEnumColumnDefinition();
        var dataGridColumn = new DataGridControlTemplateColumnDefinition();

        var flyout = flyoutFactory.Create(column, dataGridColumn, vm.FilteringModel).ShouldBeOfType<Flyout>();
        var host = new Button();
        using var window = Application.Current.CreateTestWindow(content: host);
        window.Show();
        flyout.ShowAt(host);
        await TestApplicationExtensions.WaitForUiAsync();
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
        var cluster = await Application.Current.CreateClusterAsync();
        cluster.SelectedNamespaces.Add(NamespaceResource("team-a"));

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        vm.IsNamespaceSelectionLinked.ShouldBeTrue();
        ReferenceEquals(vm.SelectedNamespaces, cluster.SelectedNamespaces).ShouldBeTrue();

        cluster.SelectedNamespaces.Add(NamespaceResource("team-b"));
        await TestApplicationExtensions.WaitForUiAsync();

        vm.SelectedNamespaces.Select(x => x.Name()).ShouldBe(["team-a", "team-b"]);
        GetNamespaceFilterValues(vm).ShouldBe(["team-a", "team-b"]);
    }

    [AvaloniaFact(DisplayName = "Namespace filter can be decoupled from cluster selection")]
    public async Task namespace_filter_can_be_decoupled_from_cluster_selection()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        cluster.SelectedNamespaces.Add(NamespaceResource("team-a"));

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        vm.IsNamespaceSelectionLinked = false;
        await TestApplicationExtensions.WaitForUiAsync();

        ReferenceEquals(vm.SelectedNamespaces, cluster.SelectedNamespaces).ShouldBeFalse();
        vm.SelectedNamespaces.Select(x => x.Name()).ShouldBe(["team-a"]);

        cluster.SelectedNamespaces.Add(NamespaceResource("team-b"));
        await TestApplicationExtensions.WaitForUiAsync();

        vm.SelectedNamespaces.Select(x => x.Name()).ShouldBe(["team-a"]);

        vm.SelectedNamespaces.Add(NamespaceResource("team-c"));
        await TestApplicationExtensions.WaitForUiAsync();

        cluster.SelectedNamespaces.Select(x => x.Name()).ShouldBe(["team-a", "team-b"]);
        GetNamespaceFilterValues(vm).ShouldBe(["team-a", "team-c"]);
    }

    [AvaloniaFact(DisplayName = "Namespace filter relinks back to cluster selection")]
    public async Task namespace_filter_relinks_back_to_cluster_selection()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        cluster.SelectedNamespaces.Add(NamespaceResource("team-a"));

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);
        vm.IsNamespaceSelectionLinked = false;
        vm.SelectedNamespaces.Clear();
        vm.SelectedNamespaces.Add(NamespaceResource("team-local"));
        await TestApplicationExtensions.WaitForUiAsync();

        vm.IsNamespaceSelectionLinked = true;
        await TestApplicationExtensions.WaitForUiAsync();

        ReferenceEquals(vm.SelectedNamespaces, cluster.SelectedNamespaces).ShouldBeTrue();
        vm.SelectedNamespaces.Select(x => x.Name()).ShouldBe(["team-a"]);
        GetNamespaceFilterValues(vm).ShouldBe(["team-a"]);
    }

    [AvaloniaFact(DisplayName = "Clearing namespace column filter preserves namespace scope filter")]
    public async Task clearing_namespace_column_filter_preserves_namespace_scope_filter()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();
        var filterService = Application.Current.GetRequiredTestService<DataGridColumnFilterService>();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("ns1", "a"));
        await AddOrUpdateAsync(cluster, Pod("ns2", "b"));
        await AddOrUpdateAsync(cluster, Pod("ns3", "c"));

        cluster.SelectedNamespaces.Add(NamespaceResource("ns1"));
        cluster.SelectedNamespaces.Add(NamespaceResource("ns2"));
        await WaitForAsync(() => vm.View.Count == 2);

        GetNamespaceFilterValues(vm).ShouldBe(["ns1", "ns2"]);

        var namespaceColumn = vm.ColumnDefinitions.First(column => string.Equals(column.ColumnKey?.ToString(), "namespace", StringComparison.OrdinalIgnoreCase));
        filterService.ApplyTextFilter(vm.FilteringModel, namespaceColumn, GetTextOperator(FilteringOperator.Contains), "ns1");
        await WaitForAsync(() => vm.View.Count == 1);

        vm.FilteringModel.Descriptors.Count.ShouldBe(2);
        ((V1Pod)vm.View[0]).Namespace().ShouldBe("ns1");

        filterService.ClearColumnFilter(vm.FilteringModel, namespaceColumn);
        await WaitForAsync(() => vm.View.Count == 2);

        vm.FilteringModel.Descriptors.Count.ShouldBe(1);
        GetNamespaceFilterValues(vm).ShouldBe(["ns1", "ns2"]);
    }

    [AvaloniaFact(DisplayName = "Pod-specific actions are hidden for multi-select")]
    public async Task pod_specific_actions_are_hidden_for_multi_select()
    {
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
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
        using var window = Application.Current.CreateTestWindow();

        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("ns1", "a"));
        await WaitForAsync(() => vm.View.Count == 1, timeoutMs: 5000);

        vm.View.Count.ShouldBe(1);

        await cluster.Runtime.DeleteResource(Pod("ns1", "a"));
        await TestApplicationExtensions.WaitForUiAsync();
        await WaitForAsync(() => vm.View.Count == 0);

        vm.View.Count.ShouldBe(0);
    }

    [AvaloniaFact(DisplayName = "Reattach keeps only saved sort descriptors")]
    public async Task reattach_keeps_only_saved_sort_descriptors()
    {
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var dockControl = new DockControl
        {
            Layout = layout,
        };

        using var window = Application.Current.CreateTestWindow(content: dockControl);
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Namespace>>();
        vm.Initialize(cluster);

        window.Show();

        var otherDockable = Application.Current.GetRequiredTestService<AboutViewModel>();
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
        await WaitForAsync(() => vm.View.OfType<V1Namespace>().Count(item => item.Name() is "a" or "b" or "c") == 3);

        var labelsColumn = vm.ColumnDefinitions.First(x => Equals(x.ColumnKey, "name"));

        vm.SortingModel.Clear();

        vm.SortingModel.SetOrUpdate(new(labelsColumn, ListSortDirection.Descending, null, labelsColumn.CustomSortComparer));

        await TestApplicationExtensions.WaitForUiAsync();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        await TestApplicationExtensions.WaitForUiAsync();

        var view = await WaitForValueAsync(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        view.ShouldNotBeNull();

        var sortedNamespaces = vm.View.OfType<V1Namespace>()
            .Where(item => item.Name() is "a" or "b" or "c")
            .ToArray();
        sortedNamespaces.Select(item => item.Name()).ShouldBe(["c", "b", "a"]);
        vm.SortingModel.Descriptors.Count.ShouldBe(1);
        ((DataGridColumnDefinition)(vm.SortingModel.Descriptors[0].ColumnId)).ColumnKey.ShouldBe("name");

        factory.SetActiveDockable(otherDockable);
        factory.SetFocusedDockable(documents, otherDockable);
        await TestApplicationExtensions.WaitForUiAsync();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        await TestApplicationExtensions.WaitForUiAsync();

        var restoredView = await WaitForValueAsync(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        restoredView.ShouldNotBeNull();

        var restoredNamespaces = vm.View.OfType<V1Namespace>()
            .Where(item => item.Name() is "a" or "b" or "c")
            .ToArray();
        restoredNamespaces.Select(item => item.Name()).ShouldBe(["c", "b", "a"]);
        vm.SortingModel.Descriptors.Count.ShouldBe(1);
        ((DataGridColumnDefinition)(vm.SortingModel.Descriptors[0].ColumnId)).ColumnKey.ShouldBe("name");
    }

    [AvaloniaFact(DisplayName = "Switching document tabs preserves DataGrid scroll offset")]
    public async Task switching_document_tabs_preserves_datagrid_scroll_offset()
    {
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var dockControl = new DockControl
        {
            Layout = layout,
        };

        using var window = Application.Current.CreateTestWindow(height: 900, content: dockControl);
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        window.Show();

        var otherDockable = Application.Current.GetRequiredTestService<AboutViewModel>();
        otherDockable.Id = nameof(AboutViewModel);

        factory.AddToDocuments(vm);
        factory.AddToDocuments(otherDockable);

        // Seed many items so vertical scrolling appears
        for (var i = 0; i < 400; i++)
        {
            await AddOrUpdateAsync(cluster, Pod("ns", i.ToString()));
        }

        await WaitForAsync(() => vm.View.Count == 400, 5000);
        await TestApplicationExtensions.WaitForUiAsync();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        await TestApplicationExtensions.WaitForUiAsync();

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
        await WaitForAsync(() => Math.Abs(scrollViewer.Offset.Y - targetOffset.Y) < 0.1, 5000);

        // switch away to trigger capture
        factory.SetActiveDockable(otherDockable);
        factory.SetFocusedDockable(documents, otherDockable);
        await TestApplicationExtensions.WaitForUiAsync();

        vm.DataGridRuntimeState.ShouldNotBeNull();
        vm.DataGridRuntimeState!.Scroll.ShouldNotBeNull();
        vm.DataGridRuntimeState.Scroll!.VerticalOffset.ShouldBe(targetOffset.Y);

        // switch back and ensure restore
        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        await TestApplicationExtensions.WaitForUiAsync();

        var restoredView = await WaitForValueAsync(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        restoredView.ShouldNotBeNull();

        var restoredGrid = restoredView!.FindControl<DataGrid>("PART_Grid");
        restoredGrid.ShouldNotBeNull();

        var restoredScrollViewer = restoredGrid.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        restoredScrollViewer.ShouldNotBeNull();

        // Wait until restored grid is scrollable
        await WaitForAsync(() => restoredScrollViewer.Extent.Height > restoredScrollViewer.Viewport.Height, 3000);

        await WaitForAsync(
            () => Math.Abs(restoredScrollViewer.Offset.Y - targetOffset.Y) < 0.1,
            10000);
        restoredScrollViewer.Offset.Y.ShouldBe(targetOffset.Y);
        ReferenceEquals(grid, restoredGrid).ShouldBeFalse();
        vm.DataGridRuntimeState.ShouldNotBeNull();

    }

    [AvaloniaFact(DisplayName = "Reattach captures runtime state and restores on reattach")]
    public async Task reattach_captures_runtime_state_and_restores_on_reattach()
    {
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var dockControl = new DockControl
        {
            Layout = layout,
        };

        using var window = Application.Current.CreateTestWindow(content: dockControl);
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Namespace>>();
        vm.Initialize(cluster);

        window.Show();

        var otherDockable = Application.Current.GetRequiredTestService<AboutViewModel>();
        otherDockable.Id = nameof(AboutViewModel);

        factory.AddToDocuments(vm);
        factory.AddToDocuments(otherDockable);

        var nsA = NamespaceResource("a");
        var nsB = NamespaceResource("b");
        var nsC = NamespaceResource("c");

        await AddOrUpdateAsync(cluster, nsA);
        await AddOrUpdateAsync(cluster, nsB);
        await AddOrUpdateAsync(cluster, nsC);

        await WaitForAsync(
            () => vm.View.OfType<V1Namespace>().Count(item => item.Name() is "a" or "b" or "c") == 3,
            3000);

        var labelsColumn = vm.ColumnDefinitions.First(x => Equals(x.ColumnKey, "labels"));

        vm.SortingModel.Clear();

        vm.SortingModel.SetOrUpdate(new(labelsColumn, ListSortDirection.Descending, null, labelsColumn.CustomSortComparer));

        await TestApplicationExtensions.WaitForUiAsync();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        await TestApplicationExtensions.WaitForUiAsync();

        var view = await WaitForValueAsync(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        view.ShouldNotBeNull();

        // switch away to trigger capture
        factory.SetActiveDockable(otherDockable);
        factory.SetFocusedDockable(documents, otherDockable);
        await TestApplicationExtensions.WaitForUiAsync();

        // runtime snapshot should be captured on VM by behavior
        vm.DataGridRuntimeState.ShouldNotBeNull();

        // switch back and ensure restore
        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        await TestApplicationExtensions.WaitForUiAsync();

        var restoredView = await WaitForValueAsync(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        restoredView.ShouldNotBeNull();
        await WaitForAsync(
            () => vm.View.OfType<V1Namespace>()
                .Where(item => item.Name() is "a" or "b" or "c")
                .Select(item => item.Name())
                .OrderBy(name => name)
                .SequenceEqual(["a", "b", "c"]),
            3000);
        var sortedNamespaces = vm.View.OfType<V1Namespace>()
            .Where(item => item.Name() is "a" or "b" or "c")
            .ToArray();
        sortedNamespaces.Select(item => item.Name()).OrderBy(name => name).ShouldBe(["a", "b", "c"]);
        vm.SortingModel.Descriptors.Count.ShouldBe(1);
        ((DataGridColumnDefinition)(vm.SortingModel.Descriptors[0].ColumnId)).ColumnKey.ShouldBe("labels");
    }

    [AvaloniaFact(DisplayName = "Restoring DataGrid state preserves column widths")]
    public async Task restoring_datagrid_state_preserves_column_widths()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();
        grid.Columns.Count.ShouldBeGreaterThan(1);

        var columns = grid.Columns.Take(2).ToList();
        foreach (var (column, width) in columns.Zip([180d, 240d]))
        {
            column.Width = new DataGridLength(width);
        }

        grid.UpdateLayout();
        await TestApplicationExtensions.WaitForUiAsync();

        var widths = columns.ToDictionary(
            column => column.ColumnKey ?? column.Header!,
            column => column.Width.DisplayValue);

        window.Content = null;
        await TestApplicationExtensions.WaitForUiAsync();

        var restoredView = Application.Current.GetRequiredTestService<ResourceListView>();
        restoredView.DataContext = vm;
        window.Content = restoredView;
        await TestApplicationExtensions.WaitForUiAsync();

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
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();
        grid.Columns.First().MinWidth.ShouldBe(90);
        window.Content = null;
        await TestApplicationExtensions.WaitForUiAsync();

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

        var restoredView = Application.Current.GetRequiredTestService<ResourceListView>();
        restoredView.DataContext = vm;
        window.Content = restoredView;
        await TestApplicationExtensions.WaitForUiAsync();

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
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();
        var column = grid.Columns.First();
        column.Width = new DataGridLength(180);
        grid.UpdateLayout();
        await TestApplicationExtensions.WaitForUiAsync();
        var originalWidth = column.Width.DisplayValue;

        window.Content = null;
        await TestApplicationExtensions.WaitForUiAsync();
        vm.DataGridRuntimeState.ShouldNotBeNull();

        var replacementVm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        replacementVm.Initialize(cluster);
        replacementVm.DataGridRuntimeState = vm.DataGridRuntimeState;

        var restoredView = Application.Current.GetRequiredTestService<ResourceListView>();
        restoredView.DataContext = vm;
        window.Content = restoredView;
        await TestApplicationExtensions.WaitForUiAsync();

        restoredView.DataContext = replacementVm;
        await TestApplicationExtensions.WaitForUiAsync();

        var restoredGrid = restoredView.FindControl<DataGrid>("PART_Grid");
        restoredGrid.ShouldNotBeNull();
        restoredGrid.Columns.First().Width.DisplayValue.ShouldBe(originalWidth, tolerance: 0.1);
    }

    [AvaloniaFact(DisplayName = "Saving DataGrid state preserves column width changes when scroll state is unavailable")]
    public async Task saving_datagrid_state_preserves_column_width_changes_when_scroll_state_is_unavailable()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();
        var column = grid.Columns.First();
        column.Width = new DataGridLength(180);
        grid.UpdateLayout();
        await TestApplicationExtensions.WaitForUiAsync();

        window.Content = null;
        await TestApplicationExtensions.WaitForUiAsync();
        vm.DataGridRuntimeState.ShouldNotBeNull();
        vm.DataGridRuntimeState!.Scroll = new DataGridScrollState();

        var changedView = Application.Current.GetRequiredTestService<ResourceListView>();
        changedView.DataContext = vm;
        window.Content = changedView;
        await TestApplicationExtensions.WaitForUiAsync();

        var changedGrid = changedView.FindControl<DataGrid>("PART_Grid");
        changedGrid.ShouldNotBeNull();
        changedGrid.Columns.First().Width = new DataGridLength(240);
        changedGrid.UpdateLayout();
        await TestApplicationExtensions.WaitForUiAsync();

        window.Content = null;
        await TestApplicationExtensions.WaitForUiAsync();

        var restoredView = Application.Current.GetRequiredTestService<ResourceListView>();
        restoredView.DataContext = vm;
        window.Content = restoredView;
        await TestApplicationExtensions.WaitForUiAsync();

        var restoredGrid = restoredView.FindControl<DataGrid>("PART_Grid");
        restoredGrid.ShouldNotBeNull();
        restoredGrid.Columns.First().Width.DisplayValue.ShouldBe(240, tolerance: 0.1);
    }

    [AvaloniaFact(DisplayName = "Namespace filter initializes from selected namespaces")]
    public async Task namespace_filter_initializes_from_selected_namespaces()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        cluster.SelectedNamespaces.Add(NamespaceResource("default"));

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        await TestApplicationExtensions.WaitForUiAsync();

        vm.FilteringModel.Descriptors.Count.ShouldBe(1);
        var descriptor = vm.FilteringModel.Descriptors[0];
        descriptor.Values.Count.ShouldBe(1);
        descriptor.Values[0].ShouldBe("default");
    }

    [AvaloniaFact(DisplayName = "Namespace selector filters the resource list")]
    public async Task namespace_selector_filters_the_resource_list()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();

        await AddOrUpdateAsync(cluster, NamespaceResource("ns1"));
        await AddOrUpdateAsync(cluster, NamespaceResource("ns2"));

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("ns1", "a"));
        await AddOrUpdateAsync(cluster, Pod("ns2", "b"));
        await TestApplicationExtensions.WaitForUiAsync();
        await WaitForAsync(() => vm.View.Count == 2);

        vm.View.Count.ShouldBe(2);

        var selector = view.GetVisualDescendants().OfType<Ursa.Controls.MultiComboBox>().Single();
        selector.SelectedItems.ShouldBeSameAs(vm.SelectedNamespaces);
        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.ShouldNotBeNull();

        var ns1 = cluster.Runtime.Namespaces.Single(x => x.Name() == "ns1");
        selector.IsDropDownOpen = true;
        await TestApplicationExtensions.WaitForUiAsync();

        var item = selector.ContainerFromItem(ns1).ShouldBeOfType<Ursa.Controls.MultiComboBoxItem>();
        item.IsSelected = true;
        selector.IsDropDownOpen = false;
        await TestApplicationExtensions.WaitForUiAsync();

        vm.SelectedNamespaces.Select(x => x.Name()).ShouldBe(["ns1"]);
        await WaitForAsync(() => vm.View.Count == 1);
        vm.View[0].ShouldBeOfType<V1Pod>().Namespace().ShouldBe("ns1");
        grid!.ItemsSource.ShouldBeSameAs(vm.View);

        for (var i = 0; i < 5; i++)
        {
            grid.UpdateLayout();
            await TestApplicationExtensions.WaitForUiAsync();
        }

        var allRows = GetAllRows(grid).ToList();
        allRows.Count.ShouldBeGreaterThan(0);
        allRows.Select(x => (x.DataContext as V1Pod)?.Namespace()).ShouldContain("ns1");
        var rows = allRows.Where(x => x.IsVisible).ToList();
        rows.Count.ShouldBe(1);
        rows[0].DataContext.ShouldBeOfType<V1Pod>().Namespace().ShouldBe("ns1");
    }

    [AvaloniaFact(DisplayName = "Clearing grid filters preserves namespace selector filtering")]
    public async Task clearing_grid_filters_preserves_namespace_selector_filtering()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        await AddOrUpdateAsync(cluster, Pod("ns1", "a"));
        await AddOrUpdateAsync(cluster, Pod("ns2", "b"));
        await WaitForAsync(() => vm.View.Count == 2);

        cluster.SelectedNamespaces.Add(NamespaceResource("ns1"));
        await WaitForAsync(() => vm.View.Count == 1);

        vm.FilteringModel.Clear();
        await WaitForAsync(() => vm.View.Count == 1);

        vm.View[0].ShouldBeOfType<V1Pod>().Namespace().ShouldBe("ns1");
        GetNamespaceFilterValues(vm).ShouldBe(["ns1"]);
    }

    [AvaloniaFact(DisplayName = "Search query is debounced before filtering view")]
    public async Task search_query_is_debounced_before_filtering_view()
    {
        var cluster = await Application.Current.CreateClusterAsync();

        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        await AddOrUpdateAsync(cluster, Pod("ns", "alpha"));
        await AddOrUpdateAsync(cluster, Pod("ns", "beta"));
        await WaitForAsync(() => vm.View.Count == 2);

        vm.View.Count.ShouldBe(2);

        vm.SearchQuery = "alpha";
        await TestApplicationExtensions.WaitForUiAsync();

        vm.View.Count.ShouldBe(2);

        await WaitForAsync(() => vm.View.Count == 1);
        vm.View[0].ShouldBeOfType<V1Pod>().Name().ShouldBe("alpha");
    }

    [AvaloniaFact(DisplayName = "Sorting pods by name orders the resource view")]
    public async Task sorting_pods_by_name_orders_the_resource_view()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await WaitForAsync(() => view.FindControl<DataGrid>("PART_Grid")?.Columns.Count > 0);

        await AddOrUpdateAsync(cluster, Pod("ns", "zeta"));
        await AddOrUpdateAsync(cluster, Pod("ns", "alpha"));
        await AddOrUpdateAsync(cluster, Pod("ns", "middle"));
        await WaitForAsync(() => vm.View.Count == 3);

        var grid = view.FindControl<DataGrid>("PART_Grid").ShouldNotBeNull();
        var nameColumn = grid.Columns.Single(column =>
            string.Equals(column.ColumnKey?.ToString(), "name", StringComparison.Ordinal));
        vm.SortingModel.SetOrUpdate(new(nameColumn, ListSortDirection.Ascending, null, null));

        await WaitForAsync(() => vm.View.Cast<V1Pod>().Select(item => item.Name()).SequenceEqual(["alpha", "middle", "zeta"]));
    }

    [AvaloniaFact(DisplayName = "Sorting pods by name updates rendered row order")]
    public async Task sorting_pods_by_name_updates_rendered_row_order()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await WaitForAsync(() => view.FindControl<DataGrid>("PART_Grid")?.Columns.Count > 0);
        await AddOrUpdateAsync(cluster, Pod("ns", "zeta"));
        await AddOrUpdateAsync(cluster, Pod("ns", "alpha"));
        await AddOrUpdateAsync(cluster, Pod("ns", "middle"));

        var grid = view.FindControl<DataGrid>("PART_Grid").ShouldNotBeNull();
        await WaitForAsync(() => vm.View.Count == 3 && GetAllRows(grid).Any(row => row.IsVisible));

        var nameColumn = grid.Columns.Single(column =>
            string.Equals(column.ColumnKey?.ToString(), "name", StringComparison.Ordinal));
        using var adapter = vm.SortingAdapterFactory.Create(grid, vm.SortingModel);
        adapter.AttachView(grid.CollectionView);
        adapter.HandleHeaderClick(nameColumn, KeyModifiers.None);

        vm.SortingModel.Descriptors.Count.ShouldBe(1);
        vm.SortingModel.Descriptors[0].Direction.ShouldBe(ListSortDirection.Ascending);
    }

    [AvaloniaFact(DisplayName = "Sorting events refreshes virtualized time cells")]
    public async Task sorting_events_refreshes_virtualized_time_cells()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<Corev1Event>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await WaitForAsync(() => view.FindControl<DataGrid>("PART_Grid")?.Columns.Count > 0);

        var now = DateTime.UtcNow;
        var events = Enumerable.Range(0, 100)
            .Select(index => Event(
                "ns",
                $"event-{index:D3}",
                index < 50 ? now.AddMinutes(-1) : now.AddYears(-10)))
            .Select(item =>
            {
                item.Metadata.Uid = item.Name();
                return item;
            })
            .ToArray();
        cluster.Runtime.GetResourceSourceCache<Corev1Event>().Edit(updater => updater.AddOrUpdate(events));

        var grid = view.FindControl<DataGrid>("PART_Grid").ShouldNotBeNull();
        await WaitForAsync(() => vm.View.Count == 100 && GetAllRows(grid).Any(row => row.IsVisible));

        var ageColumn = grid.Columns.Single(column =>
            string.Equals(column.ColumnKey?.ToString(), "age", StringComparison.Ordinal));
        var lastSeenColumn = grid.Columns.Single(column =>
            string.Equals(column.ColumnKey?.ToString(), "last-seen", StringComparison.Ordinal));

        using var adapter = vm.SortingAdapterFactory.Create(grid, vm.SortingModel);
        adapter.AttachView(grid.CollectionView);
        adapter.HandleHeaderClick(ageColumn, KeyModifiers.None);

        await WaitForAsync(() => ((Corev1Event)vm.View[0]).Name() == "event-050");
        Dispatcher.UIThread.RunJobs();

        var visibleRows = GetAllRows(grid)
            .Where(row => row.IsVisible)
            .Select(row =>
            {
                var resource = row.DataContext.ShouldBeOfType<Corev1Event>();
                return (
                    resource,
                    LastSeen: GetCellText(grid, row, lastSeenColumn.DisplayIndex),
                    Age: GetCellText(grid, row, ageColumn.DisplayIndex));
        })
            .ToArray();

        visibleRows.ShouldNotBeEmpty();
        foreach (var row in visibleRows)
        {
            row.resource.Name().ShouldNotBeNull();
            row.resource.Name()!.CompareTo("event-050", StringComparison.Ordinal).ShouldBeGreaterThanOrEqualTo(0);
            row.LastSeen.ShouldStartWith("10y");
            row.Age.ShouldStartWith("10y");
        }
    }

    [AvaloniaFact(DisplayName = "Sorting a large pod list updates the first rendered rows")]
    public async Task sorting_a_large_pod_list_updates_the_first_rendered_rows()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await WaitForAsync(() => view.FindControl<DataGrid>("PART_Grid")?.Columns.Count > 0);

        foreach (var name in new[]
        {
            "actions-runner-controller-8fc4cd56c-zfgg8",
            "immich-backup-29767830-67twc",
            "actions-runner-8fc4cd56c-zfgg8",
        }.Concat(Enumerable.Range(0, 122).Select(index => $"pod-{index:D3}")))
        {
            await AddOrUpdateAsync(cluster, Pod("ns", name));
        }

        var grid = view.FindControl<DataGrid>("PART_Grid").ShouldNotBeNull();
        await WaitForAsync(() => vm.View.Count == 125 && GetAllRows(grid).Any(row => row.IsVisible));
        var nameColumn = grid.Columns.Single(column =>
            string.Equals(column.ColumnKey?.ToString(), "name", StringComparison.Ordinal));
        using var adapter = vm.SortingAdapterFactory.Create(grid, vm.SortingModel);
        adapter.AttachView(grid.CollectionView);
        adapter.HandleHeaderClick(nameColumn, KeyModifiers.None);
        grid.UpdateLayout();
        await TestApplicationExtensions.WaitForUiAsync();

        vm.SortingModel.Descriptors.Count.ShouldBe(1);
        vm.SortingModel.Descriptors[0].Direction.ShouldBe(ListSortDirection.Ascending);

    }

    [AvaloniaFact(DisplayName = "Clicking pod name header reverses name sort")]
    public async Task clicking_pod_name_header_reverses_name_sort()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await WaitForAsync(() => view.FindControl<DataGrid>("PART_Grid")?.Columns.Count > 0);
        await AddOrUpdateAsync(cluster, Pod("ns", "zeta"));
        await AddOrUpdateAsync(cluster, Pod("ns", "alpha"));
        await AddOrUpdateAsync(cluster, Pod("ns", "middle"));
        await WaitForAsync(() => vm.View.Count == 3);

        var grid = view.FindControl<DataGrid>("PART_Grid").ShouldNotBeNull();
        var nameColumn = grid.Columns.Single(column =>
            string.Equals(column.ColumnKey?.ToString(), "name", StringComparison.Ordinal));
        using var adapter = vm.SortingAdapterFactory.Create(grid, vm.SortingModel);
        adapter.AttachView(grid.CollectionView);
        adapter.HandleHeaderClick(nameColumn, KeyModifiers.None);

        vm.SortingModel.Descriptors.Count.ShouldBe(1);
        vm.SortingModel.Descriptors[0].Direction.ShouldBe(ListSortDirection.Ascending);
    }

    [AvaloniaFact(DisplayName = "Attached resource list search filters matching resource")]
    public async Task attached_resource_list_search_filters_matching_resource()
    {
        using var window = Application.Current.CreateTestWindow();
        var cluster = await Application.Current.CreateClusterAsync();
        var vm = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = vm;
        window.Content = view;
        window.Show();

        await WaitForAsync(() => view.FindControl<DataGrid>("PART_Grid")?.Columns.Count > 0, timeoutMs: 5000);

        await AddOrUpdateAsync(cluster, Pod("ns", "alpha"));
        await AddOrUpdateAsync(cluster, Pod("ns", "beta"));
        await AddOrUpdateAsync(cluster, Pod("ns", "gamma"));
        await WaitForAsync(() => vm.View.Count == 3, timeoutMs: 5000);

        vm.SearchQuery = "beta";
        await WaitForAsync(() => vm.View.Count == 1, timeoutMs: 5000);

        vm.View[0].ShouldBeOfType<V1Pod>().Name().ShouldBe("beta");
        vm.SearchModel.Descriptors.Single().Query.ShouldBe("beta");
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
        await TestWait.UntilAsync(
            predicate,
            timeoutMs,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());
    }

    private static async Task<T> WaitForValueAsync<T>(Func<T?> getter, int timeoutMs = 1000) where T : class
    {
        return (await TestWait.UntilValueAsync(
            getter,
            timeoutMs,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs())).ShouldNotBeNull()!;
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
    public Avalonia.Resources.SortDirection Sort { get; set; } = Avalonia.Resources.SortDirection.None;
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
    public DataGridState? DataGridRuntimeState { get; set; }
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
    public DataGridState? DataGridRuntimeState { get; set; }
}
