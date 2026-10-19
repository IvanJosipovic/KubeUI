using FluentIcons.Common;
using KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;

namespace KubeUI.Avalonia.Features.Crossplane;

internal sealed class CrossplaneNavigationSectionProvider : INavigationSectionProvider
{
    public const string FeatureId = "crossplane-mr-diff-detection";

    public CrossplaneNavigationSectionProvider()
    {
        Section = new NavigationSection(
            "crossplane",
            Assets.Resources.NavigationView_Crossplane!,
            NavigationSections.AfterCustomResourcesOrder,
            null);

        Documents =
        [
            new NavigationDocument<MRDiffDetectionViewModel>(
                FeatureId,
                Assets.Resources.MRDiffDetectionView_Title!,
                100,
                Icon.DataUsage,
                static context => MRDiffDetectionViewModel.IsAvailable(context.Cluster))
        ];
    }

    public NavigationSection Section { get; }

    public IReadOnlyList<INavigationDocument> Documents { get; }

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
            NavigationTreeEditor.ConstructCustomResourceGroups(group));
    }
}
