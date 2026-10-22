using Avalonia.Headless.XUnit;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Kubernetes;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Helm;

public sealed class LiveHelmVisualizationTests
{
    [AvaloniaFact(Explicit = true)]
    public async Task Alpha10_lists_releases_across_namespaces_and_reads_release_metadata()
    {
        var context = Environment.GetEnvironmentVariable("KUBEUI_LIVE_CONTEXT")
            ?? throw new InvalidOperationException("Set KUBEUI_LIVE_CONTEXT to the kubeconfig context containing Authentik.");
        using var cluster = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = KubernetesBackend.Live;
            config.Name = context;
        });
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(90));
        var service = Application.Current.GetRequiredTestService<IHelmService>();
        var connection = new HelmConnection(cluster.Runtime.KubeConfigPath, context, "default");
        var releases = await service.ListReleasesAsync(connection, allNamespaces: true, cancellationToken: timeout.Token);
        var authentik = releases.Single(release => release.Name == "authentik" && release.Namespace == "authentik");
        authentik.ChartName.ShouldNotBeEmpty();
        authentik.ChartVersion.ShouldNotBeEmpty();
        TestContext.Current.TestOutputHelper!.WriteLine(
            $"All-namespace releases: {releases.Count}; Authentik chart: {authentik.ChartName} {authentik.ChartVersion}");
    }

    [AvaloniaFact(Explicit = true)]
    public async Task Alpha10_reads_stored_authentik_manifest()
    {
        var context = Environment.GetEnvironmentVariable("KUBEUI_LIVE_CONTEXT")
            ?? throw new InvalidOperationException("Set KUBEUI_LIVE_CONTEXT to the kubeconfig context containing Authentik.");
        using var cluster = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = KubernetesBackend.Live;
            config.Name = context;
        });
        var service = Application.Current.GetRequiredTestService<IHelmService>();
        var connection = new HelmConnection(cluster.Runtime.KubeConfigPath, context, "authentik");
        (await service.GetManifestAsync(connection, "authentik", cancellationToken: TestContext.Current.CancellationToken))
            .ShouldNotBeEmpty();
    }

    [AvaloniaFact(Explicit = true)]
    public async Task Authentik_live_release_visualization_loads_deployments()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(90));
        var context = Environment.GetEnvironmentVariable("KUBEUI_LIVE_CONTEXT")
            ?? throw new InvalidOperationException("Set KUBEUI_LIVE_CONTEXT to the kubeconfig context containing Authentik.");
        using var cluster = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = KubernetesBackend.Live;
            config.Name = context;
        });
        var deployments = await cluster.Runtime.Client!.AppsV1.ListNamespacedDeploymentAsync(
            "authentik", cancellationToken: timeout.Token);
        var expected = deployments.Items.Where(item =>
            item.Metadata.Annotations.TryGetValue("meta.helm.sh/release-name", out var name) && name == "authentik"
            && item.Metadata.Annotations.TryGetValue("meta.helm.sh/release-namespace", out var ns) && ns == "authentik")
            .Select(item => item.Name()).ToArray();
        expected.ShouldNotBeEmpty();
        using var viewModel = new VisualizationViewModel();
        var service = new HelmReleasePreviewService(
            Mock.Of<IHelmService>(), () => viewModel, Mock.Of<IHelmVisualizationDocumentOpener>());
        service.VisualizeRelease(cluster, "authentik", "authentik");
        using var window = Application.Current.CreateTestWindow(content: new VisualizationView { DataContext = viewModel });
        try
        {
            window.Show();
            try
            {
                await TestWait.UntilAsync(
                    () => expected.All(name => viewModel.Graph!.Resources.Any(item => item.Kind == "Deployment" && item.Name() == name)),
                    TimeSpan.FromSeconds(45), cancellationToken: timeout.Token);
            }
            catch (TimeoutException)
            {
                Assert.Fail($"Graph={viewModel.Graph!.Resources.Count}; error={viewModel.ErrorMessage}; "
                    + $"types={string.Join(',', viewModel.ResourceTypes)}; seeded={cluster.Runtime.Objects.Count}; "
                    + $"cachedDeployments={cluster.Runtime.GetResourceList<V1Deployment>().Count}; "
                    + $"namespaces={string.Join(',', viewModel.SelectedNamespaces.Select(item => item.Name()))}");
            }

            viewModel.IsNamespaceSelectionEnabled.ShouldBeFalse();
            viewModel.IsNamespaceSelectionLinked.ShouldBeFalse();
            viewModel.SelectedNamespaces.Select(item => item.Name()).ShouldBe(["authentik"]);
            viewModel.Error.ShouldBeNull();
            TestContext.Current.TestOutputHelper!.WriteLine(
                $"Live Authentik graph: {viewModel.Graph!.Resources.Count} resources; "
                + $"{viewModel.Graph.Relationships.Count} relationships; deployments={string.Join(',', expected)}");
        }
        finally
        {
            window.Close();
        }
    }
}
