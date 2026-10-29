using KubeUI.Avalonia.Services.FeatureFlags;

namespace KubeUI.Avalonia.Tests.Infra;

internal sealed class TestFeatureClient : IFeatureClient
{
    private readonly Dictionary<string, bool> _values = new(StringComparer.Ordinal);

    public TestFeatureClient(bool helm = false, bool hubble = false)
    {
        _values[FeatureFlagKeys.Helm] = helm;
        _values[FeatureFlagKeys.Hubble] = hubble;
        _values[FeatureFlagKeys.DisableJsonSerializationReflection] = false;
        _values[FeatureFlagKeys.DisableYamlSerializationReflection] = false;
    }

    public void Set(string flagKey, bool value)
    {
        _values[flagKey] = value;
    }

    public bool GetBooleanValue(
        string flagKey,
        bool defaultValue,
        EvaluationContext? context = null)
        => GetBooleanDetails(flagKey, defaultValue, context).Value;

    public ResolutionDetails<bool> GetBooleanDetails(
        string flagKey,
        bool defaultValue,
        EvaluationContext? context = null)
    {
        _ = context;
        if (_values.TryGetValue(flagKey, out var value))
        {
            return new ResolutionDetails<bool>(
                flagKey,
                value,
                FeatureErrorType.None,
                FeatureReason.Static,
                value ? "on" : "off");
        }

        return new ResolutionDetails<bool>(
            flagKey,
            defaultValue,
            FeatureErrorType.FlagNotFound,
            FeatureReason.Error);
    }
}
