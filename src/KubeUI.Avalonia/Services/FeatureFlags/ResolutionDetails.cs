namespace KubeUI.Avalonia.Services.FeatureFlags;

/// <summary>Describes the result of one feature flag evaluation.</summary>
/// <typeparam name="T">Flag value type.</typeparam>
/// <param name="FlagKey">Evaluated flag key.</param>
/// <param name="Value">Resolved or fallback value.</param>
/// <param name="ErrorType">Evaluation error classification.</param>
/// <param name="Reason">Reason describing how the value was resolved.</param>
/// <param name="Variant">Optional semantic identifier for the value.</param>
/// <param name="ErrorMessage">Optional error detail.</param>
public sealed record ResolutionDetails<T>(
    string FlagKey,
    T Value,
    FeatureErrorType ErrorType = FeatureErrorType.None,
    string? Reason = null,
    string? Variant = null,
    string? ErrorMessage = null);
