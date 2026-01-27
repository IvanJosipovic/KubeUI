using Avalonia.Threading;
using Avalonia.Headless.XUnit;
using FluentAssertions;
using k8s.Models;
using KubeUI.ViewModels;
using KubeUI.Tests.Infra;
using Avalonia;
using Avalonia.Controls;
using KubeUI.Views;
using System.Reflection;
using Avalonia.VisualTree;
using FluentAvalonia.Core;

namespace KubeUI.Tests;

public class ResourceListViewTests
{
    private static V1Pod Pod(string ns, string name)
        => new()
        {
            ApiVersion = V1Pod.KubeApiVersion + "/" + V1Pod.KubeApiVersion,
            Kind = V1Pod.KubeKind,
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = ns,
                Name = name,
                CreationTimestamp = DateTime.UtcNow,
            }
        };

    private static async Task AddOrUpdateAsync(TestCluster cluster, V1Pod pod)
    {
        await cluster.AddOrUpdateResource(pod);
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


    [AvaloniaFact]
    public async Task MultiSelect_replace_middle_preserves_all_selected()
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

    [AvaloniaFact]
    public async Task SingleSelect_replace_preserves_only_selected_item_no_fallback_neighbor()
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

    [AvaloniaFact]
    public async Task Refresh_in_place_mutation_updates_grid_text()
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
        grid = grid!;

        var pod = Pod("ns", "a");
        await AddOrUpdateAsync(cluster, pod);

        var before = GetFirstRowFirstColumnText(grid, 0, 0);
        before.Should().NotBeNull();
        before.Should().Contain("a");

        // Mutate in place and trigger DynamicData refresh.
        pod.Metadata.Name = "a2";
        await AddOrUpdateAsync(cluster, pod);

        var after = GetFirstRowFirstColumnText(grid, 0, 0);
        after.Should().NotBeNull();
        after.Should().Contain("a2");
    }
}
