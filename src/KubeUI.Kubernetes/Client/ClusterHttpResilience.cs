using Microsoft.Extensions.Http.Resilience;
using Polly.Retry;

namespace KubeUI.Kubernetes;

/// <summary>
/// Builds the HTTP retry options used by the <see cref="Cluster"/> client pipeline.
/// </summary>
internal static class ClusterHttpResilience
{
    private const string WatchQueryParameter = "watch=true";

    // Keep the package's transient-failure definition (408, 429, 5xx, HttpRequestException,
    // TimeoutRejectedException) instead of duplicating it here.
    private static readonly Func<RetryPredicateArguments<HttpResponseMessage>, ValueTask<bool>> s_transientShouldHandle =
        new HttpRetryStrategyOptions().ShouldHandle;

    internal static HttpRetryStrategyOptions CreateRetryOptions() => new()
    {
        MaxRetryAttempts = 3,
        ShouldHandle = static args => IsWatchOperation(args.Context.OperationKey)
            ? ValueTask.FromResult(false)
            : s_transientShouldHandle(args),
    };

    /// <summary>
    /// Watches are long-lived streams whose reconnects are owned by the informer.
    /// Retrying them in the HTTP pipeline duplicates streams and can abandon stream reads.
    /// </summary>
    private static bool IsWatchOperation(string? operationKey)
        => operationKey is not null
           && (operationKey.Contains("?" + WatchQueryParameter, StringComparison.OrdinalIgnoreCase)
               || operationKey.Contains("&" + WatchQueryParameter, StringComparison.OrdinalIgnoreCase));
}
