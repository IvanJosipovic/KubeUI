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

    /// <summary>Creates the Helm navigation section.</summary>
    public HelmNavigationSectionProvider()
    {
        Section = new NavigationSection(
            "helm",
            HelmLanguageManager.Instance.HelmNavigationSectionProvider_SectionTitle.CurrentValue()!,
            NavigationSectionOrder.AfterCustomResources,
            null,
            IconAssetPath,
            IconDarkAssetPath);

        Documents =
        [
            new NavigationDocument(
                ChartsDocumentId,
                HelmLanguageManager.Instance.HelmChartsView_Title.CurrentValue()!,
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
                HelmLanguageManager.Instance.HelmRepositoriesView_Title.CurrentValue()!,
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
                HelmLanguageManager.Instance.HelmReleasesView_Title.CurrentValue()!,
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

    private static bool IsAvailable(ClusterWorkspace cluster) => cluster.Runtime.Connected;
}
