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
using Avalonia.Styling;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.Core;
using KubeUI.Avalonia;
using KubeUI.Avalonia.Behaviors;
using KubeUI.Avalonia.Resources;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Avalonia.ViewModels;
using KubeUI.Avalonia.Views;

namespace KubeUI.Avalonia.Tests;

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
        var window = new Window
        {
            Width = 1200,
            Height = 800
        };
        var cluster = new TestCluster().CreateWorkspace();

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredService<ResourceListView>();
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
        var window = new Window
        {
            Width = 1200,
            Height = 800
        };
        var cluster = new TestCluster().CreateWorkspace();

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredService<ResourceListView>();
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
        var window = new Window
        {
            Width = 1200,
            Height = 800
        };
        var cluster = new TestCluster().CreateWorkspace();

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<Corev1Event>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredService<ResourceListView>();
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
        var window = new Window
        {
            Width = 1200,
            Height = 800
        };
        var cluster = new TestCluster().CreateWorkspace();

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<Corev1Event>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredService<ResourceListView>();
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
        var window = new Window
        {
            Width = 1200,
            Height = 800
        };

        var cluster = new TestCluster().CreateWorkspace();

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredService<ResourceListView>();
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
        var window = new Window
        {
            Width = 1200,
            Height = 800
        };

        var cluster = new TestCluster().CreateWorkspace();

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Namespace>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredService<ResourceListView>();
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

    [AvaloniaFact(DisplayName = "Namespace filter preserves selection when included")]
    public async Task namespace_filter_preserves_selection_when_included()
    {
        var window = new Window
        {
            Width = 1200,
            Height = 800
        };
        var cluster = new TestCluster().CreateWorkspace();

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredService<ResourceListView>();
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
        var window = new Window
        {
            Width = 1200,
            Height = 800
        };
        var cluster = new TestCluster().CreateWorkspace();

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredService<ResourceListView>();
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
        var window = new Window
        {
            Width = 1200,
            Height = 800
        };
        var cluster = new TestCluster().CreateWorkspace();

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredService<ResourceListView>();
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

    [AvaloniaFact(DisplayName = "Pod-specific actions are hidden for multi-select")]
    public async Task pod_specific_actions_are_hidden_for_multi_select()
    {
        var cluster = new TestCluster().CreateWorkspace();

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Pod>>();
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
        var window = new Window
        {
            Width = 1200,
            Height = 800
        };

        var cluster = new TestCluster().CreateWorkspace();

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredService<ResourceListView>();
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
        var factory = Application.Current.GetRequiredService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        var dockControl = new DockControl
        {
            Layout = layout,
        };

        var window = new Window
        {
            Content = dockControl,
            Width = 1200,
            Height = 800
        };
        var cluster = new TestCluster().CreateWorkspace();

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Namespace>>();
        vm.Initialize(cluster);

        window.Show();

        var otherDockable = Application.Current.GetRequiredService<AboutViewModel>();
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

        var labelsColumn = vm.ColumnDefinitions.First(x => Equals(x.ColumnKey, "labels"));

        vm.SortingModel.Clear();

        vm.SortingModel.SetOrUpdate(new(labelsColumn, ListSortDirection.Descending, null, labelsColumn.CustomSortComparer));

        Dispatcher.UIThread.RunJobs();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        Dispatcher.UIThread.RunJobs();

        var view = WaitForValue(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        view.ShouldNotBeNull();

        vm.SortingModel.Descriptors.Count.ShouldBe(1);
        ((DataGridControlTemplateColumnDefinition)(vm.SortingModel.Descriptors[0].ColumnId)).ColumnKey.ShouldBe("labels");

        factory.SetActiveDockable(otherDockable);
        factory.SetFocusedDockable(documents, otherDockable);
        Dispatcher.UIThread.RunJobs();

        factory.SetActiveDockable(vm);
        factory.SetFocusedDockable(documents, vm);
        Dispatcher.UIThread.RunJobs();

        var restoredView = WaitForValue(() => FindVisibleView<ResourceListView>(window, vm), 3000);
        restoredView.ShouldNotBeNull();

        vm.SortingModel.Descriptors.Count.ShouldBe(1);
        ((DataGridControlTemplateColumnDefinition)(vm.SortingModel.Descriptors[0].ColumnId)).ColumnKey.ShouldBe("labels");
    }

    [AvaloniaFact(DisplayName = "Namespace filter initializes from selected namespaces")]
    public async Task namespace_filter_initializes_from_selected_namespaces()
    {
        var window = new Window
        {
            Width = 1200,
            Height = 800
        };
        var cluster = new TestCluster().CreateWorkspace();

        cluster.SelectedNamespaces.Add(NamespaceResource("default"));

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        Dispatcher.UIThread.RunJobs();

        vm.FilteringModel.Descriptors.Count.ShouldBe(1);
        var descriptor = vm.FilteringModel.Descriptors[0];
        descriptor.Values.Count.ShouldBe(1);
        descriptor.Values[0].ShouldBe("default");
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
    public GroupApiVersionKind Kind => GroupApiVersionKind.From<V1Pod>();
    public int ItemCount => View.Count;
    public string SearchQuery { get; set; } = string.Empty;
    public ISettingsService SettingsService => Application.Current.GetRequiredService<ISettingsService>();
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
    public bool ShowNewResource => true;
    public bool IsCustomResource => false;
    public GroupApiVersionKind Kind => GroupApiVersionKind.From<V1Pod>();
    public IList<IResourceListColumn> Columns() => Array.Empty<IResourceListColumn>();
    public IEnumerable<MenuItemViewModel> GetDefaultMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public IEnumerable<MenuItemViewModel> GetCustomMenuItems(IEnumerable? selectedItems) => Array.Empty<MenuItemViewModel>();
    public int Order => 0;
    public string Name => "Pods";
    public string? Category => null;
    public IStyle ListStyle() => null;
    public Task UpdatePermissions() => Task.CompletedTask;
    public Type Type => typeof(V1Pod);
    public IRelayCommand NewResourceCommand => new RelayCommand(() => { });
    public IRelayCommand<IList> ViewCommand { get; }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
    }
}

