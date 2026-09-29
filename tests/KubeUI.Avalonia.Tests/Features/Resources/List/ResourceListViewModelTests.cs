using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using DynamicData;
using k8s.Models;
using KubeUI.Avalonia.Features.AI;
using Shouldly;
using SvcSystems.Avalonia.DynamicTableView;

namespace KubeUI.Avalonia.Tests.Features.Resources.List;

public sealed class ResourceListViewModelTests
{
    [AvaloniaFact]
    public async Task Namespace_scope_search_filter_and_sort_compose_in_the_library_source()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var viewModel = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        viewModel.Initialize(cluster);
        viewModel.Objects.AddOrUpdate([
            Pod("team-a", "api-a"),
            Pod("team-a", "worker-a"),
            Pod("team-b", "api-b")]);

        await WaitForAsync(() => viewModel.ItemCount == 3);
        viewModel.TableSource.SetSort([new("name", ListSortDirection.Descending)]);
        cluster.SelectedNamespaces.Add(NamespaceResource("team-a"));
        await WaitForAsync(() => viewModel.ItemCount == 2);

        viewModel.SearchQuery = "api";
        await WaitForAsync(() => viewModel.ItemCount == 1);
        Rows(viewModel).ShouldBe(["api-a"]);

        viewModel.TableSource.SetFilter(new("name", DynamicTableViewFilterOperator.Contains, "worker"));
        await WaitForAsync(() => viewModel.ItemCount == 0);
        viewModel.TableSource.ClearFilters();
        await WaitForAsync(() => viewModel.ItemCount == 1);

        viewModel.SearchQuery = string.Empty;
        await WaitForAsync(() => viewModel.ItemCount == 2);
        Rows(viewModel).ShouldBe(["worker-a", "api-a"]);
    }

    [AvaloniaFact]
    public async Task Local_namespace_scope_decouples_then_relinks_to_cluster_selection()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        cluster.SelectedNamespaces.Add(NamespaceResource("team-a"));
        using var viewModel = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        viewModel.Initialize(cluster);
        viewModel.Objects.AddOrUpdate([Pod("team-a", "api-a"), Pod("team-b", "api-b")]);
        await WaitForAsync(() => viewModel.ItemCount == 1);

        viewModel.IsNamespaceSelectionLinked = false;
        cluster.SelectedNamespaces.Add(NamespaceResource("team-b"));
        await WaitForAsync(() => viewModel.SelectedNamespaces.Count == 1);
        Rows(viewModel).ShouldBe(["api-a"]);

        viewModel.SelectedNamespaces.Add(NamespaceResource("team-b"));
        await WaitForAsync(() => viewModel.ItemCount == 2);
        viewModel.IsNamespaceSelectionLinked = true;
        await WaitForAsync(() => viewModel.ItemCount == 2);
        ReferenceEquals(viewModel.SelectedNamespaces, cluster.SelectedNamespaces).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task Keyed_selection_tracks_an_informer_replacement_in_the_visible_rows()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var viewModel = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        viewModel.Initialize(cluster);
        var selected = Pod("team-a", "selected");
        viewModel.Objects.AddOrUpdate([selected, Pod("team-a", "other")]);
        await WaitForAsync(() => viewModel.ItemCount == 2);

        var selectedIndex = viewModel.TableSource.Items.Cast<V1Pod>().ToList().FindIndex(item => item.Name() == "selected");
        viewModel.SelectionModel.Select(selectedIndex);
        var replacement = Pod("team-a", "selected");
        replacement.Spec = new V1PodSpec { NodeName = "replacement-node" };
        viewModel.Objects.AddOrUpdate(replacement);

        await WaitForAsync(() => ReferenceEquals(viewModel.SelectedItem, replacement));
        viewModel.SelectedItem!.Spec!.NodeName.ShouldBe("replacement-node");
    }

    [AvaloniaFact]
    public async Task Selection_updates_the_kubernetes_agent_context()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var viewModel = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        var contextService = Application.Current.GetRequiredTestService<IAgentContextService>();
        viewModel.Initialize(cluster);
        viewModel.Objects.AddOrUpdate([Pod("team-a", "api"), Pod("team-a", "worker")]);
        await WaitForAsync(() => viewModel.ItemCount == 2);

        viewModel.SelectionModel.Select(0);
        viewModel.SelectionModel.Select(1);
        await WaitForAsync(() => contextService.Context?.SelectedResources.Count == 2);

        contextService.Context!.Namespace.ShouldBe("team-a");
        contextService.Context.SelectedResources.Select(static resource => resource.Name).Order().ShouldBe(["api", "worker"]);
    }

    [AvaloniaFact]
    public async Task Resource_list_view_binds_the_dynamic_table_source_and_resource_columns()
    {
        using var window = Application.Current.CreateTestWindow();
        using var cluster = await Application.Current.CreateClusterAsync();
        using var viewModel = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        viewModel.Initialize(cluster);
        viewModel.Objects.AddOrUpdate(Pod("team-a", "api"));

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = viewModel;
        window.Content = view;
        window.Show();
        var table = await TestWait.UntilValueAsync(
            () => view.FindControl<DynamicTableView>("PART_Grid"),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs()) ?? throw new TimeoutException("Dynamic table did not attach.");

        await WaitForAsync(() => viewModel.ItemCount == 1);
        table.Source.ShouldBeSameAs(viewModel.TableSource);
        table.Columns.Count.ShouldBe(viewModel.ResourceConfig.Columns().Count);
        Rows(viewModel).ShouldBe(["api"]);
    }

    private static string[] Rows(ResourceListViewModel<V1Pod> viewModel)
        => viewModel.TableSource.Items.Cast<V1Pod>().Select(static pod => pod.Name()!).ToArray();

    private static V1Pod Pod(string ns, string name)
        => new()
        {
            ApiVersion = V1Pod.KubeApiVersion,
            Kind = V1Pod.KubeKind,
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = ns,
                Name = name,
                CreationTimestamp = DateTime.UtcNow
            }
        };

    private static V1Namespace NamespaceResource(string name)
        => new() { Metadata = new V1ObjectMeta { Name = name } };

    private static Task WaitForAsync(Func<bool> condition)
        => TestWait.UntilAsync(
            condition,
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());
}
