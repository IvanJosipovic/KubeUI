using FluentIcons.Common;
using k8s.Models;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Crossplane.MRDiffDetection;
using KubeUI.Features.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Features.Crossplane;

internal sealed class CrossplaneNavigationSectionProvider : INavigationSectionProvider
{
    public const string FeatureId = "crossplane-mr-diff-detection";
    public const string IconAssetPath = "avares://KubeUI.Features.Crossplane/Assets/crossplane/crossplane-light.svg";
    public const string IconDarkAssetPath = "avares://KubeUI.Features.Crossplane/Assets/crossplane/crossplane-dark.svg";

    public CrossplaneNavigationSectionProvider()
    {
        Section = new NavigationSection(
            "crossplane",
            CrossplaneLanguageManager.Instance.CrossplaneNavigationSectionProvider_SectionTitle.CurrentValue()!,
            NavigationSectionOrder.AfterCustomResources,
            null,
            IconAssetPath,
            IconDarkAssetPath);

        Documents =
        [
            new NavigationDocument(
                FeatureId,
                CrossplaneLanguageManager.Instance.MRDiffDetectionView_Title.CurrentValue()!,
                100,
                Icon.DataUsage,
                MRDiffDetectionViewModel.IsAvailable,
                static (services, cluster) =>
                {
                    var viewModel = services.GetRequiredService<MRDiffDetectionViewModel>();
                    viewModel.Initialize(cluster);
                    return viewModel;
                })
        ];
    }

    public NavigationSection Section { get; }

    public bool IsSectionAvailable(ClusterWorkspace cluster)
    {
        try
        {
            return cluster.Runtime.GetResourceList<V1CustomResourceDefinition>()
                .Any(definition => string.Equals(definition.Spec?.Group, "crossplane.io", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    public IReadOnlyList<NavigationDocument> Documents { get; }

    public NavigationPlacement? Place(IResourceConfig resource)
    {
        var group = resource.Kind.Group ?? string.Empty;
        if (!string.Equals(group, "crossplane.io", StringComparison.Ordinal)
            && !group.EndsWith(".crossplane.io", StringComparison.Ordinal))
        {
            return null;
        }

        return NavigationPlacement.InSection(
            Section.Id,
            NavigationGroupPath.CreateCustomResourceGroups(group));
    }
}
