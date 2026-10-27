namespace KubeUI.Avalonia.Services.FeatureFlags;

/// <summary>Stable flag keys used to evaluate KubeUI preview features.</summary>
public static class FeatureFlagKeys
{
    /// <summary>The Helm integration flag key.</summary>
    public const string Helm = "kubeui.helm";

    /// <summary>The Hubble integration flag key.</summary>
    public const string Hubble = "kubeui.hubble";
}
