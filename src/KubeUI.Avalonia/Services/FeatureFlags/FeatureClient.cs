namespace KubeUI.Avalonia.Services.FeatureFlags;

/// <summary>Default feature flag client that delegates to a provider and fails closed to caller defaults.</summary>
public sealed class FeatureClient : IFeatureClient
{
    private readonly IFeatureProvider _provider;
    private readonly ILogger<FeatureClient> _logger;

    /// <summary>Creates a client over the supplied provider.</summary>
    /// <param name="provider">Provider that resolves flag values.</param>
    /// <param name="logger">Client logger.</param>
    public FeatureClient(IFeatureProvider provider, ILogger<FeatureClient> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool GetBooleanValue(
        string flagKey,
        bool defaultValue,
        EvaluationContext? context = null)
        => GetBooleanDetails(flagKey, defaultValue, context).Value;

    /// <inheritdoc />
    public ResolutionDetails<bool> GetBooleanDetails(
        string flagKey,
        bool defaultValue,
        EvaluationContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(flagKey))
        {
            _logger.LogWarning("Feature flag key must not be empty; returning default {DefaultValue}", defaultValue);
            return new ResolutionDetails<bool>(
                flagKey,
                defaultValue,
                FeatureErrorType.FlagNotFound,
                FeatureReason.Error,
                ErrorMessage: "Flag key must not be empty.");
        }

        try
        {
            return _provider.ResolveBooleanValue(flagKey, defaultValue, context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to evaluate feature flag {FlagKey}", flagKey);
            return new ResolutionDetails<bool>(
                flagKey,
                defaultValue,
                FeatureErrorType.General,
                FeatureReason.Error,
                ErrorMessage: exception.Message);
        }
    }
}
