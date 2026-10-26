using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using HanumanInstitute.MvvmDialogs;
using Helm.NET;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Features.Helm.Charts;
using KubeUI.Avalonia.Features.Helm.Releases;
using KubeUI.Avalonia.Features.Helm.Repositories;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using SvcSystems.Avalonia.DynamicTableView;

namespace KubeUI.Avalonia.Tests.Features.Helm;

public sealed class HelmChartsViewModelTests
{
    [AvaloniaFact]
    public async Task Charts_refresh_when_view_loads()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new HelmRepository("repo", "https://repo.test")]);
        helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new HelmChartSearchResult("repo/chart", "chart", "1.0.0")]);
        using var charts = new HelmChartsViewModel(helm.Object, Mock.Of<IHelmDocumentOpener>());
        charts.Status.ShouldBeEmpty();
        charts.Initialize(workspace);
        charts.SelectRepository("repo");
        charts.ItemCount.ShouldBe(0);

        var view = new HelmChartsView { DataContext = charts };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        try
        {
            await TestWait.UntilAsync(() => charts.ItemCount == 1,
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);

            global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<ComboBox>().Single().SelectedItem.ShouldBe("repo");
            helm.Verify(service => service.ListChartsAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            window.Close();
        }
    }

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
        helm.Setup(service => service.GetChartValuesAsync(
                It.IsAny<HelmConnection>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);
        var factory = new Mock<IFactory>();
        var documents = new DocumentDock { VisibleDockables = [] };
        factory.Setup(item => item.GetDockable<IDocumentDock>("Documents")).Returns(documents);
        HelmReleaseEditorViewModel? opened = null;
        factory.Setup(item => item.InsertDockable(documents, It.IsAny<IDockable>(), 0))
            .Callback<IDock, IDockable, int>((_, dockable, _) => opened = dockable.ShouldBeOfType<HelmReleaseEditorViewModel>());
        using var charts = new HelmChartsViewModel(helm.Object, CreateOpener(helm, factory));
        charts.SelectedRepository = "first";
        workspace.SelectedNamespaces.Clear();
        workspace.SelectedNamespaces.Add(new k8s.Models.V1Namespace
        {
            Metadata = new k8s.Models.V1ObjectMeta { Name = "apps" }
        });
        using var services = new ServiceCollection().AddSingleton(charts).BuildServiceProvider();
        var navigation = new HelmNavigationSectionProvider().Documents
            .Single(document => document.Id == HelmNavigationSectionProvider.ChartsDocumentId);
        navigation.CreateDockable(services, workspace).ShouldBeSameAs(charts);
        await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(() => charts.ItemCount == 1,
            TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
        charts.Repositories.ShouldBe(["Alpha", "beta", "first", "second", "zeta"]);
        charts.SelectedRepository.ShouldBe("first");
        charts.Status.ShouldBeEmpty();
        charts.ItemCount.ShouldBe(1);

        var view = new HelmChartsView { DataContext = charts };
        using var window = Application.Current.CreateTestWindow(content: view);
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            var controls = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view).ToArray();
            var header = controls.OfType<Grid>().First(grid => grid.ColumnDefinitions.Count == 3);
            header.MinHeight.ShouldBe(32);
            header.Margin.ShouldBe(new Thickness(0, 2));
            var repository = controls.OfType<ComboBox>().Single();
            repository.SelectedItem = "second";
            await TestWait.UntilAsync(() => charts.ItemCount == 2,
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
            var search = controls.OfType<TextBox>()
                .Single(control => control.PlaceholderText == Assets.Resources.HelmChartsView_SearchPlaceholder);
            search.Height.ShouldBe(32);
            search.VerticalContentAlignment.ShouldBe(VerticalAlignment.Center);
            search.Text = "tools";
            await TestWait.UntilAsync(() => charts.ItemCount == 1,
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
            search.Text = string.Empty;
            await TestWait.UntilAsync(() => charts.ItemCount == 2,
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
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
            opened.ValuesYaml.ShouldBeEmpty();
            charts.Status.ShouldBeEmpty();
            factory.Setup(item => item.InsertDockable(documents, It.IsAny<IDockable>(), 0))
                .Throws(new InvalidOperationException("Cannot open document"));
            await charts.InstallChartCommand.ExecuteAsync(menu.CommandParameter).WaitAsync(TestContext.Current.CancellationToken);
            charts.Status.ShouldBe("Cannot open document");

            charts.SelectedRepository = "first";
            await TestWait.UntilAsync(() => charts.ItemCount == 1,
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
            helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new("second", "https://second.test")]);
            await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
            charts.SelectedRepository.ShouldBe("second");
            await TestWait.UntilAsync(() => charts.ItemCount == 2,
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
        }
        finally
        {
            window.Close();
        }
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
        charts.Initialize(workspace);
        var refresh = charts.RefreshCommand.ExecuteAsync(null);
        charts.IsLoading.ShouldBeTrue();
        charts.RefreshCommand.CanExecute(null).ShouldBeFalse();
        pending.SetResult([new("repo", "https://repo.test")]);
        await refresh.WaitAsync(TestContext.Current.CancellationToken);
        charts.IsLoading.ShouldBeFalse();
        charts.RefreshCommand.CanExecute(null).ShouldBeTrue();
        await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        charts.SelectedRepository.ShouldBe("repo");
        helm.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("repo/web", "web", "1.0")]);
        await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(() => charts.ItemCount == 1,
            TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Repository configuration unavailable"));
        await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        charts.Status.ShouldBe("Repository configuration unavailable");
        await TestWait.UntilAsync(() => charts.ItemCount == 0,
            TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
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
        await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        charts.Status.ShouldBe(fails ? "Index unavailable" : Assets.Resources.Shared_NoCharts);
        charts.ItemCount.ShouldBe(0);
        charts.IsLoading.ShouldBeFalse();
        await charts.InstallChartCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        charts.SelectedRepository.ShouldBeNull();
        charts.Repositories.ShouldBeEmpty();
    }

    private static HelmDocumentOpener CreateOpener(Mock<IHelmService> helm, Mock<IFactory> factory)
        => new(
            () => new HelmChartsViewModel(helm.Object, Mock.Of<IHelmDocumentOpener>()),
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
