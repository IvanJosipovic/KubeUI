using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Model.Core;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using Helm.NET;
using KubeUI.Controls.Layout;
using KubeUI.Features.Helm;
using KubeUI.Features.Helm.Helm;
using KubeUI.Features.Helm.Releases;
using KubeUI.Features.Helm.Repositories;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;
using Moq;
using Shouldly;

namespace KubeUI.Features.Helm.Tests.Repositories;

public sealed class HelmRepositoriesViewModelTests : IDisposable
{
    private readonly ServiceProvider _serializationServices = new ServiceCollection()
        .AddKubeUIKubernetesServices(jsonAllowReflection: false)
        .BuildServiceProvider();
    private IKubernetesYamlSerializer _yamlSerializer => _serializationServices.GetRequiredService<IKubernetesYamlSerializer>();

    public void Dispose() => _serializationServices.Dispose();

    [AvaloniaFact]
    public async Task Repository_header_matches_release_header_spacing_count_and_action_alignment()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new HelmRepository("test", "https://example.test")]);
        helm.Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), true,
            It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        using var repositories = new HelmRepositoriesViewModel(helm.Object, Mock.Of<IDialogService>(),
            Mock.Of<IHelmDocumentOpener>(), Mock.Of<IHelmListNotifier>());
        using var releases = new HelmReleasesViewModel(helm.Object, _yamlSerializer, Mock.Of<IFactory>(), Mock.Of<IDialogService>(),
            Mock.Of<IHelmReleasePreviewService>(),
            () => Application.Current.GetRequiredTestService<ResourceYamlViewModel>(),
            Mock.Of<IHelmDocumentOpener>(), Mock.Of<IHelmListNotifier>());
        repositories.Initialize(workspace);
        releases.Initialize(workspace);
        await repositories.RefreshCommand.ExecutionTask!.WaitAsync(TestContext.Current.CancellationToken);
        var repositoryView = new HelmRepositoriesView { DataContext = repositories };
        var releaseView = new HelmReleasesView { DataContext = releases };
        using var window = Application.Current.CreateTestWindow(content: new StackPanel
        {
            Children = { repositoryView, releaseView }
        });
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            var repositoryHeader = Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(repositoryView)
                .OfType<CompactToolbar>().Single();
            var releaseHeader = Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(releaseView)
                .OfType<CompactToolbar>().Single();
            repositoryHeader.MinHeight.ShouldBe(releaseHeader.MinHeight);
            repositoryHeader.Margin.ShouldBe(releaseHeader.Margin);
            var leading = repositoryHeader.Leading.ShouldBeOfType<Button>();
            leading.Command.ShouldBeSameAs(repositories.NewRepositoryCommand);
            var count = repositoryHeader.Middle.ShouldBeOfType<Label>();
            count.Content.ShouldBe(string.Format(HelmLanguageManager.Instance.HelmRepositoriesView_ItemsFormat.CurrentValue()!, 1));
            var leadingPosition = leading.TranslatePoint(default, repositoryHeader)!.Value;
            leadingPosition.X.ShouldBe(2);
            var countPosition = count.TranslatePoint(default, repositoryHeader)!.Value;
            countPosition.X.ShouldBeGreaterThan(leadingPosition.X + leading.Bounds.Width);
            var actions = repositoryHeader.Trailing.ShouldBeOfType<StackPanel>();
            var actionPosition = actions.TranslatePoint(default, repositoryHeader)!.Value;
            actionPosition.X.ShouldBeGreaterThan(countPosition.X + count.Bounds.Width);
            actions.TranslatePoint(new Point(actions.Bounds.Width, 0), repositoryHeader)!.Value.X
                .ShouldBe(repositoryHeader.Bounds.Width);
            actions.Children.OfType<Button>().Select(button => button.Command)
                .ShouldBe([repositories.UpdateIndexesCommand, repositories.RefreshCommand]);
            actions.Children.OfType<Button>().Select(button => button.Content.ShouldBeOfType<FluentIcon>().Icon)
                .ShouldBe([Icon.ArrowDownload, Icon.ArrowSync]);
            helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            await repositories.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
            count.Content.ShouldBe(string.Format(HelmLanguageManager.Instance.HelmRepositoriesView_ItemsFormat.CurrentValue()!, 0));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Refresh_and_update_indexes_use_the_configured_helm_service()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helmService = new Mock<IHelmService>();
        var repository = new HelmRepository("bitnami", "https://charts.bitnami.com/bitnami");
        helmService
            .Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([repository]);
        helmService
            .Setup(service => service.UpdateRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var dialogs = new Mock<IDialogService>();
        using var viewModel = new HelmRepositoriesViewModel(helmService.Object, dialogs.Object,
            Mock.Of<IHelmDocumentOpener>(), Mock.Of<IHelmListNotifier>());
        viewModel.Initialize(workspace);

        await viewModel.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        await viewModel.UpdateIndexesCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        helmService.Verify(service => service.ListRepositoriesAsync(
            It.Is<HelmConnection>(connection => connection.KubeConfigPath == workspace.Runtime.KubeConfigPath),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        helmService.Verify(service => service.UpdateRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()), Times.Once);
        viewModel.Status.ShouldBe(HelmLanguageManager.Instance.HelmRepositoriesView_IndexesUpdated.CurrentValue());
    }

    [AvaloniaFact]
    public async Task Repository_editor_notifications_reload_open_lists()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helmService = new Mock<IHelmService>();
        helmService
            .Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new HelmRepository("first", "https://first.test")]);
        var notifier = new HelmListNotifier();
        using var viewModel = new HelmRepositoriesViewModel(helmService.Object, Mock.Of<IDialogService>(),
            Mock.Of<IHelmDocumentOpener>(), notifier);
        viewModel.Initialize(workspace);
        await viewModel.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        viewModel.ItemCount.ShouldBe(1);

        helmService
            .Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new HelmRepository("first", "https://first.test"),
                new HelmRepository("second", "https://second.test")]);
        notifier.NotifyRepositoriesChanged();

        await TestWait.UntilAsync(() => viewModel.ItemCount == 2,
            TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
    }

    [AvaloniaFact]
    public async Task View_charts_opens_the_charts_document_for_the_repository()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helmService = new Mock<IHelmService>();
        helmService
            .Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new HelmRepository("bitnami", "https://charts.bitnami.com/bitnami")]);
        var opener = new Mock<IHelmDocumentOpener>();
        using var viewModel = new HelmRepositoriesViewModel(helmService.Object, Mock.Of<IDialogService>(),
            opener.Object, Mock.Of<IHelmListNotifier>());
        viewModel.Initialize(workspace);
        await viewModel.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        viewModel.ViewChartsCommand.Execute(new HelmRepository("bitnami", "https://charts.bitnami.com/bitnami"));

        opener.Verify(item => item.OpenCharts(workspace, "bitnami"), Times.Once);
    }
}
