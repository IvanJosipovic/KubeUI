using System.Runtime.CompilerServices;
using Avalonia.Headless.XUnit;
using Xunit.v3;

namespace KubeUI.Avalonia.Tests.Infra;

/// <summary>
/// Marks a test that only runs against a live cluster, identified by an environment variable.
/// </summary>
/// <remarks>
/// The test is skipped rather than marked explicit because an explicit test is reported as
/// <c>NotRunnable</c> in the TRX file, which the CI test reporter counts as a failure. A skipped
/// test is reported as <c>NotExecuted</c> and is counted as skipped instead.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer(typeof(AvaloniaFactDiscoverer))]
public sealed class LiveClusterFactAttribute : FactAttribute
{
    /// <summary>Environment variable holding the kubeconfig context that the test connects to.</summary>
    public const string ContextVariable = "KUBEUI_LIVE_CONTEXT";

    /// <summary>
    /// Initializes a live-cluster test.
    /// </summary>
    /// <param name="environmentVariable">Environment variable that must be set to a non-empty value.</param>
    /// <param name="sourceFilePath">This parameter is provided automatically by the compiler.</param>
    /// <param name="sourceLineNumber">This parameter is provided automatically by the compiler.</param>
    public LiveClusterFactAttribute(
        string environmentVariable = ContextVariable,
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(environmentVariable)))
        {
            Skip = $"Set {environmentVariable} to the kubeconfig context to run this live-cluster test.";
        }
    }
}
