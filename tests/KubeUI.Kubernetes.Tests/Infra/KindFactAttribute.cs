using Xunit;

namespace KubeUI.Kubernetes.Tests.Infra;

public sealed class KindFactAttribute : FactAttribute
{
    private const string EnvironmentVariableName = "KUBEUI_RUN_KIND_TESTS";

    public KindFactAttribute()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable(EnvironmentVariableName), "1", StringComparison.Ordinal))
        {
            Skip = $"Set {EnvironmentVariableName}=1 to run kind-backed tests.";
        }
    }
}
