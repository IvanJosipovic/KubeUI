namespace KubeUI.Avalonia.Services.FeatureFlags;

/// <summary>Classifies a feature flag evaluation failure.</summary>
public enum FeatureErrorType
{
    /// <summary>No error occurred.</summary>
    None,

    /// <summary>The provider is not ready to evaluate flags.</summary>
    ProviderNotReady,

    /// <summary>The requested flag does not exist.</summary>
    FlagNotFound,

    /// <summary>The flag value does not match the requested type.</summary>
    TypeMismatch,

    /// <summary>The flag value could not be parsed.</summary>
    ParseError,

    /// <summary>An unexpected evaluation error occurred.</summary>
    General
}
