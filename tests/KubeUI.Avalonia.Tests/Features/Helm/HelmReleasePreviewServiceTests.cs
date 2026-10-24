using Avalonia.Headless.XUnit;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using Helm.NET;
using k8s.Models;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Resources.Relationships;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Helm;

public sealed class HelmReleasePreviewServiceTests
{
    [AvaloniaFact]
    public async Task Versioned_preview_renders_selected_version_with_edited_values()
    {
        using var cluster = await Application.Current.CreateClusterAsync(connect: false);
        using var viewModel = new VisualizationViewModel();
        var helm = new Mock<IHelmService>();
        var connection = new HelmConnection("config", "context", "apps");
        Dictionary<string, object?> values = new() { ["replicas"] = 2 };
        helm.Setup(service => service.TemplateVersionAsync(connection, "web", "repo/web", "2.0", values,
            It.IsAny<CancellationToken>())).ReturnsAsync("apiVersion: v1\nkind: ConfigMap\nmetadata:\n  name: preview\n");
        var factory = CreateFactory();
        var service = new HelmReleasePreviewService(helm.Object, () => viewModel, factory.Object);

        await service.PreviewVersionAsync(cluster, connection, "web", "repo/web", "2.0", values,
            TestContext.Current.CancellationToken);

        factory.Verify(item => item.InsertDockable(
            It.IsAny<IDocumentDock>(), viewModel, It.IsAny<int>()), Times.Once);
        viewModel.IsPreview.ShouldBeTrue();
        helm.VerifyAll();
    }

    [AvaloniaFact]
    public async Task VisualizeRelease_opens_live_release_scope_without_calling_helm()
    {
        using var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        using var viewModel = new VisualizationViewModel();
        var helm = new Mock<IHelmService>(MockBehavior.Strict);
        var factory = CreateFactory();
        var sut = new HelmReleasePreviewService(helm.Object, () => viewModel, factory.Object);

        await cluster.Runtime.AddOrUpdateResource(new V1Namespace
        {
            ApiVersion = "v1",
            Kind = "Namespace",
            Metadata = new V1ObjectMeta { Name = "kube-system" }
        });
        sut.VisualizeRelease(cluster, "cilium", "kube-system");

        viewModel.IsPreview.ShouldBeFalse();
        viewModel.SelectedNamespaces.Select(item => item.Metadata.Name).ShouldBe(["kube-system"]);
        viewModel.IsNamespaceSelectionEnabled.ShouldBeFalse();
        viewModel.IsNamespaceSelectionLinked.ShouldBeFalse();
        viewModel.RootResourceDisplay.ShouldBe("Helm release: cilium (kube-system)");
        viewModel.IsRootResourceDisplayVisible.ShouldBeTrue();
        await cluster.Runtime.SeedResource<V1ConfigMap>(true, TestContext.Current.CancellationToken);
        foreach (var namespaceName in new[] { "kube-system", "other" })
        {
            await cluster.Runtime.AddOrUpdateResource(new V1ConfigMap
            {
                ApiVersion = "v1",
                Kind = "ConfigMap",
                Metadata = new V1ObjectMeta
                {
                    Name = namespaceName,
                    NamespaceProperty = "kube-system",
                    Annotations = new Dictionary<string, string>
                    {
                        ["meta.helm.sh/release-name"] = "cilium",
                        ["meta.helm.sh/release-namespace"] = namespaceName
                    }
                }
            });
        }

        await TestWait.UntilAsync(() => viewModel.Graph!.Resources.Count == 1,
            TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
        viewModel.Graph!.Resources.Single().Metadata.Name.ShouldBe("kube-system");
        factory.Verify(item => item.InsertDockable(
            It.IsAny<IDocumentDock>(), viewModel, It.IsAny<int>()), Times.Once);
        helm.VerifyNoOtherCalls();
    }

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
        var factory = CreateFactory(dockable => openedViewModel = dockable as VisualizationViewModel);
        var sut = new HelmReleasePreviewService(helmService.Object, static () => new VisualizationViewModel(), factory.Object);

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
            viewModel.Graph!.Resources.OfType<V1Deployment>().ShouldNotBeEmpty();
            viewModel.Graph.Resources.OfType<V1ReplicaSet>().ShouldNotBeEmpty();
            viewModel.Graph!.Relationships.ShouldContain(relationship => relationship.Kind == ResourceRelationshipKind.Owner);
        }

        cluster.Runtime.Objects.Count.ShouldBe(runtimeObjectCount);
        helmService.Verify(service => service.TemplateAsync(
            It.IsAny<HelmConnection>(),
            "web",
            "bitnami/nginx",
            It.IsAny<IReadOnlyDictionary<string, object?>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        factory.Verify(item => item.InsertDockable(
            It.IsAny<IDocumentDock>(), It.IsAny<VisualizationViewModel>(), It.IsAny<int>()), Times.Once);
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
        var factory = CreateFactory();
        var sut = new HelmReleasePreviewService(helmService.Object, static () => new VisualizationViewModel(), factory.Object);

        await Should.ThrowAsync<InvalidOperationException>(() => sut.PreviewAsync(
            cluster,
            new HelmConnection("config", "context", "apps"),
            "web",
            "bitnami/nginx",
            cancellationToken: TestContext.Current.CancellationToken));

        factory.Verify(item => item.InsertDockable(
            It.IsAny<IDocumentDock>(), It.IsAny<VisualizationViewModel>(), It.IsAny<int>()), Times.Never);
    }

    private static Mock<IFactory> CreateFactory(Action<IDockable>? onInsert = null)
    {
        var factory = new Mock<IFactory>();
        var documents = new DocumentDock { VisibleDockables = [] };
        factory.Setup(item => item.GetDockable<IDocumentDock>("Documents")).Returns(documents);
        factory.Setup(item => item.InsertDockable(documents, It.IsAny<IDockable>(), It.IsAny<int>()))
            .Callback<IDock, IDockable, int>((_, dockable, _) => onInsert?.Invoke(dockable));
        return factory;
    }
}
