using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Helm.Charts;
using KubeUI.Features.Helm.Releases;
using KubeUI.Features.Helm.Repositories;
using KubeUI.Features.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Features.Helm;

/// <summary>Contributes Helm charts, repositories, and releases to cluster navigation.</summary>
public sealed class HelmNavigationSectionProvider : INavigationSectionProvider
{
    public const string RepositoriesDocumentId = "helm-repositories";
    public const string ReleasesDocumentId = "helm-releases";
    public const string ChartsDocumentId = "helm-charts";

    /// <summary>Asset path of the official CNCF Helm color icon, used in the light theme (see Assets/helm/README.md for licensing).</summary>
    public const string IconAssetPath = "avares://KubeUI.Features.Helm/Assets/helm/helm-icon-color.svg";

    /// <summary>Asset path of the official CNCF Helm white icon, used in the dark theme.</summary>
    public const string IconDarkAssetPath = "avares://KubeUI.Features.Helm/Assets/helm/helm-icon-white.svg";

    private readonly bool _enabled;

    /// <summary>Creates the Helm navigation section.</summary>
    /// <param name="enabled">Whether Helm is enabled by application configuration.</param>
    public HelmNavigationSectionProvider(bool enabled)
    {
        _enabled = enabled;
        Section = new NavigationSection(
            "helm",
            Assets.Resources.NavigationView_Helm!,
            NavigationSectionOrder.AfterCustomResources,
            null,
            IconAssetPath,
            IconDarkAssetPath);

        Documents =
        [
            new NavigationDocument(
                ChartsDocumentId,
                Assets.Resources.HelmChartsView_Title!,
                150,
                null,
                IsAvailable,
                static (services, cluster) =>
                {
                    var viewModel = services.GetRequiredService<HelmChartsViewModel>();
                    viewModel.Initialize(cluster);
                    return viewModel;
                }),
            new NavigationDocument(
                RepositoriesDocumentId,
                Assets.Resources.HelmRepositoriesView_Title!,
                200,
                null,
                IsAvailable,
                static (services, cluster) =>
                {
                    var viewModel = services.GetRequiredService<HelmRepositoriesViewModel>();
                    viewModel.Initialize(cluster);
                    return viewModel;
                }),
            new NavigationDocument(
                ReleasesDocumentId,
                Assets.Resources.HelmReleasesView_Title!,
                100,
                null,
                IsAvailable,
                static (services, cluster) =>
                {
                    var viewModel = services.GetRequiredService<HelmReleasesViewModel>();
                    viewModel.Initialize(cluster);
                    return viewModel;
                })
        ];
    }

    public NavigationSection Section { get; }

    public IReadOnlyList<NavigationDocument> Documents { get; }

    public NavigationPlacement? Place(IResourceConfig resource) => null;

    private bool IsAvailable(ClusterWorkspace cluster) => _enabled && cluster.Runtime.Connected;
}
