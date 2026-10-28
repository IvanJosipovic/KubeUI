namespace KubeUI.Avalonia.Services.FeatureFlags;

/// <summary>Stable flag keys used to evaluate KubeUI application features.</summary>
public static class FeatureFlagKeys
{
    /// <summary>The Helm integration flag key.</summary>
    public const string Helm = "kubeui.helm";

    /// <summary>The Hubble integration flag key.</summary>
    public const string Hubble = "kubeui.hubble";

    /// <summary>The Kubernetes JSON reflection flag key.</summary>
    public const string JsonAllowReflection = "kubeui.json-allow-reflection";

    /// <summary>The Kubernetes YAML reflection flag key.</summary>
    public const string YamlAllowReflection = "kubeui.yaml-allow-reflection";
}
