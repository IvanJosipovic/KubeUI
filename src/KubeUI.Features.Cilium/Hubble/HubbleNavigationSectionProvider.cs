using KubeUI.Features.Cilium.Hubble;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Features.Cilium.Hubble;

/// <summary>Contributes Hubble views and Cilium resource groups to cluster navigation.</summary>
public sealed class HubbleNavigationSectionProvider : INavigationSectionProvider
{
    public const string FlowsDocumentId = "hubble-flows";
    public const string ServiceMapDocumentId = "hubble-service-map";
    public const string CiliumIconAssetPath = "avares://KubeUI.Features.Cilium/Assets/cilium/cilium-light.svg";
    public const string CiliumIconDarkAssetPath = "avares://KubeUI.Features.Cilium/Assets/cilium/cilium-dark.svg";
    public const string HubbleIconAssetPath = "avares://KubeUI.Features.Cilium/Assets/cilium/hubble-light.svg";
    public const string HubbleIconDarkAssetPath = "avares://KubeUI.Features.Cilium/Assets/cilium/hubble-dark.svg";

    private readonly bool _enabled;

    /// <summary>Creates the Hubble navigation section.</summary>
    /// <param name="enabled">Whether Hubble is enabled by application configuration.</param>
    public HubbleNavigationSectionProvider(bool enabled)
    {
        _enabled = enabled;
        Section = new NavigationSection(
            "cilium",
            L10n.Instance.HubbleNavigationSectionProvider_SectionTitle.CurrentValue()!,
            NavigationSectionOrder.AfterCustomResources,
            null,
            CiliumIconAssetPath,
            CiliumIconDarkAssetPath);

        Documents =
        [
            new NavigationDocument(
                FlowsDocumentId,
                L10n.Instance.HubbleNavigationSectionProvider_Flows_Title.CurrentValue()!,
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
                L10n.Instance.HubbleNavigationSectionProvider_ServiceMap_Title.CurrentValue()!,
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
        [new NavigationGroup("hubble", L10n.Instance.HubbleNavigationSectionProvider_GroupName.CurrentValue()!, 0, null, HubbleIconAssetPath, HubbleIconDarkAssetPath)]);

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
            NavigationGroupPath.CreateCustomResourceGroups(group));
    }

    private bool IsAvailable(ClusterWorkspace cluster)
        => _enabled && cluster.Runtime.Connected;
}
