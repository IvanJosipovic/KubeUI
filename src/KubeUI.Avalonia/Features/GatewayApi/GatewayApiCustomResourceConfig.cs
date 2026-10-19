using FluentIcons.Common;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal abstract class GatewayApiCustomResourceConfig : CustomResourceConfig
{
    protected abstract string NavigationSectionId { get; }

    public override NavigationPlacement Navigation => NavigationPlacement.InSection(
        NavigationSectionId,
        IsExperimental
            ?
            [
                new NavigationGroup("gateway", Assets.Resources.NavigationView_GatewayApi!, 0, null),
                new NavigationGroup("experimental", Assets.Resources.NavigationView_GatewayApiExperimental!, int.MaxValue, Icon.Beaker),
            ]
            : [new NavigationGroup("gateway", Assets.Resources.NavigationView_GatewayApi!, 0, null)]);

    private bool IsExperimental =>
        string.Equals(ApiGroup, "gateway.networking.x-k8s.io", StringComparison.Ordinal)
        || (Definition.Metadata?.Annotations?.TryGetValue("gateway.networking.k8s.io/channel", out var channel) == true
            && string.Equals(channel, "experimental", StringComparison.Ordinal));
}
