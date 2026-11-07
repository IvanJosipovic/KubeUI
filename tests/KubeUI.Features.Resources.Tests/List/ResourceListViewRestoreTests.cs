using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using k8s.Models;
using Shouldly;
using SvcSystems.Avalonia.DynamicTableView;

namespace KubeUI.Features.Resources.Tests.List;

public sealed class ResourceListViewRestoreTests
{
    [AvaloniaFact]
    public async Task Replacing_data_context_replaces_source_and_ignores_stale_restore_callbacks()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var firstViewModel = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        using var secondViewModel = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        firstViewModel.SearchQuery = "first-target";
        secondViewModel.SearchQuery = "second-target";
        firstViewModel.Initialize(cluster);
        secondViewModel.Initialize(cluster);
        firstViewModel.Objects.AddOrUpdate([Pod("team-a", "first-target")]);
        secondViewModel.Objects.AddOrUpdate([Pod("team-a", "second-target")]);
        await WaitForAsync(() => firstViewModel.ItemCount == 1 && secondViewModel.ItemCount == 1);

        firstViewModel.SelectionModel.Select(0);
        firstViewModel.CaptureSelectionState();
        secondViewModel.SelectionModel.Select(0);
        secondViewModel.CaptureSelectionState();
        var staleColumns = firstViewModel.TableSource.Columns
            .Select((column, index) => new DynamicTableViewColumnState(column.Key, index, 140))
            .ToArray();
        (staleColumns[0], staleColumns[1]) = (staleColumns[1] with { Order = 0 }, staleColumns[0] with { Order = 1 });
        firstViewModel.TableViewRuntimeState = new(staleColumns, [], [], default);
        var currentHeaders = secondViewModel.TableSource.Columns
            .Select(static column => column.Header.ToString() ?? string.Empty)
            .ToArray();
        secondViewModel.TableViewRuntimeState = new(
            secondViewModel.TableSource.Columns
                .Select((column, index) => new DynamicTableViewColumnState(column.Key, index, 220))
                .ToArray(),
            [], [], default);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = firstViewModel;
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();

        // Change the context before background-priority restoration runs. The queued callback for
        // the first source must be ignored; the second source and selection must win.
        view.DataContext = secondViewModel;
        var table = await WaitForTableAsync(view);
        await WaitForHeadersAsync(table, currentHeaders);
        await WaitForAsync(() => table.Source == secondViewModel.TableSource
            && table.GetVisualDescendants().OfType<TableViewRow>().Count() == 1);

        ReferenceEquals(table.Source, firstViewModel.TableSource).ShouldBeFalse();
        table.Source.ShouldBeSameAs(secondViewModel.TableSource);
        table.GetVisualDescendants().OfType<TableViewRow>()
            .Select(static row => ((V1Pod)row.DataContext!).Metadata.Name)
            .ShouldBe(["second-target"]);
        secondViewModel.SelectionModel.SelectedItems.Cast<V1Pod>()
            .Select(static pod => pod.Metadata.Name)
            .ShouldBe(["second-target"]);
        ReadHeaderLabels(table).ShouldBe(currentHeaders);
    }

    [AvaloniaFact]
    public async Task Resource_list_headers_remain_visible_after_restore_and_tab_reactivation()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var viewModel = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        viewModel.Initialize(cluster);
        viewModel.Objects.AddOrUpdate([Pod("team-a", "api"), Pod("team-a", "worker")]);
        await WaitForAsync(() => viewModel.ItemCount == 2);

        var originalHeaders = viewModel.TableSource.Columns.Select(static column => column.Header.ToString() ?? string.Empty).ToArray();
        var expectedHeaders = originalHeaders.ToArray();
        var savedColumns = viewModel.TableSource.Columns
            .Select((column, index) => new DynamicTableViewColumnState(column.Key, index, 140))
            .ToArray();
        savedColumns[0] = savedColumns[0] with { Order = 1 };
        savedColumns[1] = savedColumns[1] with { Order = 0 };
        (expectedHeaders[0], expectedHeaders[1]) = (expectedHeaders[1], expectedHeaders[0]);
        viewModel.TableViewRuntimeState = new(savedColumns, [], [], default);

        var view = Application.Current.GetRequiredTestService<ResourceListView>();
        view.DataContext = viewModel;
        TabControl tabs = new()
        {
            Items =
            {
                new TabItem { Header = "Resources", Content = view },
                new TabItem { Header = "Other", Content = new Border() }
            }
        };
        using var window = Application.Current.CreateTestWindow(content: tabs);
        window.Show();

        var table = await WaitForTableAsync(view);
        await WaitForHeadersAsync(table, expectedHeaders);
        ReadHeaderLabels(table).ShouldBe(expectedHeaders);
        viewModel.TableSource.Columns.Take(2).Select(static column => column.Key).ShouldBe(["containers", "name"]);

        tabs.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();
        tabs.SelectedIndex = 0;
        await WaitForHeadersAsync(table, expectedHeaders);

        ReadHeaderLabels(table).ShouldBe(expectedHeaders);
    }

    private static async Task<DynamicTableView> WaitForTableAsync(ResourceListView view)
        => await TestWait.UntilValueAsync(
            () => view.FindControl<DynamicTableView>("PART_Grid"),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs())
            ?? throw new TimeoutException("Resource list table did not attach.");

    private static Task WaitForHeadersAsync(DynamicTableView table, string[] expectedHeaders)
        => WaitForAsync(() => ReadHeaderLabels(table).SequenceEqual(expectedHeaders));

    private static string[] ReadHeaderLabels(DynamicTableView table)
        => table.GetVisualDescendants().OfType<TableViewColumnHeader>()
            .SelectMany(static header => header.GetVisualDescendants().OfType<TextBlock>())
            .Where(static label => label.IsVisible && label.Bounds.Width > 0 && label.Bounds.Height > 0)
            .Select(static label => label.Text ?? string.Empty)
            .ToArray();

    private static Task WaitForAsync(Func<bool> condition)
        => TestWait.UntilAsync(
            condition,
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());

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
}
