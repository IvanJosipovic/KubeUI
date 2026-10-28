using KubeUI.Avalonia.Features.Cilium.Hubble;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Services.FeatureFlags;
using KubeUI.Avalonia.Shell.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Avalonia.Features.Cilium.Hubble;

internal sealed class HubbleNavigationSectionProvider : INavigationSectionProvider
{
    public const string FlowsDocumentId = "hubble-flows";
    public const string ServiceMapDocumentId = "hubble-service-map";
    public const string CiliumIconAssetPath = "/Assets/cilium/cilium-light.svg";
    public const string CiliumIconDarkAssetPath = "/Assets/cilium/cilium-dark.svg";
    public const string HubbleIconAssetPath = "/Assets/cilium/hubble-light.svg";
    public const string HubbleIconDarkAssetPath = "/Assets/cilium/hubble-dark.svg";

    private readonly bool _enabled;

    public HubbleNavigationSectionProvider(IFeatureClient featureClient)
    {
        _enabled = featureClient.GetBooleanValue(FeatureFlagKeys.Hubble, false);
        Section = new NavigationSection(
            NavigationSections.CiliumId,
            Assets.Resources.NavigationView_Cilium!,
            NavigationSections.AfterCustomResourcesOrder,
            null,
            CiliumIconAssetPath,
            CiliumIconDarkAssetPath);

        Documents =
        [
            new NavigationDocument(
                FlowsDocumentId,
                Assets.Resources.Hubble_Flows_Title!,
                100,
                null,
                IsAvailable,
                static (services, cluster) =>
                {
                    var viewModel = services.GetRequiredService<FlowListViewModel>();
                    viewModel.Initialize(cluster);
                    return viewModel;
                }),
            new NavigationDocument(
                ServiceMapDocumentId,
                Assets.Resources.Hubble_ServiceMap_Title!,
                200,
                null,
                IsAvailable,
                static (services, cluster) =>
                {
                    var viewModel = services.GetRequiredService<ServiceMapViewModel>();
                    viewModel.Initialize(cluster);
                    return viewModel;
                })
        ];
    }

    public NavigationSection Section { get; }

    public NavigationPlacement DocumentPlacement => NavigationPlacement.InSection(
        Section.Id,
        [new NavigationGroup("hubble", Assets.Resources.Hubble_GroupName!, 0, null, HubbleIconAssetPath, HubbleIconDarkAssetPath)]);

    public IReadOnlyList<NavigationDocument> Documents { get; }

    public NavigationPlacement? Place(IResourceConfig resource)
    {
        var group = resource.Kind.Group ?? string.Empty;
        if (!string.Equals(group, "cilium.io", StringComparison.Ordinal)
            && !group.EndsWith(".cilium.io", StringComparison.Ordinal))
        {
            return null;
        }

        return NavigationPlacement.InSection(
            Section.Id,
            NavigationTreeEditor.ConstructCustomResourceGroups(group));
    }

    private bool IsAvailable(ClusterWorkspace cluster)
        => _enabled && cluster.Runtime.Connected;
}
