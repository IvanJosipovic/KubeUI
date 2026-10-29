namespace KubeUI.Avalonia.Services.FeatureFlags;

/// <summary>Stable flag keys used to evaluate KubeUI application features.</summary>
public static class FeatureFlagKeys
{
    /// <summary>The Helm integration flag key.</summary>
    public const string Helm = "kubeui.helm";

    /// <summary>The Hubble integration flag key.</summary>
    public const string Hubble = "kubeui.hubble";

    /// <summary>The Kubernetes JSON serialization disable-reflection flag key.</summary>
    public const string DisableJsonSerializationReflection = "kubeui.disable-json-serialization-reflection";

    /// <summary>The Kubernetes YAML serialization disable-reflection flag key.</summary>
    public const string DisableYamlSerializationReflection = "kubeui.disable-yaml-serialization-reflection";
}
