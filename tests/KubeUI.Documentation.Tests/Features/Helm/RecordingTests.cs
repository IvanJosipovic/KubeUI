using Avalonia.VisualTree;
using Dock.Model.Controls;
using KubeUI.Avalonia.Features.Helm.Charts;
using KubeUI.Avalonia.Features.Helm.Releases;
using KubeUI.Avalonia.Features.Helm.Repositories;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Documentation.Tests.Infra;

namespace KubeUI.Documentation.Tests.Features.Helm;

public sealed class RecordingTests
{
    [DocumentationVideoTheory]
    [Obsolete]
    public Task Manage_prometheus_with_helm_video(string theme)
    {
        var clusterName = $"prometheus-{theme}";
        var repositoryName = HelmWalkthroughService.GetRepositoryName();
        var chartName = HelmWalkthroughService.GetChartName();

        return KubeUIWalkthrough.Create(theme, "configure-helm-release")
            .Intro(
                "Native Helm integration",
                "KubeUI has full native Helm integration. You can manage repositories, browse charts and versions, preview rendered resources, and install or upgrade releases.")
            .FakeCluster(clusterName)
            .LoadView<MainView, MainViewModel>(x => x.ViewModel.Initialize(), connectToCluster: false)
            .Speak("KubeUI is open. I'll connect to my Kubernetes cluster and open Helm repositories.")
            .OpenCluster(clusterName)
            .SelectNavigation("Helm")
            .SelectNavigation("Repositories")
            .Speak(
                "Repositories is where you add, edit, and remove chart sources. This cluster has the official Prometheus Community repository configured; I'll browse its charts.",
                root => root is MainView main
                    && main.ViewModel.Factory.GetDockable<IDocumentDock>("Documents")?.ActiveDockable is HelmRepositoriesViewModel,
                root => root is MainView main
                    ? $"Active document: {main.ViewModel.Factory.GetDockable<IDocumentDock>("Documents")?.ActiveDockable?.GetType().Name ?? "none"}; visible docs: {string.Join(", ", main.ViewModel.Factory.GetDockable<IDocumentDock>("Documents")?.VisibleDockables?.Select(item => item.GetType().Name) ?? [])}"
                    : $"Root: {root.GetType().Name}")
            .RightClickHelmRepository(repositoryName)
            .SelectHelmContextMenuItem(
                "View Charts",
                root => root.GetVisualDescendants().OfType<HelmChartsView>().Any(view => view.IsVisible))
            .SearchHelmCharts(chartName)
            .Speak(
                "The chart catalog comes from the live Prometheus Community index. I'll open its popular prometheus chart and use the published defaults.",
                root => root.GetVisualDescendants().OfType<HelmChartsView>().Any(view => view.IsVisible))
            .RightClickHelmChart(chartName)
            .SelectHelmContextMenuItem(
                "Install chart",
                root => root.GetVisualDescendants().OfType<HelmReleaseEditorView>().Any(view => view.IsVisible))
            .Speak(
                "The install form carries the real repository, chart, current version, and published values forward. I'll name this release prometheus.",
                root => root.GetVisualDescendants().OfType<HelmReleaseEditorView>().Any(view =>
                    view.IsVisible
                    && view.ViewModel.SelectedVersion is not null
                    && view.ViewModel.Chart == "prometheus-community/prometheus"
                    && view.ViewModel.ValuesYaml.Contains("replicaCount: 1", StringComparison.Ordinal)))
            .HoverHelmEditorField("ChartVersion")
            .SetHelmReleaseName("prometheus")
            .Speak("Before installing, I'll preview resources rendered from the actual Prometheus chart.")
            .PreviewHelmRelease(root => root.GetVisualDescendants().OfType<VisualizationView>().Any(view =>
                view.IsVisible
                && view.ViewModel.IsPreview
                && view.ViewModel.Graph is { Resources.Count: >= 2 }))
            .Speak("The preview shows what this chart will create with its current values. I'll return to the form and install Prometheus.")
            .ReturnToHelmReleaseEditor()
            .SaveHelmRelease()
            .Speak("Prometheus is installed. I'll open Releases and upgrade it with a values change.")
            .SelectNavigation("Releases")
            .Speak(
                "The Prometheus release appears here with its status and namespace. I'll open its upgrade form.",
                root => root.GetVisualDescendants().OfType<HelmReleasesView>().Any(view =>
                    view.IsVisible && view.ViewModel.ItemCount == 1))
            .RightClickHelmRelease("prometheus")
            .SelectHelmContextMenuItem(
                "Upgrade",
                root => root.GetVisualDescendants().OfType<HelmReleaseEditorView>().Any(view =>
                    view.IsVisible && view.ViewModel.IsUpgrade))
            .Speak(
                "Upgrade starts with the chart's saved values. I'll increase the Prometheus server replica count from one to two.",
                root => root.GetVisualDescendants().OfType<HelmReleaseEditorView>().Any(view =>
                    view.IsVisible
                    && view.ViewModel.IsUpgrade
                    && view.ViewModel.ValuesYaml.Contains("replicaCount: 1", StringComparison.Ordinal)))
            .ClickHelmValuesEditor()
            .ReplaceHelmReplicaValue("2")
            .Speak("The values editor contains valid YAML with replicas set to two. I'll save the upgrade.")
            .SaveHelmRelease()
            .Speak("I'll confirm the upgraded release is back in the list.")
            .SelectHelmRelease("prometheus", root => root.GetVisualDescendants().OfType<HelmReleasesView>().Any(view =>
                view.IsVisible && view.ViewModel.SelectedRelease?.Revision == 2))
            .Speak(
                "Upgrade is complete. Prometheus has the updated replica count.",
                root => root.GetVisualDescendants().OfType<HelmReleasesView>().Any(view =>
                    view.IsVisible && view.ViewModel.SelectedRelease?.Revision == 2))
            .RecordAsync();
    }
}
