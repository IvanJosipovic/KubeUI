using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DynamicData;
using k8s.Models;

namespace KubeUI.Avalonia.Tests.Features.Resources.List;

public sealed class ResourceListViewRestoreTests
{
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
        Assert.Equal(expectedHeaders, ReadHeaderLabels(table));
        Assert.Equal(["containers", "name"], viewModel.TableSource.Columns.Take(2).Select(static column => column.Key));

        tabs.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();
        tabs.SelectedIndex = 0;
        await WaitForHeadersAsync(table, expectedHeaders);

        Assert.Equal(expectedHeaders, ReadHeaderLabels(table));
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
