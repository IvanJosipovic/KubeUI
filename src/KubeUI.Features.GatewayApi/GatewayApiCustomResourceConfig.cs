using FluentIcons.Common;
using KubeUI.Features.Resources;

namespace KubeUI.Features.GatewayApi;

/// <summary>
/// Shared base for Gateway API custom resources. Every Gateway API kind is placed under
/// Network, in the Gateway group, and the experimental API group adds the Experimental group.
/// </summary>
internal abstract class GatewayApiCustomResourceConfig : CustomResourceConfig
{
    /// <summary>Gets the stable Gateway API group.</summary>
    public const string StableGroup = "gateway.networking.k8s.io";

    /// <summary>Gets the experimental Gateway API group.</summary>
    public const string ExperimentalGroup = "gateway.networking.x-k8s.io";

    private NavigationPlacement? _cachedNavigation;

    /// <inheritdoc />
    public override NavigationPlacement Navigation
    {
        get
        {
            if (_cachedNavigation is not null)
            {
                return _cachedNavigation;
            }

            if (ApiGroup == ExperimentalGroup)
            {
                _cachedNavigation = NavigationPlacement.InSection(
                    NavigationSectionIds.Network,
                    [
                        new NavigationGroup("gateway", GatewayApiLanguageManager.Instance.GatewayApiCustomResourceConfig_GatewayGroup.CurrentValue()!, 0, null),
                        new NavigationGroup("experimental", GatewayApiLanguageManager.Instance.GatewayApiCustomResourceConfig_ExperimentalGroup.CurrentValue()!, int.MaxValue, Icon.Beaker),
                    ]);
            }
            else
            {
                _cachedNavigation = NavigationPlacement.InSection(
                    NavigationSectionIds.Network,
                    [new NavigationGroup("gateway", GatewayApiLanguageManager.Instance.GatewayApiCustomResourceConfig_GatewayGroup.CurrentValue()!, 0, null)]);
            }

            return _cachedNavigation;
        }
    }
}
