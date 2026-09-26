using FluentIcons.Common;
using KubeUI.Avalonia.Shell.Navigation;

namespace KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;

internal static class CrossplaneNavigationFeature
{
    public const string FeatureId = "crossplane-mr-diff-detection";

    public static INavigationFeatureDefinition CreateDefinition() =>
        new NavigationFeatureDefinition<MRDiffDetectionViewModel>(
            FeatureId,
            Assets.Resources.MRDiffDetectionView_Title!,
            [Assets.Resources.MRDiffDetectionView_NavigationGroup!],
            NavigationFeaturePlacement.Root,
            100,
            Icon.DataUsage,
            static context => MRDiffDetectionViewModel.IsAvailable(context.Cluster));
}
