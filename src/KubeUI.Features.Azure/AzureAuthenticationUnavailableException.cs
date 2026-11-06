namespace KubeUI.Features.Azure;

/// <summary>Indicates that Azure credentials are unavailable for an Azure operation.</summary>
public sealed class AzureAuthenticationUnavailableException : Exception
{
    /// <summary>Initializes the exception with the underlying credential failure.</summary>
    /// <param name="innerException">The Azure credential failure.</param>
    public AzureAuthenticationUnavailableException(Exception innerException)
        : base(innerException.Message, innerException)
    {
    }
}
