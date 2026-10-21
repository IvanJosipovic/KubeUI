using Avalonia.Headless.XUnit;
using Dock.Model.Core;
using Helm.NET;
using HanumanInstitute.MvvmDialogs;
using KubeUI.Avalonia.Features.Helm.Repositories;
using KubeUI.Kubernetes;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Helm;

public sealed class HelmRepositoriesViewModelTests
{
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
        var factory = new Mock<IFactory>();
        var dialogs = new Mock<IDialogService>();
        using var viewModel = new HelmRepositoriesViewModel(helmService.Object, factory.Object, dialogs.Object);
        viewModel.Initialize(workspace);

        await viewModel.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        await viewModel.UpdateIndexesCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);

        helmService.Verify(service => service.ListRepositoriesAsync(
            It.Is<HelmConnection>(connection => connection.KubeConfigPath == workspace.Runtime.KubeConfigPath),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        helmService.Verify(service => service.UpdateRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()), Times.Once);
        viewModel.Status.ShouldBe(Assets.Resources.HelmRepositoriesView_IndexesUpdated);
    }
}
