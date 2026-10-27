namespace KubeUI.Avalonia.Services.FeatureFlags;

/// <summary>Describes the subject and attributes used to evaluate feature flags.</summary>
/// <param name="TargetingKey">Optional stable subject identifier.</param>
/// <param name="Attributes">Optional evaluation attributes.</param>
public sealed record EvaluationContext(
    string? TargetingKey = null,
    IReadOnlyDictionary<string, object>? Attributes = null);
