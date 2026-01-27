using Avalonia.Threading;
using Avalonia.Headless.XUnit;
using FluentAssertions;
using k8s.Models;
using KubeUI.ViewModels;
using KubeUI.Tests.Infra;
using Avalonia;
using KubeUI.Views;

namespace KubeUI.Tests;

public class ResourceListViewTests
{
    private static V1Pod Pod(string ns, string name)
        => new()
        {
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

    [AvaloniaFact]
    public async Task MultiSelect_replace_middle_preserves_all_selected()
    {
        var window = new MainWindow();
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

        Dispatcher.UIThread.RunJobs();
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
        var window = new MainWindow();
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

        Dispatcher.UIThread.RunJobs();
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
}
