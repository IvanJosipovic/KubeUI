using Avalonia.Headless.XUnit;
using Dock.Model.Controls;
using Dock.Model.Core;
using Helm.NET;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Features.Helm.Charts;
using KubeUI.Kubernetes;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Helm;

public sealed class HelmDocumentOpenerTests
{
    [AvaloniaFact]
    public async Task Open_charts_creates_document_and_applies_repository_filter()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var documents = factory.GetDockable<IDocumentDock>("Documents").ShouldNotBeNull();
        var id = $"{nameof(HelmChartsViewModel)}-{workspace.Runtime.Name}";
        var created = 0;
        var helmService = new Mock<IHelmService>();
        helmService.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("bitnami", "https://charts.example.test")]);
        helmService.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var opener = CreateOpener(factory, () =>
        {
            created++;
            return new HelmChartsViewModel(helmService.Object, Mock.Of<IHelmDocumentOpener>());
        });

        try
        {
            opener.OpenCharts(workspace, "bitnami");

            var charts = factory.FindDockableById(id).ShouldBeOfType<HelmChartsViewModel>();
            created.ShouldBe(1);
            charts.SelectedRepository.ShouldBeNull();
            await charts.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
            charts.SelectedRepository.ShouldBe("bitnami");
            documents.VisibleDockables.ShouldContain(charts);
            documents.ActiveDockable.ShouldBeSameAs(charts);
        }
        finally
        {
            if (factory.FindDockableById(id) is { } document)
            {
                factory.CloseDockable(document);
            }
        }
    }

    [AvaloniaFact]
    public async Task Open_charts_updates_existing_document_and_keeps_single_tab()
    {
        using var workspace = await Application.Current.CreateClusterAsync(connect: false);
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var documents = factory.GetDockable<IDocumentDock>("Documents").ShouldNotBeNull();
        var id = $"{nameof(HelmChartsViewModel)}-{workspace.Runtime.Name}";
        var created = 0;
        var helmService = new Mock<IHelmService>();
        helmService.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("first", "https://first.example.test"), new("second", "https://second.example.test")]);
        helmService.Setup(service => service.ListChartsAsync(It.IsAny<HelmConnection>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var opener = CreateOpener(factory, () =>
        {
            created++;
            return new HelmChartsViewModel(helmService.Object, Mock.Of<IHelmDocumentOpener>());
        });

        try
        {
            opener.OpenCharts(workspace, "first");
            var first = factory.FindDockableById(id).ShouldBeOfType<HelmChartsViewModel>();

            opener.OpenCharts(workspace, "second");

            var second = factory.FindDockableById(id).ShouldBeOfType<HelmChartsViewModel>();
            second.ShouldBeSameAs(first);
            await second.RefreshCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
            second.SelectedRepository.ShouldBe("second");
            created.ShouldBe(2);
            documents.VisibleDockables!.Count(document => document.Id == id).ShouldBe(1);
        }
        finally
        {
            if (factory.FindDockableById(id) is { } document)
            {
                factory.CloseDockable(document);
            }
        }
    }

    private static HelmDocumentOpener CreateOpener(IFactory factory, Func<HelmChartsViewModel> createCharts)
        => new(
            createCharts,
            () => throw new NotSupportedException(),
            () => throw new NotSupportedException(),
            factory);
}
