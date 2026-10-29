namespace KubeUI.Avalonia.Services.FeatureFlags;

/// <summary>Resolves feature flag values from a backing source.</summary>
public interface IFeatureProvider
{
    /// <summary>Gets provider metadata.</summary>
    FeatureProviderMetadata Metadata { get; }

    /// <summary>Resolves a boolean feature flag.</summary>
    /// <param name="flagKey">Flag key to resolve.</param>
    /// <param name="defaultValue">Value returned when the flag cannot be resolved.</param>
    /// <param name="context">Optional evaluation context.</param>
    /// <returns>The evaluation result.</returns>
    ResolutionDetails<bool> ResolveBooleanValue(
        string flagKey,
        bool defaultValue,
        EvaluationContext? context = null);
}
