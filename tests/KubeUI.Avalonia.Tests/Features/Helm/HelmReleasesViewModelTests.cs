using Avalonia.Headless.XUnit;
using Dock.Model.Core;
using Helm.NET;
using HanumanInstitute.MvvmDialogs;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Features.Helm.Releases;
using KubeUI.Kubernetes;
using k8s.Models;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Helm;

public sealed class HelmReleasesViewModelTests
{
    [AvaloniaFact]
    public async Task Release_filter_follows_linked_and_local_namespace_selection()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        workspace.SelectedNamespaces.Add(new V1Namespace { Metadata = new V1ObjectMeta { Name = "apps" } });
        workspace.SelectedNamespaces.Add(new V1Namespace { Metadata = new V1ObjectMeta { Name = "system" } });
        var releases = new[]
        {
            new HelmRelease("web", "apps", "1", "2026-01-01", "deployed", "nginx-1.0.0", "1.0.0"),
            new HelmRelease("dns", "system", "1", "2026-01-01", "deployed", "dns-1.0.0", "1.0.0"),
            new HelmRelease("other", "other", "1", "2026-01-01", "deployed", "other-1.0.0", "1.0.0")
        };
        var helmService = new Mock<IHelmService>();
        var previewService = new Mock<IHelmReleasePreviewService>();
        helmService
            .Setup(service => service.ListReleasesAsync(
                It.IsAny<HelmConnection>(),
                It.IsAny<bool>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(releases);
        var factory = new Mock<IFactory>();
        var dialogs = new Mock<IDialogService>();
        using var viewModel = new HelmReleasesViewModel(helmService.Object, factory.Object, dialogs.Object, previewService.Object);

        viewModel.Initialize(workspace);
        await viewModel.RefreshCommand.ExecuteAsync(null);

        ReferenceEquals(viewModel.SelectedNamespaces, workspace.SelectedNamespaces).ShouldBeTrue();
        await TestWait.UntilAsync(() => viewModel.ItemCount == 2, TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
        helmService.Verify(service => service.ListReleasesAsync(
            It.Is<HelmConnection>(connection => connection.Namespace == "apps"),
            true,
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);

        workspace.SelectedNamespaces.RemoveAt(1);
        await TestWait.UntilAsync(() => viewModel.ItemCount == 1, TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);

        viewModel.IsNamespaceSelectionLinked = false;
        viewModel.SelectedNamespaces.Add(new V1Namespace { Metadata = new V1ObjectMeta { Name = "system" } });
        await TestWait.UntilAsync(() => viewModel.ItemCount == 2, TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
        workspace.SelectedNamespaces.Count.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task Selected_release_actions_call_helm_and_present_results()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helmService = new Mock<IHelmService>();
        var previewService = new Mock<IHelmReleasePreviewService>();
        var release = new HelmRelease("web", "apps", "2", "2026-01-02", "deployed", "nginx-1.1.0", "1.1.0");
        helmService
            .Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HelmRelease>());
        helmService
            .Setup(service => service.StatusAsync(It.IsAny<HelmConnection>(), release.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(release);
        helmService
            .Setup(service => service.GetValuesAsync(It.IsAny<HelmConnection>(), release.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, System.Text.Json.JsonElement>());
        helmService
            .Setup(service => service.HistoryAsync(It.IsAny<HelmConnection>(), release.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync([release]);
        helmService
            .Setup(service => service.RollbackAsync(It.IsAny<HelmConnection>(), release.Name, 4, It.IsAny<CancellationToken>(), false))
            .Returns(Task.CompletedTask);
        previewService
            .Setup(service => service.PreviewAsync(
                It.IsAny<ClusterWorkspace>(), It.IsAny<HelmConnection>(), release.Name, release.Chart,
                It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var factory = new Mock<IFactory>();
        var dialogs = new Mock<IDialogService>();
        using var viewModel = new HelmReleasesViewModel(helmService.Object, factory.Object, dialogs.Object, previewService.Object);
        viewModel.Initialize(workspace);

        await viewModel.ViewStatusCommand.ExecuteAsync(release).WaitAsync(TestContext.Current.CancellationToken);
        viewModel.Status.ShouldContain("deployed");
        await viewModel.ViewValuesCommand.ExecuteAsync(release).WaitAsync(TestContext.Current.CancellationToken);
        viewModel.Status.ShouldContain("{}");
        await viewModel.ViewHistoryCommand.ExecuteAsync(release).WaitAsync(TestContext.Current.CancellationToken);
        viewModel.Status.ShouldContain("2: deployed");

        viewModel.RollbackRevision = 4;
        await viewModel.RollbackCommand.ExecuteAsync(release).WaitAsync(TestContext.Current.CancellationToken);
        await viewModel.VisualizeReleaseCommand.ExecuteAsync(release).WaitAsync(TestContext.Current.CancellationToken);

        helmService.Verify(service => service.RollbackAsync(
            It.Is<HelmConnection>(connection => connection.Namespace == "apps"),
            release.Name,
            4,
            It.IsAny<CancellationToken>(),
            false), Times.Once);
        previewService.Verify(service => service.PreviewAsync(
            workspace,
            It.Is<HelmConnection>(connection => connection.Namespace == release.Namespace),
            release.Name,
            release.Chart,
            It.IsAny<IReadOnlyDictionary<string, object?>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
