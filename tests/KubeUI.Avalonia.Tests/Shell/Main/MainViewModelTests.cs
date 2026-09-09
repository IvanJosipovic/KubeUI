using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using KubeUI.Avalonia.Shell.Documents.CloudClusters.Aks;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Main;

public sealed class MainViewModelTests
{
    private MainViewModel CreateViewModel()
    {
        return Application.Current.GetTestServices().GetRequiredService<MainViewModel>()
            ?? throw new InvalidOperationException("Test services are not initialized.");
    }

    [AvaloniaFact]
    public async Task load_aks_clusters_command_opens_docked_aks_assistant()
    {
        var vm = CreateViewModel();
        var documents = Application.Current.GetRequiredTestService<IFactory>().GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();

        vm.ImportAksClusterCommand.Execute(null);
        await TestApplicationExtensions.WaitForUiAsync();

        documents.VisibleDockables!
            .OfType<ImportAksClusterViewModel>()
            .Count()
            .ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task floating_home_can_be_docked_as_document()
    {
        IFactory factory = Application.Current.GetRequiredTestService<IFactory>();
        IDocumentDock documents = factory.GetDockable<IDocumentDock>("Documents")
            .ShouldNotBeNull();
        HomeViewModel home = factory.FindDockableById(nameof(HomeViewModel))
            .ShouldBeOfType<HomeViewModel>();

        factory.FloatDockable(home);
        await TestApplicationExtensions.WaitForUiAsync();
        home.Owner.ShouldBeAssignableTo<IDocumentDock>();
        factory.GetDockable<IRootDock>("Root")!
            .Windows!
            .Single()
            .Host
            .ShouldBeOfType<HostWindow>()
            .Icon
            .ShouldNotBeNull();
        factory.GetDockable<IRootDock>("Root")!
            .Windows!
            .Single()
            .Title
            .ShouldBe("KubeUI 2");
        factory.DockAsDocument(home);

        home.Owner.ShouldBeSameAs(documents);
    }

    [AvaloniaFact]
    public async Task floating_tool_from_tool_dock_uses_document_window()
    {
        IFactory factory = Application.Current.GetRequiredTestService<IFactory>();
        IToolDock bottomDock = factory.GetDockable<IToolDock>("BottomDock")
            .ShouldNotBeNull();
        Tool tool = new()
        {
            Id = "FloatingTool",
            Title = "Floating tool",
            CanClose = true,
            CanDockAsDocument = true,
            CanFloat = true
        };

        factory.AddDockable(bottomDock, tool);
        factory.FloatDockable(tool);
        await TestApplicationExtensions.WaitForUiAsync();

        tool.Owner.ShouldBeAssignableTo<IDocumentDock>();
        tool.Owner.ShouldNotBeSameAs(bottomDock);
        factory.FindRoot(tool)!.Window!.Host!.ShouldBeOfType<HostWindow>().IsToolWindow.ShouldBeFalse();

        factory.DockAsDocument(tool);
    }

    [AvaloniaFact]
    public void reset_publishes_only_initialized_layouts()
    {
        MainViewModel vm = CreateViewModel();
        IFactory factory = Application.Current.GetRequiredTestService<IFactory>();
        IRootDock? publishedLayout = null;

        vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.Layout))
            {
                publishedLayout = vm.Layout;
                publishedLayout?.Factory.ShouldBeSameAs(factory);
            }
        };

        vm.ResetLayoutCommand.Execute(null);

        publishedLayout.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public void close_layout_clears_published_layout()
    {
        MainViewModel vm = CreateViewModel();
        IFactory factory = Application.Current.GetRequiredTestService<IFactory>();
        IRootDock layout = factory.CreateLayout();
        factory.InitLayout(layout);
        vm.Layout = layout;

        vm.CloseLayoutCommand.Execute(null);

        vm.Layout.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task main_view_attaches_the_injected_factory_to_dock_control()
    {
        MainViewModel vm = CreateViewModel();
        IFactory factory = Application.Current.GetRequiredTestService<IFactory>();
        MainView view = new() { DataContext = vm };
        using TestApplicationExtensions.TestWindow window = Application.Current.CreateTestWindow(content: view);

        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();

        view.GetVisualDescendants()
            .OfType<DockControl>()
            .Single()
            .Factory
            .ShouldBeSameAs(factory);
    }

}
