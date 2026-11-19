using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Controls.Layout;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.Workloads.v1.Pod.Services;
using KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using SvcSystems.Avalonia.DynamicTableView;

namespace KubeUI.Features.Resources.Tests.Workloads.v1.Pod;

public sealed class ContainerFileBrowserViewTests
{
    [AvaloniaFact]
    public async Task Loading_the_root_expands_the_tree_navigation()
    {
        var access = new FakeContainerFileAccess(new Dictionary<string, ContainerDirectoryListing>(StringComparer.Ordinal)
        {
            ["/"] = Listing(
                Directory("/app"),
                Directory("/etc"),
                Directory("/var"),
                File("/config.yaml", 812),
                File("/notes.txt", 2048),
                Link("/current")),
        });
        using var vm = CreateViewModel(access);
        await vm.InitializeAsync();

        var view = new ContainerFileBrowserView { DataContext = vm };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await WaitForAsync(() => FindItem(view, "/etc") is not null);

        // Documentation capture; no-op unless KUBEUI_TEST_SCREENSHOTS is set.
        TestScreenshot.CaptureIfEnabled(window, "ContainerFileBrowser");

        FindItem(view, "/")!.IsExpanded.ShouldBeTrue();
        FindItem(view, "/etc").ShouldNotBeNull();
        FindTreeIcon(view, "/")!.Icon.ShouldBe(Icon.FolderOpen);
        FindTreeIcon(view, "/etc")!.Icon.ShouldBe(Icon.Folder);

        vm.RootFolders[0].IsExpanded = false;
        await WaitForAsync(() => !FindItem(view, "/")!.IsExpanded);
        FindTreeIcon(view, "/")!.Icon.ShouldBe(Icon.Folder);
    }

    [AvaloniaFact]
    public async Task Entry_cells_show_matching_folder_file_and_link_icons()
    {
        var access = new FakeContainerFileAccess(new Dictionary<string, ContainerDirectoryListing>(StringComparer.Ordinal)
        {
            ["/"] = Listing(Directory("/etc"), File("/notes.txt", 4), Link("/shortcut")),
        });
        using var vm = CreateViewModel(access);
        await vm.InitializeAsync();
        var template = vm.TableSource.Columns.Single(column => column.Key == "name").CellTemplate;
        template.ShouldNotBeNull();

        var cases = new[]
        {
            (Entry: vm.Entries.Single(entry => entry.Name == "etc"), Icon: Icon.Folder),
            (Entry: vm.Entries.Single(entry => entry.Name == "notes.txt"), Icon: Icon.Document),
            (Entry: vm.Entries.Single(entry => entry.Name == "shortcut"), Icon: Icon.Link),
        };
        foreach (var (entry, expected) in cases)
        {
            var cell = template!.Build(entry)
                ?? throw new InvalidOperationException("The name cell template did not build.");
            cell.DataContext = entry;
            using var window = Application.Current.CreateTestWindow(content: cell);
            window.Show();
            await WaitForAsync(() => VisibleEntryIcon(cell) is not null);
            VisibleEntryIcon(cell)!.Icon.ShouldBe(expected);
        }
    }

    [AvaloniaFact]
    public async Task Opening_a_folder_from_the_table_expands_its_tree_node()
    {
        var access = new FakeContainerFileAccess(new Dictionary<string, ContainerDirectoryListing>(StringComparer.Ordinal)
        {
            ["/"] = Listing(Directory("/etc")),
            ["/etc"] = Listing(Directory("/etc/nginx")),
        });
        using var vm = CreateViewModel(access);
        await vm.InitializeAsync();

        var view = new ContainerFileBrowserView { DataContext = vm };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await WaitForAsync(() => FindItem(view, "/etc") is not null);

        await vm.OpenFolderCommand.ExecuteAsync(vm.Entries.Single(entry => entry.IsDirectory));
        await WaitForRenderAsync(window, () => FindItem(view, "/etc/nginx") is not null);

        FindItem(view, "/etc")!.IsExpanded.ShouldBeTrue();
        FindItem(view, "/etc/nginx").ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task Table_navigation_selects_the_folder_in_the_tree()
    {
        var access = new FakeContainerFileAccess(new Dictionary<string, ContainerDirectoryListing>(StringComparer.Ordinal)
        {
            ["/"] = Listing(Directory("/etc")),
            ["/etc"] = Listing(Directory("/etc/nginx")),
        });
        using var vm = CreateViewModel(access);
        await vm.InitializeAsync();

        var view = new ContainerFileBrowserView { DataContext = vm };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await WaitForAsync(() => FindItem(view, "/etc") is not null);

        await vm.OpenFolderCommand.ExecuteAsync(vm.Entries.Single(entry => entry.IsDirectory));
        await WaitForRenderAsync(window, () => FindItem(view, "/etc/nginx") is not null);

        var tree = view.GetVisualDescendants().OfType<TreeView>().Single();
        tree.SelectedItem.ShouldBeSameAs(vm.SelectedFolder);

        // The root stays reachable from the tree after the table moved away from it.
        tree.SelectedItem = vm.RootFolders[0];
        await WaitForAsync(() => vm.SelectedFolder!.Path == "/");
        vm.Entries.ShouldContain(static entry => entry.Name == "etc");
    }

    [AvaloniaFact]
    public async Task Header_uses_the_compact_toolbar_with_identity_and_trailing_refresh()
    {
        var access = new FakeContainerFileAccess(new Dictionary<string, ContainerDirectoryListing>(StringComparer.Ordinal)
        {
            ["/"] = Listing(Directory("/etc")),
        });
        using var vm = CreateViewModel(access);
        await vm.InitializeAsync();

        var view = new ContainerFileBrowserView { DataContext = vm };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await WaitForAsync(() => view.GetVisualDescendants().OfType<CompactToolbar>().Any());

        var toolbar = view.GetVisualDescendants().OfType<CompactToolbar>().Single();
        var leading = toolbar.Leading.ShouldBeOfType<StackPanel>();
        leading.Children.OfType<TextBlock>().Single().Text.ShouldBe("default/api/app");

        var trailing = toolbar.Trailing.ShouldBeOfType<StackPanel>();
        var refresh = trailing.Children.OfType<Button>().Single();
        refresh.Command.ShouldBeSameAs(vm.RefreshCommand);
        refresh.BringIntoView();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        var right = refresh.TranslatePoint(new Point(refresh.Bounds.Width, 0), toolbar);
        right.ShouldNotBeNull();
        right.Value.X.ShouldBeGreaterThan(toolbar.Bounds.Width - 8);

        var top = toolbar.TranslatePoint(new Point(0, 0), view);
        top.ShouldNotBeNull();
        top.Value.Y.ShouldBeLessThanOrEqualTo(4);
    }

    private static TreeViewItem? FindItem(Control view, string path)
        => view.GetVisualDescendants()
            .OfType<TreeViewItem>()
            .FirstOrDefault(item => item.DataContext is ContainerFolderNode node
                && string.Equals(node.Path, path, StringComparison.Ordinal));

    private static FluentIcon? FindTreeIcon(Control view, string path)
        => view.GetVisualDescendants()
            .OfType<FluentIcon>()
            .FirstOrDefault(icon => icon.DataContext is ContainerFolderNode node
                && string.Equals(node.Path, path, StringComparison.Ordinal));

    private static FluentIcon? VisibleEntryIcon(Control cell)
        => cell.GetVisualDescendants()
            .OfType<FluentIcon>()
            .FirstOrDefault(icon => icon.IsVisible);

    private static ContainerFileBrowserViewModel CreateViewModel(IContainerFileAccess access)
        => new(
            Mock.Of<IPlatformServices>(),
            new FakeContainerFileAccessFactory(access),
            new NoOpPreviewLauncher(),
            Mock.Of<ILogger<ContainerFileBrowserViewModel>>())
        {
            Cluster = null!,
            Object = new V1Pod
            {
                Metadata = new V1ObjectMeta { Name = "api", NamespaceProperty = "default" },
            },
            ContainerName = "app",
        };

    private static ContainerDirectoryListing Listing(params ContainerFileEntry[] entries)
        => new(entries, Truncated: false);

    private static ContainerFileEntry Directory(string path)
        => new(path[(path.LastIndexOf('/') + 1)..], path, IsDirectory: true, IsSymbolicLink: false, Size: 4096);

    private static ContainerFileEntry File(string path, long size)
        => new(path[(path.LastIndexOf('/') + 1)..], path, IsDirectory: false, IsSymbolicLink: false, Size: size);

    private static ContainerFileEntry Link(string path)
        => new(path[(path.LastIndexOf('/') + 1)..], path, IsDirectory: false, IsSymbolicLink: true, Size: 0);


    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 3000)
        => await TestWait.UntilAsync(
            predicate,
            timeoutMs,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());

    private static async Task WaitForRenderAsync(TopLevel window, Func<bool> predicate, int timeoutMs = 3000)
        => await TestWait.UntilAsync(
            predicate,
            timeoutMs,
            TestContext.Current.CancellationToken,
            () =>
            {
                Dispatcher.UIThread.RunJobs();
                using var frame = window.CaptureRenderedFrame();
            });

    private sealed class FakeContainerFileAccessFactory(IContainerFileAccess access) : IContainerFileAccessFactory
    {
        public IContainerFileAccess Create(ClusterWorkspace cluster, string podName, string @namespace, string containerName)
            => access;
    }

    private sealed class FakeContainerFileAccess(IReadOnlyDictionary<string, ContainerDirectoryListing> listings)
        : IContainerFileAccess
    {
        public Task<ContainerFileCapabilities> ProbeAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new ContainerFileCapabilities(CanList: true, CanRead: true, Problem: null));

        public Task<ContainerDirectoryListing> ListAsync(string path, CancellationToken cancellationToken = default)
            => Task.FromResult(listings.TryGetValue(path, out var listing)
                ? listing
                : new ContainerDirectoryListing([], Truncated: false));

        public Task<ContainerFileContent> ReadAsync(string path, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class NoOpPreviewLauncher : IContainerFilePreviewLauncher
    {
        public Task ShowAsync(ClusterWorkspace cluster, V1Pod pod, string containerName, ContainerFileEntry entry)
            => Task.CompletedTask;
    }
}
