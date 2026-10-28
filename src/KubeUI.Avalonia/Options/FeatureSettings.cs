namespace KubeUI.Avalonia.Options;

/// <summary>Persisted application feature flags.</summary>
public sealed partial class FeatureSettings : ObservableObject
{
    /// <summary>Gets or sets whether the Helm integration is enabled.</summary>
    [ObservableProperty]
    public partial bool Helm { get; set; }

    /// <summary>Gets or sets whether the Hubble integration is enabled.</summary>
    [ObservableProperty]
    public partial bool Hubble { get; set; }

    /// <summary>Gets or sets whether Kubernetes JSON serialization may use reflection.</summary>
    [ObservableProperty]
    public partial bool JsonAllowReflection { get; set; }

    /// <summary>Gets or sets whether Kubernetes YAML serialization may use reflection.</summary>
    [ObservableProperty]
    public partial bool YamlAllowReflection { get; set; }
}
