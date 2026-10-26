using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Helm.Charts;
using KubeUI.Avalonia.Features.Helm.Releases;
using KubeUI.Avalonia.Features.Helm.Repositories;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Avalonia.Features.Helm;

internal sealed class HelmNavigationSectionProvider : INavigationSectionProvider
{
    public const string RepositoriesDocumentId = "helm-repositories";
    public const string ReleasesDocumentId = "helm-releases";
    public const string ChartsDocumentId = "helm-charts";

    /// <summary>Asset path of the official CNCF Helm color icon, used in the light theme (see Assets/helm/README.md for licensing).</summary>
    public const string IconAssetPath = "/Assets/helm/helm-icon-color.svg";

    /// <summary>Asset path of the official CNCF Helm white icon, used in the dark theme.</summary>
    public const string IconDarkAssetPath = "/Assets/helm/helm-icon-white.svg";

    public HelmNavigationSectionProvider()
    {
        Section = new NavigationSection(
            "helm",
            Assets.Resources.NavigationView_Helm!,
            NavigationSections.AfterCustomResourcesOrder,
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

    private static bool IsAvailable(ClusterWorkspace cluster) => cluster.Runtime.Connected;
}
