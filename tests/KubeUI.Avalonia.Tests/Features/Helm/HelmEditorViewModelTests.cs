using Avalonia.Headless.XUnit;
using Dock.Model.Core;
using Helm.NET;
using HanumanInstitute.MvvmDialogs;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Features.Helm.Releases;
using KubeUI.Avalonia.Features.Helm.Repositories;
using KubeUI.Kubernetes;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Helm;

public sealed class HelmEditorViewModelTests
{
    [AvaloniaFact]
    public async Task Repository_form_saves_repository_and_credentials()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helmService = new Mock<IHelmService>();
        helmService
            .Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HelmRepository>());
        helmService
            .Setup(service => service.AddRepositoryAsync(
                It.IsAny<HelmConnection>(),
                "private",
                "https://charts.example.com",
                "user",
                "secret",
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var factory = new Mock<IFactory>();
        var dialogs = new Mock<IDialogService>();
        using var repositories = new HelmRepositoriesViewModel(helmService.Object, factory.Object, dialogs.Object);
        repositories.Initialize(workspace);
        var editor = new HelmRepositoryEditorViewModel(helmService.Object, factory.Object, repositories);
        editor.Initialize(workspace, new HelmConnection("config", "context", null), null);
        editor.Name = "private";
        editor.Url = "https://charts.example.com";
        editor.Username = "user";
        editor.Password = "secret";

        await editor.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        helmService.Verify(service => service.AddRepositoryAsync(
            It.IsAny<HelmConnection>(),
            "private",
            "https://charts.example.com",
            "user",
            "secret",
            It.IsAny<CancellationToken>()), Times.Once);
        factory.Verify(service => service.RemoveDockable(editor, false), Times.Once);
    }

    [AvaloniaFact]
    public async Task Release_form_previews_and_installs_chart_with_values()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var helmService = new Mock<IHelmService>();
        var previewService = new Mock<IHelmReleasePreviewService>();
        helmService
            .Setup(service => service.ListReleasesAsync(It.IsAny<HelmConnection>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HelmRelease>());
        previewService
            .Setup(service => service.PreviewAsync(
                workspace,
                It.IsAny<HelmConnection>(),
                "web",
                "bitnami/nginx",
                It.IsAny<IReadOnlyDictionary<string, object?>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        helmService
            .Setup(service => service.InstallAsync(
                It.IsAny<HelmConnection>(), "web", "bitnami/nginx", It.IsAny<IReadOnlyDictionary<string, object?>>(), true, true,
                TimeSpan.FromSeconds(30), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HelmRelease("web", "apps", "1", "2026-01-01", "deployed", "nginx-1.0.0", "1.0.0"));
        var factory = new Mock<IFactory>();
        var dialogs = HelmDialogTestHelper.CreateConfirmingDialogService();
        using var releases = new HelmReleasesViewModel(helmService.Object, factory.Object, dialogs, previewService.Object);
        releases.Initialize(workspace);
        var editor = new HelmReleaseEditorViewModel(helmService.Object, factory.Object, releases, previewService.Object, dialogs);
        editor.Initialize(workspace, new HelmConnection("config", "context", "apps"), null);
        editor.ReleaseName = "web";
        editor.Chart = "bitnami/nginx";
        editor.Namespace = "apps";
        editor.ValuesJson = "{\"replicas\": 3}";
        editor.CreateNamespace = true;
        editor.WaitForReady = true;
        editor.TimeoutSeconds = 30;

        await editor.PreviewCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        await editor.SaveCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        previewService.Verify(service => service.PreviewAsync(
            workspace,
            It.Is<HelmConnection>(connection => connection.Namespace == "apps"),
            "web",
            "bitnami/nginx",
            It.Is<IReadOnlyDictionary<string, object?>>(values => values.ContainsKey("replicas")),
            It.IsAny<CancellationToken>()), Times.Once);
        helmService.Verify(service => service.InstallAsync(
            It.Is<HelmConnection>(connection => connection.Namespace == "apps"),
            "web",
            "bitnami/nginx",
            It.Is<IReadOnlyDictionary<string, object?>>(values => values.ContainsKey("replicas")),
            true,
            true,
            TimeSpan.FromSeconds(30),
            It.IsAny<CancellationToken>()), Times.Once);
        factory.Verify(service => service.RemoveDockable(editor, false), Times.Once);
    }
}
