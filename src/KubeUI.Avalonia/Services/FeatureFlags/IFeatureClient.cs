namespace KubeUI.Avalonia.Services.FeatureFlags;

/// <summary>Evaluates application feature flags.</summary>
public interface IFeatureClient
{
    /// <summary>Gets a boolean feature flag value.</summary>
    /// <param name="flagKey">Flag key to resolve.</param>
    /// <param name="defaultValue">Value returned when the flag cannot be resolved.</param>
    /// <param name="context">Optional evaluation context.</param>
    /// <returns>The resolved value, or the default when evaluation fails.</returns>
    bool GetBooleanValue(
        string flagKey,
        bool defaultValue,
        EvaluationContext? context = null);

    /// <summary>Gets a boolean feature flag evaluation with resolution details.</summary>
    /// <param name="flagKey">Flag key to resolve.</param>
    /// <param name="defaultValue">Value returned when the flag cannot be resolved.</param>
    /// <param name="context">Optional evaluation context.</param>
    /// <returns>The evaluation result.</returns>
    ResolutionDetails<bool> GetBooleanDetails(
        string flagKey,
        bool defaultValue,
        EvaluationContext? context = null);
}
