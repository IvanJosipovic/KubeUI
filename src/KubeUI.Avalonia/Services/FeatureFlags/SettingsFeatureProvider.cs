using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;

namespace KubeUI.Avalonia.Services.FeatureFlags;

/// <summary>Resolves feature flags from persisted application settings.</summary>
public sealed class SettingsFeatureProvider : IFeatureProvider
{
    private readonly ISettingsService _settingsService;

    /// <summary>Creates a settings-backed provider.</summary>
    /// <param name="settingsService">Application settings service.</param>
    public SettingsFeatureProvider(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <inheritdoc />
    public FeatureProviderMetadata Metadata { get; } = new("settings");

    /// <inheritdoc />
    public ResolutionDetails<bool> ResolveBooleanValue(
        string flagKey,
        bool defaultValue,
        EvaluationContext? context = null)
    {
        // The settings-backed provider resolves from persisted FeatureSettings only; targeting keys and
        // evaluation attributes are intentionally not used (they are no-ops for the file-backed store).
        _ = context;
        var features = _settingsService.Settings.PreviewFeatures ?? new();

        return flagKey switch
        {
            FeatureFlagKeys.Helm => Resolve(flagKey, features.Helm),
            FeatureFlagKeys.Hubble => Resolve(flagKey, features.Hubble),
            FeatureFlagKeys.DisableJsonSerializationReflection => Resolve(flagKey, features.DisableJsonSerializationReflection),
            FeatureFlagKeys.DisableYamlSerializationReflection => Resolve(flagKey, features.DisableYamlSerializationReflection),
            _ => new ResolutionDetails<bool>(
                flagKey,
                defaultValue,
                FeatureErrorType.FlagNotFound,
                FeatureReason.Error,
                ErrorMessage: $"Feature flag '{flagKey}' is not defined.")
        };
    }

    private static ResolutionDetails<bool> Resolve(string flagKey, bool value)
        => new(
            flagKey,
            value,
            FeatureErrorType.None,
            FeatureReason.Static,
            value ? "on" : "off");
}
