using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using DynamicData;
using k8s.Models;
using KubeUI.Avalonia.Shell.Documents.About;

namespace KubeUI.Avalonia.Tests.Features.Resources.List;

public sealed class ResourceListViewSelectionTests
{
    [AvaloniaFact]
    public void Restore_selection_state_without_an_initialized_source_is_safe()
    {
        using var viewModel = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();

        viewModel.RestoreSelectionState();
    }

    [AvaloniaFact]
    public async Task Resource_list_selection_is_restored_after_tab_reactivation()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var viewModel = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        viewModel.Initialize(cluster);
        viewModel.Objects.AddOrUpdate(Enumerable.Range(0, 10)
            .Select(index => Pod("team-a", $"pod-{index:D2}")));
        await WaitForAsync(() => viewModel.ItemCount == 10);

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
        var table = await TestWait.UntilValueAsync(
            () => view.FindControl<DynamicTableView>("PART_Grid"),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs())
            ?? throw new TimeoutException("Resource list table did not attach.");
        await WaitForAsync(() => viewModel.TableSource.Items.Cast<V1Pod>().Count() == 10 &&
            table.GetVisualDescendants().OfType<TableViewRow>().Count() == 10);
        var attachCount = 1;
        var detachCount = 0;
        table.AttachedToVisualTree += (_, _) => attachCount++;
        table.DetachedFromVisualTree += (_, _) => detachCount++;

        SelectTenRows(window, table);
        Assert.Equal(10, viewModel.SelectionModel.SelectedItems.Count);

        tabs.SelectedIndex = 1;
        await WaitForAsync(() => detachCount == 1);
        Assert.Equal(10, viewModel.SelectionModel.SelectedItems.Count);
        tabs.SelectedIndex = 0;
        await WaitForAsync(() => attachCount == 2);

        Assert.Equal(10, viewModel.SelectionModel.SelectedItems.Count);

        SelectOneRow(window, table, "pod-05");
        Assert.Equal(["pod-05"],
            viewModel.SelectionModel.SelectedItems.Cast<V1Pod>().Select(static pod => pod.Metadata.Name));

        tabs.SelectedIndex = 1;
        await WaitForAsync(() => detachCount == 2);
        tabs.SelectedIndex = 0;
        await WaitForAsync(() => attachCount == 3);

        Assert.Equal(["pod-05"],
            viewModel.SelectionModel.SelectedItems.Cast<V1Pod>().Select(static pod => pod.Metadata.Name));
    }

    [AvaloniaFact]
    public async Task Docked_resource_selection_updates_survive_repeated_tab_activation()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var viewModel = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        viewModel.Initialize(cluster);
        viewModel.Objects.AddOrUpdate(Enumerable.Range(0, 10)
            .Select(index => Pod("team-a", $"dock-pod-{index:D2}")));
        await WaitForAsync(() => viewModel.ItemCount == 10);

        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents")!;
        using var window = Application.Current.CreateTestWindow(content: new DockControl { Layout = layout });
        window.Show();

        var otherDocument = Application.Current.GetRequiredTestService<AboutViewModel>();
        otherDocument.Id = $"{nameof(ResourceListViewSelectionTests)}-{Guid.NewGuid():N}";
        factory.AddToDocuments(viewModel);
        factory.AddToDocuments(otherDocument);
        factory.SetActiveDockable(viewModel);
        factory.SetFocusedDockable(documents, viewModel);
        await WaitForAsync(() => ReferenceEquals(documents.ActiveDockable, viewModel));

        var table = await TestWait.UntilValueAsync(
            () => window.GetVisualDescendants().OfType<DynamicTableView>().FirstOrDefault(),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs())
            ?? throw new TimeoutException("Docked resource list table did not attach.");
        await WaitForAsync(() => table.GetVisualDescendants().OfType<TableViewRow>().Count() == 10);

        SelectRowsByName(window, table, "dock-pod-00", "dock-pod-09");
        Assert.Equal(10, viewModel.SelectionModel.SelectedItems.Count);

        factory.SetActiveDockable(otherDocument);
        factory.SetFocusedDockable(documents, otherDocument);
        await WaitForAsync(() => ReferenceEquals(documents.ActiveDockable, otherDocument));
        Assert.Equal(10, viewModel.SelectionModel.SelectedItems.Count);
        factory.SetActiveDockable(viewModel);
        factory.SetFocusedDockable(documents, viewModel);
        await WaitForAsync(() => ReferenceEquals(documents.ActiveDockable, viewModel));
        table = await WaitForDockTableAsync(window, table);
        await WaitForAsync(() => viewModel.SelectionModel.SelectedItems.Count == 10);
        Assert.Equal(10, viewModel.SelectionModel.SelectedItems.Count);

        SelectOneRow(window, table, "dock-pod-05");
        Assert.Equal(["dock-pod-05"],
            viewModel.SelectionModel.SelectedItems.Cast<V1Pod>().Select(static pod => pod.Metadata.Name));

        var selectionChangesDuringRestore = new List<string>();
        viewModel.SelectionModel.SelectionChanged += (_, _) => selectionChangesDuringRestore.Add(
            string.Join(",", viewModel.SelectionModel.SelectedItems.Cast<V1Pod>().Select(static pod => pod.Metadata.Name)));
        var sourceChangesDuringRestore = 0;
        EventHandler sourceChanged = (_, _) => Interlocked.Increment(ref sourceChangesDuringRestore);
        viewModel.TableSource.Changed += sourceChanged;
        factory.SetActiveDockable(otherDocument);
        factory.SetFocusedDockable(documents, otherDocument);
        await WaitForAsync(() => ReferenceEquals(documents.ActiveDockable, otherDocument));
        Assert.Equal(viewModel.TableViewRuntimeState!.Sorts, viewModel.TableSource.SortDescriptors);
        Assert.Equal(viewModel.TableViewRuntimeState.Filters, viewModel.TableSource.FilterDescriptors);
        factory.SetActiveDockable(viewModel);
        factory.SetFocusedDockable(documents, viewModel);
        await WaitForAsync(() => ReferenceEquals(documents.ActiveDockable, viewModel));
        table = await WaitForDockTableAsync(window, table);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        Assert.Equal(["dock-pod-05"],
            viewModel.SelectionModel.SelectedItems.Cast<V1Pod>().Select(static pod => pod.Metadata.Name));
        Assert.Empty(selectionChangesDuringRestore);
        Assert.Equal(0, Volatile.Read(ref sourceChangesDuringRestore));
        viewModel.TableSource.Changed -= sourceChanged;
    }

    private static async Task<DynamicTableView> WaitForDockTableAsync(Window window, DynamicTableView previousTable)
        => await TestWait.UntilValueAsync(
            () => window.GetVisualDescendants().OfType<DynamicTableView>()
                .FirstOrDefault(table => !ReferenceEquals(table, previousTable) &&
                    table.GetVisualDescendants().OfType<TableViewRow>().Count() == 10),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs())
            ?? throw new TimeoutException("A new docked resource list table did not attach.");

    private static void SelectTenRows(Window window, DynamicTableView table)
    {
        var rows = table.GetVisualDescendants().OfType<TableViewRow>()
            .OrderBy(static row => ((V1Pod)row.DataContext!).Metadata.Name)
            .ToArray();
        var first = RowCenter(window, rows[0]);
        var last = RowCenter(window, rows[9]);
        window.MouseDown(first, MouseButton.Left);
        window.MouseMove(last, RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();
        window.MouseUp(last, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static void SelectOneRow(Window window, DynamicTableView table, string podName)
    {
        var position = RowCenter(window, FindRow(table, podName));
        window.MouseDown(position, MouseButton.Left);
        window.MouseUp(position, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static void SelectRowsByName(Window window, DynamicTableView table, string firstName, string lastName)
    {
        window.MouseDown(RowCenter(window, FindRow(table, firstName)), MouseButton.Left);
        window.MouseMove(RowCenter(window, FindRow(table, lastName)), RawInputModifiers.LeftMouseButton);
        Dispatcher.UIThread.RunJobs();
        window.MouseUp(RowCenter(window, FindRow(table, lastName)), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static TableViewRow FindRow(DynamicTableView table, string name)
        => table.GetVisualDescendants().OfType<TableViewRow>()
            .Single(row => ((V1Pod)row.DataContext!).Metadata.Name == name);

    private static Point RowCenter(Window window, TableViewRow row)
        => row.TranslatePoint(new Point(row.Bounds.Width / 2, row.Bounds.Height / 2), window)
            ?? throw new InvalidOperationException("Resource row has no window position.");

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
