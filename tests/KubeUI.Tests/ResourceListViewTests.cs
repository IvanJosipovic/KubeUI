using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAssertions;
using FluentAvalonia.Core;
using k8s;
using k8s.Models;
using KubeUI.Tests.Infra;
using KubeUI.ViewModels;
using KubeUI.Views;

namespace KubeUI.Tests;

public class ResourceListViewTests
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

    private static async Task AddOrUpdateAsync<T>(TestCluster cluster, T resource) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await cluster.AddOrUpdateResource(resource);
        Dispatcher.UIThread.RunJobs();
    }

    private static IEnumerable<DataGridRow> GetAllRows(DataGrid grid)
    {
        var mi = grid.GetType().GetMethod("GetAllRows", BindingFlags.Instance | BindingFlags.NonPublic);
        mi.Should().NotBeNull("ProDataGrid DataGrid should expose internal GetAllRows()");
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
        dataGridRow.Should().NotBeNull();

        return GetCellText(grid, dataGridRow!, column);
    }


    [AvaloniaFact(DisplayName = "All select update middle")]
    public async Task all_select_update_middle_preserves_all_selected()
    {
        var window = new MainWindow
        {
            Width = 1200,
            Height = 800
        };
        var cluster = new TestCluster();

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

        vm.SelectionModel.SelectedIndexes.Should().BeEquivalentTo([0, 1, 2]);

        // Replace 'b' with a new instance (same key)
        await AddOrUpdateAsync(cluster, Pod("ns", "b"));

        vm.SelectionModel.SelectedIndexes.Should().BeEquivalentTo([0, 1, 2]);

        vm.SelectedItems.Count.Should().Be(3);

        vm.SelectedItems[0].Namespace().Should().Be("ns");
        vm.SelectedItems[0].Name().Should().Be("a");
        vm.SelectedItems[1].Namespace().Should().Be("ns");
        vm.SelectedItems[1].Name().Should().Be("b");
        vm.SelectedItems[2].Namespace().Should().Be("ns");
        vm.SelectedItems[2].Name().Should().Be("c");
    }

    [AvaloniaFact(DisplayName = "Single select update middle")]
    public async Task single_select_update__preserves_only_selected()
    {
        var window = new MainWindow
        {
            Width = 1200,
            Height = 800
        };
        var cluster = new TestCluster();

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

        vm.SelectionModel.SelectedIndexes.Should().BeEquivalentTo([1]);

        // Replace 'b' with new instance (same key)
        await AddOrUpdateAsync(cluster, Pod("ns", "b"));

        vm.SelectionModel.SelectedIndexes.Should().BeEquivalentTo([1]);

        vm.SelectedItems.Count.Should().Be(1);

        vm.SelectedItems[0].Namespace().Should().Be("ns");
        vm.SelectedItems[0].Name().Should().Be("b");

        vm.SelectedItem.Namespace().Should().Be("ns");
        vm.SelectedItem.Name().Should().Be("b");
    }

    [AvaloniaFact(DisplayName = "Single select with sort due to update")]
    public async Task single_select_with_sort_preserves_only_selected()
    {
        var window = new MainWindow
        {
            Width = 1200,
            Height = 800
        };
        var cluster = new TestCluster();

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

        vm.View.ElementAt(0).As<Corev1Event>().Name().Should().Be("c");
        vm.View.ElementAt(1).As<Corev1Event>().Name().Should().Be("b");
        vm.View.ElementAt(2).As<Corev1Event>().Name().Should().Be("a");


        // Select only middle
        vm.SelectionModel.Select(1);

        vm.SelectionModel.SelectedIndexes.Should().BeEquivalentTo([1]);

        // Replace 'b' with new instance (same key)
        await AddOrUpdateAsync(cluster, Event("ns", "b"));


        vm.View.ElementAt(0).As<Corev1Event>().Name().Should().Be("b");
        vm.View.ElementAt(1).As<Corev1Event>().Name().Should().Be("c");
        vm.View.ElementAt(2).As<Corev1Event>().Name().Should().Be("a");

        vm.SelectionModel.SelectedIndexes.Should().BeEquivalentTo([0]);

        vm.SelectedItems.Count.Should().Be(1);

        vm.SelectedItems[0].Namespace().Should().Be("ns");
        vm.SelectedItems[0].Name().Should().Be("b");


        vm.SelectedItem.Namespace().Should().Be("ns");
        vm.SelectedItem.Name().Should().Be("b");
    }

    [AvaloniaFact(DisplayName = "All select with sort due to update")]
    public async Task all_select_with_sort_preserves_all_selected()
    {
        var window = new MainWindow
        {
            Width = 1200,
            Height = 800
        };
        var cluster = new TestCluster();

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

        vm.View.ElementAt(0).As<Corev1Event>().Name().Should().Be("c");
        vm.View.ElementAt(1).As<Corev1Event>().Name().Should().Be("b");
        vm.View.ElementAt(2).As<Corev1Event>().Name().Should().Be("a");


        // Select all 3
        vm.SelectionModel.Select(0);
        vm.SelectionModel.Select(1);
        vm.SelectionModel.Select(2);

        vm.SelectionModel.SelectedIndexes.Should().BeEquivalentTo([0, 1, 2]);

        // Replace 'b' with new instance (same key)
        await AddOrUpdateAsync(cluster, Event("ns", "b"));

        vm.View.ElementAt(0).As<Corev1Event>().Name().Should().Be("b");
        vm.View.ElementAt(1).As<Corev1Event>().Name().Should().Be("c");
        vm.View.ElementAt(2).As<Corev1Event>().Name().Should().Be("a");

        vm.SelectionModel.SelectedIndexes.Should().BeEquivalentTo([0, 1, 2]);

        vm.SelectedItems.Count.Should().Be(3);

        vm.SelectedItems[0].Namespace().Should().Be("ns");
        vm.SelectedItems[0].Name().Should().Be("b");


        vm.SelectedItem.Namespace().Should().Be("ns");
        vm.SelectedItem.Name().Should().Be("b");
    }

    [AvaloniaFact(DisplayName = "Update check DataGrid Text update")]
    public async Task UpdateResourceTextBox()
    {
        var window = new MainWindow
        {
            Width = 1200,
            Height = 800
        };

        var cluster = new TestCluster();

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.Should().NotBeNull();


        var pod = Pod("ns", "a");
        await AddOrUpdateAsync(cluster, pod);

        var before = GetFirstRowFirstColumnText(grid, 0, 0);
        before.Should().NotBeNull();
        before.Should().Contain("a");

        // Mutate in place and trigger DynamicData refresh.
        pod.Metadata.Name = "b";
        await AddOrUpdateAsync(cluster, pod);

        var after = GetFirstRowFirstColumnText(grid, 0, 0);
        after.Should().NotBeNull();
        after.Should().Contain("b");
    }

    [AvaloniaFact(DisplayName = "Update check DataGrid Text update2")]
    public async Task UpdateResourceTextBox2()
    {
        var window = new MainWindow
        {
            Width = 1200,
            Height = 800
        };

        var cluster = new TestCluster();

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Namespace>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        var grid = view.FindControl<DataGrid>("PART_Grid");
        grid.Should().NotBeNull();

        var ns = new V1Namespace()
        {
            Metadata = new()
            {
                Name = "a"
            }
        };

        await AddOrUpdateAsync(cluster, ns);

        var before = GetFirstRowFirstColumnText(grid, 0, 1);
        before.Should().NotBeNull();
        before.Should().BeEmpty();

        ns.Metadata.Labels = new Dictionary<string, string>()
        {
            {"test", "value" }
        };

        await AddOrUpdateAsync(cluster, ns);

        var after = GetFirstRowFirstColumnText(grid, 0, 1);
        after.Should().NotBeNull();
        after.Should().Contain("test=value");
    }

    [AvaloniaFact(DisplayName = "Namespace filter preserves selection when included")]
    public async Task namespace_filter_preserves_selection_when_included()
    {
        var window = new MainWindow
        {
            Width = 1200,
            Height = 800
        };
        var cluster = new TestCluster();

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

        vm.SelectedItem.Should().NotBeNull();
        vm.SelectedItem!.Namespace().Should().Be("ns1");
        vm.SelectedItem.Name().Should().Be("a");
    }

    [AvaloniaFact(DisplayName = "Delete Resource")]
    public async Task delete_resource()
    {
        var window = new MainWindow
        {
            Width = 1200,
            Height = 800
        };

        var cluster = new TestCluster();

        var vm = Application.Current.GetRequiredService<ResourceListViewModel<V1Pod>>();
        vm.Initialize(cluster);

        var view = Application.Current.GetRequiredService<ResourceListView>();
        view.DataContext = vm;

        window.Content = view;
        window.Show();

        await AddOrUpdateAsync(cluster, Pod("ns1", "a"));

        vm.View.Count().Should().Be(1);

        await cluster.DeleteResource(Pod("ns1", "a"));
        Dispatcher.UIThread.RunJobs();

        vm.View.Count().Should().Be(0);
    }
}
