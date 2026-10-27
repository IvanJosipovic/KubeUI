namespace KubeUI.Avalonia.Options;

/// <summary>Persisted opt-in state for preview features.</summary>
public sealed partial class FeatureSettings : ObservableObject
{
    /// <summary>Gets or sets whether the Helm integration is enabled.</summary>
    [ObservableProperty]
    public partial bool Helm { get; set; }

    /// <summary>Gets or sets whether the Hubble integration is enabled.</summary>
    [ObservableProperty]
    public partial bool Hubble { get; set; }
}
