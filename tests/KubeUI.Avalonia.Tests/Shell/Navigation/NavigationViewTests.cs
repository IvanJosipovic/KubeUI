using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Dock.Model.Controls;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.List;
using KubeUI.Avalonia.Shell.Documents.About;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Avalonia.Shell.Navigation;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Navigation;

public sealed class NavigationViewTests
{
    [Fact]
    public void navigation_item_name_comparer_handles_nulls_and_names()
    {
        var comparer = new NavigationItemNameComparer();
        var first = new NavigationItem { Name = "a" };
        var second = new NavigationItem { Name = "b" };

        comparer.Compare(null, second).ShouldBe(0);
        comparer.Compare(first, null).ShouldBeGreaterThan(0);
        comparer.Compare(first, second).ShouldBeLessThan(0);
    }

    [Fact]
    public void navigation_item_order_comparer_uses_reference_order_name_and_id()
    {
        var comparer = new NavigationItemOrderComparer();
        var first = new NavigationItem { Id = "a", Name = "same", Order = 1 };
        var second = new NavigationItem { Id = "b", Name = "same", Order = 1 };
        var later = new NavigationItem { Id = "c", Name = "later", Order = 2 };

        comparer.Compare(first, first).ShouldBe(0);
        comparer.Compare(null, first).ShouldBeLessThan(0);
        comparer.Compare(first, null).ShouldBeGreaterThan(0);
        comparer.Compare(first, later).ShouldBeLessThan(0);
        comparer.Compare(first, second).ShouldBeLessThan(0);
        comparer.Compare(second, first).ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public void cluster_navigation_name_update_with_same_name_preserves_child_ids()
    {
        var workspace = Application.Current.GetTestServices()
            .GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        using var clusterNode = new ClusterNavigationNode(workspace);
        var child = new NavigationItem
        {
            Id = $"{workspace.Runtime.Name}-child",
            Name = "Child"
        };
        clusterNode.NavigationItems.Add(child);

        clusterNode.UpdateNavigationName(workspace.Runtime.Name);

        child.Id.ShouldBe($"{workspace.Runtime.Name}-child");
    }

    [AvaloniaFact]
    public void navigation_converters_handle_valid_and_invalid_values()
    {
        var culture = CultureInfo.InvariantCulture;
        var geometry = new StringToGeometryConverter();
        var svg = new StringToSvgImageConverter();

        geometry.Convert("M 0,0 L 1,1", typeof(object), null, culture).ShouldNotBeNull();
        geometry.Convert(string.Empty, typeof(object), null, culture).ShouldBeNull();
        geometry.Convert(42, typeof(object), null, culture).ShouldBeNull();
        geometry.ConvertBack(null, typeof(object), null, culture).ShouldBe(BindingOperations.DoNothing);

        svg.Convert("/Assets/kube/blank.svg", typeof(object), null, culture).ShouldNotBeNull();
        svg.Convert(" ", typeof(object), null, culture).ShouldBeNull();
        svg.Convert(42, typeof(object), null, culture).ShouldBeNull();
        svg.ConvertBack(null, typeof(object), null, culture).ShouldBe(BindingOperations.DoNothing);
    }

    [AvaloniaFact]
    public async Task resource_count_assigned_after_template_creation_is_rendered()
    {
        var services = Application.Current.GetTestServices();
        var config = services.GetRequiredService<TestClusterConfig>();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        using var navigation = services.GetRequiredService<NavigationViewModel>();
        var clusterNode = new ClusterNavigationNode(workspace) { IsExpanded = true };
        var podsLink = new ResourceNavigationLink
        {
            Cluster = workspace,
            Id = "test-pods",
            Name = "Pods",
            ResourceKind = GroupApiVersionKind.From<V1Pod>(),
        };
        clusterNode.NavigationItems.Add(podsLink);
        navigation.Clusters.Add(clusterNode);

        var view = new NavigationView { DataContext = navigation };
        var window = new Window { Content = view };
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();
        await TestApplicationExtensions.WaitForUiAsync();

        podsLink.Count = Observable.Return(3);
        await TestApplicationExtensions.WaitForUiAsync();
        await TestApplicationExtensions.WaitForUiAsync();

        view.GetVisualDescendants().OfType<TextBlock>().Select(text => text.Text).ShouldContain("3");

        window.Content = null;
        window.Close();
    }

    [AvaloniaFact]
    public async Task clicking_selected_navigation_section_collapses_it()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        using var navigation = services.GetRequiredService<NavigationViewModel>();
        var clusterNode = navigation.Clusters.Single(node => node.Cluster == workspace);
        var section = new NavigationItem { Id = "network", Name = "Network" };
        section.NavigationItems.Add(new NavigationItem { Id = "services", Name = "Services" });
        clusterNode.NavigationItems.Add(section);
        clusterNode.IsExpanded = true;

        var view = new NavigationView { DataContext = navigation };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();

        var treeItem = await FindTreeItemAsync(view, section);
        var tree = view.GetVisualDescendants().OfType<TreeView>().Single();
        var sectionSelections = 0;
        tree.SelectionChanged += (_, args) =>
        {
            if (args.AddedItems.Contains(section))
            {
                sectionSelections++;
            }
        };

        ClickNavigationItem(window, treeItem, section.Name);
        sectionSelections.ShouldBe(1);
        section.IsExpanded.ShouldBeTrue();
        treeItem.IsExpanded.ShouldBeTrue();
        ReferenceEquals(tree.SelectedItem, section).ShouldBeTrue();

        ClickNavigationItem(window, treeItem, section.Name);
        sectionSelections.ShouldBe(2);
        section.IsExpanded.ShouldBeFalse();
        treeItem.IsExpanded.ShouldBeFalse();

        var expander = treeItem.GetVisualDescendants().OfType<ToggleButton>()
            .First(toggle => ReferenceEquals(toggle.DataContext, section));
        ClickControl(window, expander);
        ReferenceEquals(tree.SelectedItem, section).ShouldBeTrue();
        section.IsExpanded.ShouldBeTrue();
        treeItem.IsExpanded.ShouldBeTrue();

        ClickControl(window, expander);
        ReferenceEquals(tree.SelectedItem, section).ShouldBeTrue();
        section.IsExpanded.ShouldBeFalse();
        treeItem.IsExpanded.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task clicking_selected_navigation_section_after_double_tap_window_collapses_it()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        using var navigation = services.GetRequiredService<NavigationViewModel>();
        var clusterNode = navigation.Clusters.Single(node => node.Cluster == workspace);
        var section = new NavigationItem { Id = "network", Name = "Network" };
        section.NavigationItems.Add(new NavigationItem { Id = "services", Name = "Services" });
        clusterNode.NavigationItems.Add(section);
        clusterNode.IsExpanded = true;

        var view = new NavigationView { DataContext = navigation };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();

        var treeItem = await FindTreeItemAsync(view, section);
        ClickNavigationItem(window, treeItem, section.Name);
        section.IsExpanded.ShouldBeTrue();

        var platformSettings = Application.Current?.PlatformSettings
            ?? throw new InvalidOperationException("Avalonia platform settings are unavailable.");
        var doubleTapTime = platformSettings.GetDoubleTapTime(PointerType.Mouse);
        var clickTimestamp = Stopwatch.GetTimestamp();
        await TestWait.UntilAsync(
            () => Stopwatch.GetElapsedTime(clickTimestamp) > doubleTapTime,
            doubleTapTime + TimeSpan.FromSeconds(2),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        ClickNavigationItem(window, treeItem, section.Name);

        section.IsExpanded.ShouldBeFalse();
        treeItem.IsExpanded.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task clicking_connected_cluster_toggles_expansion_without_native_double_tap()
    {
        var services = Application.Current.GetTestServices();
        using var workspace = await Application.Current.CreateClusterAsync();
        using var navigation = services.GetRequiredService<NavigationViewModel>();
        await TestWait.UntilAsync(
            () => navigation.Clusters.Any(node => ReferenceEquals(node.Cluster, workspace)),
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());

        var clusterNode = navigation.Clusters.Single(node => ReferenceEquals(node.Cluster, workspace));
        clusterNode.IsExpanded.ShouldBeFalse();

        var view = new NavigationView { DataContext = navigation };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();

        var treeItem = await FindTreeItemAsync(view, clusterNode);
        ClickNavigationItem(window, treeItem, workspace.Runtime.Name);
        await TestWait.UntilAsync(
            () => clusterNode.IsExpanded,
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());

        ClickNavigationItem(window, treeItem, workspace.Runtime.Name);
        await TestWait.UntilAsync(
            () => !clusterNode.IsExpanded,
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());
    }

    [AvaloniaFact]
    public async Task clicking_disconnected_cluster_row_connects_and_expands_it()
    {
        var services = Application.Current.GetTestServices();
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var catalog = services.GetRequiredService<ClusterWorkspaceCatalog>();
        catalog.Clusters.Add(workspace);
        using var navigation = services.GetRequiredService<NavigationViewModel>();
        await TestWait.UntilAsync(
            () => navigation.Clusters.Any(node => ReferenceEquals(node.Cluster, workspace)),
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());

        var clusterNode = navigation.Clusters.Single(node => ReferenceEquals(node.Cluster, workspace));
        clusterNode.IsExpanded.ShouldBeFalse();
        var view = new NavigationView { DataContext = navigation };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();

        var treeItem = await FindTreeItemAsync(view, clusterNode);
        ClickNavigationItem(window, treeItem, workspace.Runtime.Name);

        await TestWait.UntilAsync(
            () => workspace.Runtime.Connected && clusterNode.IsExpanded,
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());
    }

    [AvaloniaFact]
    public async Task keyboard_arrows_expand_and_collapse_focused_navigation_section()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        using var navigation = services.GetRequiredService<NavigationViewModel>();
        var clusterNode = navigation.Clusters.Single(node => node.Cluster == workspace);
        var section = new NavigationItem { Id = "network", Name = "Network" };
        var child = new NavigationItem { Id = "services", Name = "Services" };
        section.NavigationItems.Add(child);
        clusterNode.NavigationItems.Add(section);
        clusterNode.IsExpanded = true;

        var view = new NavigationView { DataContext = navigation };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();

        var sectionTreeItem = await FindTreeItemAsync(view, section);
        ClickNavigationItem(window, sectionTreeItem, section.Name);
        section.IsExpanded.ShouldBeTrue();

        sectionTreeItem.Focus();
        window.KeyPress(Key.Left, RawInputModifiers.None, PhysicalKey.ArrowLeft, null);
        Dispatcher.UIThread.RunJobs();
        section.IsExpanded.ShouldBeFalse();
        sectionTreeItem.IsExpanded.ShouldBeFalse();

        window.KeyPress(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, null);
        Dispatcher.UIThread.RunJobs();
        section.IsExpanded.ShouldBeTrue();
        sectionTreeItem.IsExpanded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task selection_behavior_ignores_non_row_presses_and_detaches_cleanly()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        using var navigation = services.GetRequiredService<NavigationViewModel>();
        var clusterNode = navigation.Clusters.Single(node => node.Cluster == workspace);
        var section = new NavigationItem { Id = "network", Name = "Network" };
        section.NavigationItems.Add(new NavigationItem { Id = "services", Name = "Services" });
        clusterNode.NavigationItems.Add(section);
        clusterNode.IsExpanded = true;

        var view = new NavigationView { DataContext = navigation };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();

        var treeItem = await FindTreeItemAsync(view, section);
        var tree = view.GetVisualDescendants().OfType<TreeView>().Single();
        ClickNavigationItem(window, treeItem, section.Name);
        section.IsExpanded.ShouldBeTrue();
        ReferenceEquals(tree.SelectedItem, section).ShouldBeTrue();

        var label = treeItem.GetVisualDescendants().OfType<TextBlock>()
            .First(textBlock => textBlock.Text == section.Name);
        ClickControl(window, label, MouseButton.Right);
        ReferenceEquals(tree.SelectedItem, section).ShouldBeTrue();
        section.IsExpanded.ShouldBeTrue();

        var emptyPosition = tree.TranslatePoint(
            new Point(tree.Bounds.Width - 2, tree.Bounds.Height - 2),
            window) ?? throw new InvalidOperationException("Navigation tree has no window position.");
        window.MouseDown(emptyPosition, MouseButton.Left);
        window.MouseUp(emptyPosition, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        ReferenceEquals(tree.SelectedItem, section).ShouldBeTrue();
        section.IsExpanded.ShouldBeTrue();

        var behavior = Interaction.GetBehaviors(tree).OfType<NavigationTreeSelectionBehavior>().Single();
        Interaction.GetBehaviors(tree).Remove(behavior);

        ClickNavigationItem(window, treeItem, section.Name);

        section.IsExpanded.ShouldBeTrue();
        ReferenceEquals(tree.SelectedItem, section).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task right_click_opens_the_context_menu_for_the_navigation_row()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        using var navigation = services.GetRequiredService<NavigationViewModel>();
        var clusterNode = navigation.Clusters.Single(node => node.Cluster == workspace);
        var podsLink = new ResourceNavigationLink
        {
            Id = "test-pods",
            Name = "Pods",
            Cluster = workspace,
            ResourceKind = GroupApiVersionKind.From<V1Pod>(),
            OpenCommand = navigation.OpenResourceNavigationCommand,
            OpenInNewTabCommand = navigation.OpenResourceNavigationInNewTabCommand,
        };
        clusterNode.NavigationItems.Add(podsLink);
        clusterNode.IsExpanded = true;

        var view = new NavigationView { DataContext = navigation };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();

        var clusterTreeItem = await FindTreeItemAsync(view, clusterNode);
        var tree = view.GetVisualDescendants().OfType<TreeView>().Single();
        ClickNavigationItem(window, clusterTreeItem, workspace.Runtime.Name, MouseButton.Right);

        var clusterMenu = FindContextMenu(clusterTreeItem, clusterNode);
        clusterMenu.IsOpen.ShouldBeTrue();
        clusterMenu.Items.OfType<MenuItem>()
            .Select(menuItem => menuItem.Header?.ToString())
            .ShouldBe([
                Assets.Resources.NavigationView_ContextMenu_Connect,
                Assets.Resources.NavigationView_ContextMenu_Settings
            ]);
        tree.SelectedItem.ShouldBeNull();
        clusterMenu.Close();
        clusterMenu.IsOpen.ShouldBeFalse();

        var podsTreeItem = await FindTreeItemAsync(view, podsLink);
        ClickNavigationItem(window, podsTreeItem, podsLink.Name, MouseButton.Right);

        var podsMenu = FindContextMenu(podsTreeItem, podsLink);
        clusterMenu.IsOpen.ShouldBeFalse();
        podsMenu.IsOpen.ShouldBeTrue();
        podsMenu.Items.OfType<MenuItem>()
            .Select(menuItem => menuItem.Header?.ToString())
            .ShouldBe([
                Assets.Resources.NavigationView_ContextMenu_Open,
                Assets.Resources.NavigationView_ContextMenu_OpenNewTab
            ]);
        tree.SelectedItem.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task clicking_selected_resource_navigation_link_reactivates_its_document()
    {
        var services = Application.Current.GetTestServices();
        using var workspace = await Application.Current.CreateClusterAsync();
        using var navigation = services.GetRequiredService<NavigationViewModel>();
        var main = services.GetRequiredService<MainViewModel>();
        main.Initialize();
        await TestWait.UntilAsync(
            () => navigation.Clusters.Any(node => ReferenceEquals(node.Cluster, workspace)),
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());

        var clusterNode = navigation.Clusters.Single(node => ReferenceEquals(node.Cluster, workspace));
        clusterNode.IsExpanded = true;
        var podsLink = new ResourceNavigationLink
        {
            Id = "test-pods",
            Name = "Pods",
            Cluster = workspace,
            ResourceKind = GroupApiVersionKind.From<V1Pod>(),
            OpenCommand = navigation.OpenResourceNavigationCommand,
            OpenInNewTabCommand = navigation.OpenResourceNavigationInNewTabCommand,
        };
        clusterNode.NavigationItems.Add(podsLink);

        var navigationView = new NavigationView { DataContext = navigation };
        var mainView = new MainView { DataContext = main };
        var shell = new Grid { RowDefinitions = new RowDefinitions("220,*") };
        Grid.SetRow(navigationView, 0);
        Grid.SetRow(mainView, 1);
        shell.Children.Add(navigationView);
        shell.Children.Add(mainView);
        using var window = Application.Current.CreateTestWindow(content: shell);
        window.Show();

        var treeItem = await FindTreeItemAsync(navigationView, podsLink);
        ClickNavigationItem(window, treeItem, podsLink.Name);

        var documents = navigation.Factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();
        await TestWait.UntilAsync(
            () => documents.VisibleDockables?.OfType<IResourceListViewModel>().Any(document =>
                ReferenceEquals(document.Cluster, workspace) && document.Kind == GroupApiVersionKind.From<V1Pod>()) == true,
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());
        var podsDocument = documents.VisibleDockables!.OfType<IResourceListViewModel>()
            .Single(document => ReferenceEquals(document.Cluster, workspace)
                && document.Kind == GroupApiVersionKind.From<V1Pod>());

        var otherDocument = services.GetRequiredService<AboutViewModel>();
        otherDocument.Id = $"navigation-test-{Guid.NewGuid():N}";
        otherDocument.Title = "Other navigation test tab";
        navigation.Factory.AddToDocuments(otherDocument);
        navigation.Factory.SetActiveDockable(otherDocument);
        navigation.Factory.SetFocusedDockable(documents, otherDocument);
        ReferenceEquals(documents.ActiveDockable, otherDocument).ShouldBeTrue();

        var otherTabHeader = await TestWait.UntilValueAsync(
            () => mainView.GetVisualDescendants().OfType<TextBlock>()
                .FirstOrDefault(textBlock => textBlock.Text == otherDocument.Title),
            5000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs())
            ?? throw new TimeoutException("The other document tab did not attach.");
        ClickControl(window, otherTabHeader);
        await TestWait.UntilAsync(
            () => ReferenceEquals(documents.ActiveDockable, otherDocument),
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());

        ClickNavigationItem(window, treeItem, podsLink.Name);

        await TestWait.UntilAsync(
            () => ReferenceEquals(documents.ActiveDockable, podsDocument),
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());
    }

    private static async Task<TreeViewItem> FindTreeItemAsync(Control view, NavigationItem item)
    {
        return await TestWait.UntilValueAsync(
            () => view.GetVisualDescendants().OfType<TreeViewItem>()
                .FirstOrDefault(treeItem => ReferenceEquals(treeItem.DataContext, item)),
            5000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs())
            ?? throw new TimeoutException($"Navigation item '{item.Name}' did not attach.");
    }

    private static ContextMenu FindContextMenu(TreeViewItem treeItem, object dataContext)
    {
        return treeItem.GetVisualDescendants().OfType<Control>()
            .First(control => ReferenceEquals(control.DataContext, dataContext) && control.ContextMenu != null)
            .ContextMenu!;
    }

    private static void ClickNavigationItem(
        Window window,
        TreeViewItem treeItem,
        string name,
        MouseButton button = MouseButton.Left)
    {
        var label = treeItem.GetVisualDescendants().OfType<TextBlock>()
            .First(textBlock => textBlock.Text == name);
        ClickControl(window, label, button);
    }

    private static void ClickControl(Window window, Control control, MouseButton button = MouseButton.Left)
    {
        var position = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window)
            ?? throw new InvalidOperationException("Navigation control has no window position.");
        window.MouseDown(position, button);
        window.MouseUp(position, button);
        Dispatcher.UIThread.RunJobs();
    }
}
