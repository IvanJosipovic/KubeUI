namespace KubeUI.Avalonia.Services.FeatureFlags;

/// <summary>Standard reasons describing how a feature flag value was resolved.</summary>
public static class FeatureReason
{
    /// <summary>The provider returned a statically configured value.</summary>
    public const string Static = "STATIC";

    /// <summary>The caller-provided default was returned.</summary>
    public const string Default = "DEFAULT";

    /// <summary>The value could not be resolved.</summary>
    public const string Error = "ERROR";
}
