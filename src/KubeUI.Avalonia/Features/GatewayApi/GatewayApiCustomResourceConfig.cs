using FluentIcons.Common;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;

namespace KubeUI.Avalonia.Features.GatewayApi;

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

    /// <inheritdoc />
    public override NavigationPlacement Navigation
    {
        get
        {
            if (ApiGroup == ExperimentalGroup)
            {
                return NavigationPlacement.InSection(
                    NavigationSections.NetworkId,
                    [
                        new NavigationGroup("gateway", Assets.Resources.NavigationView_GatewayApi!, 0, null),
                        new NavigationGroup("experimental", Assets.Resources.NavigationView_GatewayApiExperimental!, int.MaxValue, Icon.Beaker),
                    ]);
            }

            return NavigationPlacement.InSection(
                NavigationSections.NetworkId,
                [new NavigationGroup("gateway", Assets.Resources.NavigationView_GatewayApi!, 0, null)]);
        }
    }
}
