using Avalonia.Svg.Skia;

namespace KubeUI.Avalonia.Infrastructure;

internal static class ApplicationIcons
{
    internal const string ControlPlaneIconPath = "avares://KubeUI.Avalonia/Assets/kube/infrastructure_components/unlabeled/control-plane.svg";

    private static readonly Lazy<SvgSource> s_controlPlaneSource = new(
        () => SvgSource.Load(ControlPlaneIconPath)
            ?? throw new InvalidOperationException($"Unable to load application icon '{ControlPlaneIconPath}'."));

    public static SvgImage CreateControlPlaneImage() => new()
    {
        Source = s_controlPlaneSource.Value,
    };
}
