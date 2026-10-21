using Avalonia.Headless.XUnit;
using Helm.NET;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Resources.Relationships;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Helm;

public sealed class HelmReleasePreviewServiceTests
{
    [AvaloniaFact]
    public async Task PreviewAsync_opens_graph_for_all_rendered_resources_without_applying_them()
    {
        const string manifest = """
            apiVersion: apps/v1
            kind: Deployment
            metadata:
              name: web
              namespace: apps
              uid: deployment-uid
            spec: {}
            ---
            apiVersion: apps/v1
            kind: ReplicaSet
            metadata:
              name: web-abc
              namespace: apps
              uid: replicaset-uid
              ownerReferences:
                - apiVersion: apps/v1
                  kind: Deployment
                  name: web
                  uid: deployment-uid
            spec: {}
            """;
        using var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        var runtimeObjectCount = cluster.Runtime.Objects.Count;
        VisualizationViewModel? openedViewModel = null;
        var helmService = new Mock<IHelmService>();
        helmService
            .Setup(service => service.TemplateAsync(
                It.IsAny<HelmConnection>(),
                "web",
                "bitnami/nginx",
                It.IsAny<IReadOnlyDictionary<string, object?>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(manifest);
        var opener = new Mock<IHelmVisualizationDocumentOpener>();
        opener.Setup(service => service.Open(It.IsAny<VisualizationViewModel>()))
            .Callback<VisualizationViewModel>(viewModel => openedViewModel = viewModel);
        var sut = new HelmReleasePreviewService(helmService.Object, static () => new VisualizationViewModel(), opener.Object);

        await sut.PreviewAsync(
            cluster,
            new HelmConnection("config", "context", "apps"),
            "web",
            "bitnami/nginx",
            new Dictionary<string, object?> { ["replicas"] = 2 },
            TestContext.Current.CancellationToken);

        var viewModel = openedViewModel.ShouldNotBeNull();
        using (viewModel)
        {
            await TestWait.UntilAsync(
                () => viewModel.Graph?.Resources.Count == 2,
                TimeSpan.FromSeconds(5),
                cancellationToken: TestContext.Current.CancellationToken);
            viewModel.IsPreview.ShouldBeTrue();
            viewModel.PreviewTitle.ShouldBe("web");
            viewModel.Graph!.Relationships.ShouldContain(relationship => relationship.Kind == ResourceRelationshipKind.Owner);
        }

        cluster.Runtime.Objects.Count.ShouldBe(runtimeObjectCount);
        helmService.Verify(service => service.TemplateAsync(
            It.IsAny<HelmConnection>(),
            "web",
            "bitnami/nginx",
            It.IsAny<IReadOnlyDictionary<string, object?>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        opener.Verify(service => service.Open(It.IsAny<VisualizationViewModel>()), Times.Once);
    }

    [AvaloniaFact]
    public async Task PreviewAsync_rejects_rendered_output_without_resources()
    {
        using var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        var helmService = new Mock<IHelmService>();
        helmService
            .Setup(service => service.TemplateAsync(
                It.IsAny<HelmConnection>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyDictionary<string, object?>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);
        var opener = new Mock<IHelmVisualizationDocumentOpener>();
        var sut = new HelmReleasePreviewService(helmService.Object, static () => new VisualizationViewModel(), opener.Object);

        await Should.ThrowAsync<InvalidOperationException>(() => sut.PreviewAsync(
            cluster,
            new HelmConnection("config", "context", "apps"),
            "web",
            "bitnami/nginx",
            cancellationToken: TestContext.Current.CancellationToken));

        opener.Verify(service => service.Open(It.IsAny<VisualizationViewModel>()), Times.Never);
    }
}
