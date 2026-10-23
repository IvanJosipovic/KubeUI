using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using HanumanInstitute.MvvmDialogs;
using Helm.NET;
using k8s.Models;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Features.Helm.Charts;
using KubeUI.Avalonia.Features.Helm.Releases;
using KubeUI.Avalonia.Features.Helm.Repositories;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using SvcSystems.Avalonia.DynamicTableView;
using Ursa.Controls;

namespace KubeUI.Avalonia.Tests.Features.Helm;

public sealed class HelmChartsViewModelTests
{
    [AvaloniaFact]
    public async Task Charts_filter_by_repository_and_context_install_opens_prefilled_editor()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new("zeta", "https://zeta.test"), new("second", "https://second.test"),
                new("Alpha", "https://alpha.test"), new("first", "https://first.test"),
                new("beta", "https://beta.test")]);
        helm.Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), true, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new("first/web", "web", "2.0"), new("first/web", "web", "1.0"),
                new("second/web", "web", "3.0"), new("second/web-tools", "web-tools", "1.0")]);
        var factory = new Mock<IFactory>();
        var documents = new DocumentDock { VisibleDockables = [] };
        factory.Setup(item => item.GetDockable<IDocumentDock>("Documents")).Returns(documents);
        HelmReleaseEditorViewModel? opened = null;
        factory.Setup(item => item.InsertDockable(documents, It.IsAny<IDockable>(), 0))
            .Callback<IDock, IDockable, int>((_, dockable, _) => opened = dockable.ShouldBeOfType<HelmReleaseEditorViewModel>());
        using var charts = new HelmChartsViewModel(helm.Object, CreateOpener(helm, factory));
        charts.SelectedRepository = "first";
        workspace.SelectedNamespaces.Clear();
        workspace.SelectedNamespaces.Add(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "apps" }
        });
        using var services = new ServiceCollection().AddSingleton(charts).BuildServiceProvider();
        var navigation = new HelmNavigationSectionProvider().Documents
            .Single(document => document.Id == HelmNavigationSectionProvider.ChartsDocumentId);
        navigation.CreateDockable(services, workspace).ShouldBeSameAs(charts);
        await charts.RefreshCommand.ExecutionTask!.WaitAsync(TestContext.Current.CancellationToken);
        charts.Repositories.ShouldBe([Assets.Resources.HelmChartsView_AllRepositories, "Alpha", "beta", "first", "second", "zeta"]);
        charts.SelectedRepository.ShouldBe("first");
        charts.Status.ShouldBeEmpty();
        charts.SearchText = " web ";
        await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        charts.ItemCount.ShouldBe(1);

        var view = new HelmChartsView { DataContext = charts };
        using var window = Application.Current.CreateTestWindow(content: view);
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            var controls = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view).ToArray();
            var header = controls.OfType<Grid>().First(grid => grid.ColumnDefinitions.Count == 4);
            header.MinHeight.ShouldBe(32);
            header.Margin.ShouldBe(new Thickness(0, 2));
            var repository = controls.OfType<ComboBox>().Single();
            repository.SelectedItem = "second";
            charts.ItemCount.ShouldBe(2);
            var table = controls.OfType<DynamicTableView>().Single();
            var menu = table.ContextMenuItemsFactory!([new HelmChartSearchResult("second/web", "web", "3.0")])
                .Cast<MenuItem>().Single();
            menu.Header.ShouldBe(Assets.Resources.HelmChartsView_Install);
            menu.Command.ShouldBeSameAs(charts.InstallChartCommand);
            await charts.InstallChartCommand.ExecuteAsync(menu.CommandParameter).WaitAsync(TestContext.Current.CancellationToken);
            opened.ShouldNotBeNull();
            opened.IsUpgrade.ShouldBeFalse();
            opened.Namespace.ShouldBe("apps");
            opened.Chart.ShouldBe("second/web");
            opened.SelectedRepository.ShouldBe("second");
            opened.SelectedChart.ShouldBe("web");
            opened.ReleaseName.ShouldBeEmpty();
            opened.ValuesYaml.ShouldBe("{}");
            charts.Status.ShouldBeEmpty();
            factory.Setup(item => item.InsertDockable(documents, It.IsAny<IDockable>(), 0))
                .Throws(new InvalidOperationException("Cannot open document"));
            await charts.InstallChartCommand.ExecuteAsync(menu.CommandParameter).WaitAsync(TestContext.Current.CancellationToken);
            charts.Status.ShouldBe("Cannot open document");

            charts.SelectedRepository = "first";
            charts.ItemCount.ShouldBe(1);
            helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new("second", "https://second.test")]);
            await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
            charts.SelectedRepository.ShouldBe("second");
            charts.ItemCount.ShouldBe(2);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Charts_show_all_repositories_when_all_selected()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("first", "https://first.test"), new("second", "https://second.test")]);
        helm.Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), true, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new("first/web", "web", "2.0"),
                new("second/api", "api", "1.0")]);
        using var charts = new HelmChartsViewModel(helm.Object, Mock.Of<IHelmDocumentOpener>());
        charts.Initialize(workspace);
        await charts.RefreshCommand.ExecutionTask!.WaitAsync(TestContext.Current.CancellationToken);

        charts.SelectedRepository = Assets.Resources.HelmChartsView_AllRepositories;
        charts.ItemCount.ShouldBe(2);

        charts.SelectedRepository = "first";
        charts.ItemCount.ShouldBe(1);

        charts.SelectedRepository = "second";
        charts.ItemCount.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task Charts_namespace_selection_linked_to_workspace_by_default()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("repo", "https://repo.test")]);
        helm.Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), true, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("repo/web", "web", "1.0")]);
        using var charts = new HelmChartsViewModel(helm.Object, Mock.Of<IHelmDocumentOpener>());
        charts.IsNamespaceSelectionLinked.ShouldBeTrue();
        charts.SelectedNamespaces.ShouldBeSameAs(workspace.SelectedNamespaces);

        charts.Initialize(workspace);
        await charts.RefreshCommand.ExecutionTask!.WaitAsync(TestContext.Current.CancellationToken);

        workspace.SelectedNamespaces.Clear();
        workspace.SelectedNamespaces.Add(new V1Namespace { Metadata = new V1ObjectMeta { Name = "prod" } });
        charts.SelectedNamespaces.ShouldContain(ns => ns.Metadata != null && ns.Metadata.Name == "prod");
    }

    [AvaloniaFact]
    public async Task Charts_namespace_selection_can_be_unlinked()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("repo", "https://repo.test")]);
        helm.Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), true, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("repo/web", "web", "1.0")]);
        using var charts = new HelmChartsViewModel(helm.Object, Mock.Of<IHelmDocumentOpener>());
        charts.Initialize(workspace);
        await charts.RefreshCommand.ExecutionTask!.WaitAsync(TestContext.Current.CancellationToken);

        charts.IsNamespaceSelectionLinked = false;
        charts.SelectedNamespaces.ShouldNotBeSameAs(workspace.SelectedNamespaces);
        charts.SelectedNamespaces.Clear();
        charts.SelectedNamespaces.Add(new V1Namespace { Metadata = new V1ObjectMeta { Name = "custom" } });

        workspace.SelectedNamespaces.Clear();
        workspace.SelectedNamespaces.Add(new V1Namespace { Metadata = new V1ObjectMeta { Name = "prod" } });
        charts.SelectedNamespaces.ShouldContain(ns => ns.Metadata != null && ns.Metadata.Name == "custom");
        charts.SelectedNamespaces.ShouldNotContain(ns => ns.Metadata != null && ns.Metadata.Name == "prod");
    }

    [AvaloniaFact]
    public async Task Charts_disable_refresh_while_loading_and_keep_repository_selection()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        var pending = new TaskCompletionSource<IReadOnlyList<HelmRepository>>(TaskCreationOptions.RunContinuationsAsynchronously);
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .Returns(pending.Task);
        using var charts = new HelmChartsViewModel(helm.Object, Mock.Of<IHelmDocumentOpener>());
        await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        charts.Initialize(workspace);
        charts.IsLoading.ShouldBeTrue();
        charts.RefreshCommand.CanExecute(null).ShouldBeFalse();
        pending.SetResult([new("repo", "https://repo.test")]);
        await charts.RefreshCommand.ExecutionTask!.WaitAsync(TestContext.Current.CancellationToken);
        charts.IsLoading.ShouldBeFalse();
        charts.RefreshCommand.CanExecute(null).ShouldBeTrue();
        await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        charts.SelectedRepository.ShouldBe("repo");
        charts.SearchText = "web";
        helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("repo/web", "web", "1.0")]);
        await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        charts.ItemCount.ShouldBe(1);
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Repository configuration unavailable"));
        await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        charts.Status.ShouldBe("Repository configuration unavailable");
        charts.ItemCount.ShouldBe(0);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Charts_report_empty_search_results_or_discovery_errors(bool fails)
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("repo", "https://repo.test")]);
        var search = helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
        if (fails)
        {
            search.ThrowsAsync(new InvalidOperationException("Index unavailable"));
        }
        else
        {
            search.ReturnsAsync([]);
        }

        using var charts = new HelmChartsViewModel(helm.Object, Mock.Of<IHelmDocumentOpener>());
        charts.Initialize(workspace);
        await charts.RefreshCommand.ExecutionTask!.WaitAsync(TestContext.Current.CancellationToken);
        charts.SearchText = "missing";
        await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        charts.Status.ShouldBe(fails ? "Index unavailable" : Assets.Resources.Shared_NoCharts);
        charts.ItemCount.ShouldBe(0);
        charts.IsLoading.ShouldBeFalse();
        await charts.InstallChartCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        charts.SearchText = "";
        await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        charts.SelectedRepository.ShouldBe(Assets.Resources.HelmChartsView_AllRepositories);
        charts.Repositories.ShouldBe([Assets.Resources.HelmChartsView_AllRepositories]);
    }

    private static HelmDocumentOpener CreateOpener(Mock<IHelmService> helm, Mock<IFactory> factory)
        => new(
            () => new HelmReleaseEditorViewModel(
                helm.Object,
                factory.Object,
                Mock.Of<IHelmListNotifier>(),
                Mock.Of<IHelmReleasePreviewService>(),
                Mock.Of<IDialogService>()),
            () => new HelmRepositoryEditorViewModel(
                helm.Object,
                factory.Object,
                Mock.Of<IHelmListNotifier>()),
            factory.Object);
}
