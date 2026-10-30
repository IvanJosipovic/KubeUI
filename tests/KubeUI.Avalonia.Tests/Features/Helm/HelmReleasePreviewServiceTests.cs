using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using AvaloniaEdit;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using Helm.NET;
using k8s.Models;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Features.Helm.Releases;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Resources.Relationships;
using KubeUI.Kubernetes.Serialization;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Helm;

public sealed class HelmReleasePreviewServiceTests : IDisposable
{
    private readonly KubernetesYamlSerializer _yamlSerializer = new(allowReflection: true);

    public void Dispose() => _yamlSerializer.Dispose();

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
        var service = new HelmReleasePreviewService(helm.Object, _yamlSerializer, () => viewModel, factory.Object);

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
        var sut = new HelmReleasePreviewService(helm.Object, _yamlSerializer, () => viewModel, factory.Object);

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
        var sut = new HelmReleasePreviewService(helmService.Object, _yamlSerializer, static () => new VisualizationViewModel(), factory.Object);

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
    public async Task New_release_preview_can_be_reopened_after_close()
    {
        const string chart = "oci://docker.io/envoyproxy/gateway-helm";
        const string manifest = """
            apiVersion: v1
            kind: ConfigMap
            metadata:
              name: preview-config
            ---
            # Helm charts can emit comment-only documents when a template is disabled.
            ---
            apiVersion: v1
            kind: ServiceAccount
            metadata:
              name: preview-account
            """;
        using var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        VisualizationViewModel? openedVisualization = null;
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.GetChartValuesAsync(
                It.IsAny<HelmConnection>(), chart, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("replicas: 2\n");
        helm.Setup(service => service.TemplateAsync(
                It.IsAny<HelmConnection>(), "gateway", chart,
                It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(manifest);
        var factory = CreateFactory(dockable => openedVisualization = dockable as VisualizationViewModel);
        var previewService = new HelmReleasePreviewService(helm.Object, _yamlSerializer, static () => new VisualizationViewModel(), factory.Object);
        var editor = new HelmReleaseEditorViewModel(
            helm.Object, _yamlSerializer,
            Mock.Of<IFactory>(),
            Mock.Of<IHelmListNotifier>(),
            previewService,
            Mock.Of<HanumanInstitute.MvvmDialogs.IDialogService>());
        editor.Initialize(cluster, new HelmConnection("config", "context", "apps"), null);
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);
        editor.ReleaseName = "gateway";
        editor.IsOciSource = true;
        editor.Chart = chart;
        await TestWait.UntilAsync(
            () => editor.ValuesYaml.Contains("replicas: 2", StringComparison.Ordinal),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        for (var attempt = 0; attempt < 2; attempt++)
        {
            await editor.PreviewCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
            var visualization = openedVisualization.ShouldNotBeNull();
            using (visualization)
            {
                var view = new VisualizationView { DataContext = visualization };
                using var window = Application.Current.CreateTestWindow(content: view);
                try
                {
                    window.Show();
                    await TestWait.UntilAsync(
                        () => visualization.Graph?.Resources.Count > 1 || visualization.Error is not null,
                        TimeSpan.FromSeconds(10),
                        cancellationToken: TestContext.Current.CancellationToken);
                    var graphControl = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                        .OfType<ResourceGraphControl>().Single();
                    await TestWait.UntilAsync(
                        () => graphControl.IsViewportStable || visualization.Error is not null,
                        TimeSpan.FromSeconds(10),
                        cancellationToken: TestContext.Current.CancellationToken);
                    visualization.Error.ShouldBeNull();
                }
                finally
                {
                    window.Close();
                }
            }

            openedVisualization = null;
        }

        helm.Verify(service => service.TemplateAsync(
            It.IsAny<HelmConnection>(), "gateway", chart,
            It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [AvaloniaFact]
    public async Task Repository_install_preview_preserves_edited_values_when_returning_to_editor()
    {
        using var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        VisualizationViewModel? openedVisualization = null;
        var helm = new Mock<IHelmService>();
        helm.Setup(service => service.ListRepositoriesAsync(It.IsAny<HelmConnection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("stable", "https://stable.test")]);
        helm.Setup(service => service.ListChartsAsync(
                It.IsAny<HelmConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("stable/web", "web", "1.0")]);
        helm.Setup(service => service.GetChartValuesAsync(
                It.IsAny<HelmConnection>(), "stable/web", "1.0", It.IsAny<CancellationToken>()))
            .ReturnsAsync("replicas: 2\nserver:\n  enabled: true\n");
        helm.Setup(service => service.TemplateVersionAsync(
                It.IsAny<HelmConnection>(), "web", "stable/web", "1.0",
                It.Is<IReadOnlyDictionary<string, object?>>(values => Convert.ToInt64(values["replicas"]) == 4),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((HelmConnection _, string _, string _, string _, IReadOnlyDictionary<string, object?> values, CancellationToken _) =>
                $"apiVersion: v1\nkind: ConfigMap\nmetadata:\n  name: preview\ndata:\n  replicas: \"{values["replicas"]}\"\n");
        var factory = CreateFactory(dockable => openedVisualization = dockable as VisualizationViewModel);
        var previewService = new HelmReleasePreviewService(helm.Object, _yamlSerializer, static () => new VisualizationViewModel(), factory.Object);
        var editor = new HelmReleaseEditorViewModel(
            helm.Object, _yamlSerializer,
            Mock.Of<IFactory>(),
            Mock.Of<IHelmListNotifier>(),
            previewService,
            Mock.Of<HanumanInstitute.MvvmDialogs.IDialogService>());
        editor.Initialize(cluster, new HelmConnection("config", "context", "apps"), null);
        await editor.InitializationTask.WaitAsync(TestContext.Current.CancellationToken);
        editor.ReleaseName = "web";
        editor.SelectedRepository = "stable";
        editor.SelectedChart = "web";
        editor.SelectedVersion = "1.0";
        await TestWait.UntilAsync(
            () => editor.ValuesYaml.Contains("replicas: 2", StringComparison.Ordinal),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        var view = new HelmReleaseEditorView { DataContext = editor };
        using (var window = Application.Current.CreateTestWindow(content: view))
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            var textEditor = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(view)
                .OfType<TextEditor>().Single();
            textEditor.Text = "replicas: 4\nserver:\n  enabled: false\n";
            await TestApplicationExtensions.WaitForUiAsync();
            await editor.PreviewCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
            editor.ValuesYaml.ShouldBe("replicas: 4\nserver:\n  enabled: false\n");
            var visualization = openedVisualization.ShouldNotBeNull();
            await TestWait.UntilAsync(
                () => visualization.Graph?.Resources.Count == 1,
                TimeSpan.FromSeconds(5),
                cancellationToken: TestContext.Current.CancellationToken);
            visualization.Graph!.Resources.OfType<V1ConfigMap>().Single().Data!["replicas"].ShouldBe("4");
            window.Close();
        }

        var returnedView = new HelmReleaseEditorView { DataContext = editor };
        using var returnedWindow = Application.Current.CreateTestWindow(content: returnedView);
        try
        {
            returnedWindow.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(returnedView)
                .OfType<TextEditor>().Single().Text.ShouldBe("replicas: 4\nserver:\n  enabled: false\n");
        }
        finally
        {
            returnedWindow.Close();
            openedVisualization?.Dispose();
        }
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
        var sut = new HelmReleasePreviewService(helmService.Object, _yamlSerializer, static () => new VisualizationViewModel(), factory.Object);

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
