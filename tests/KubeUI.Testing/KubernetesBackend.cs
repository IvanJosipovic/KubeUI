using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace KubeUI.Testing;

public enum KubernetesBackend
{
    Fake,
    Kind,
}

public static class KubernetesBackendData
{
    public static bool RunKindTests => string.Equals(Environment.GetEnvironmentVariable("KUBEUI_RUN_KIND_TESTS"), "1", StringComparison.Ordinal);
}

public sealed class KubernetesBackendDataAttribute : DataAttribute
{
    public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker)
    {
        ArgumentNullException.ThrowIfNull(testMethod);

        List<ITheoryDataRow> rows = [CreateRow(testMethod.Name, KubernetesBackend.Fake)];

        if (KubernetesBackendData.RunKindTests)
            rows.Add(CreateRow(testMethod.Name, KubernetesBackend.Kind));

        return new ValueTask<IReadOnlyCollection<ITheoryDataRow>>(rows);
    }

    public override bool SupportsDiscoveryEnumeration() => true;

    private static TheoryDataRow<KubernetesBackend> CreateRow(string testName, KubernetesBackend backend) =>
        new(backend)
        {
            TestDisplayName = $"{testName} - {GetBackendSuffix(backend)}"
        };

    private static string GetBackendSuffix(KubernetesBackend backend) => backend switch
    {
        KubernetesBackend.Fake => "fake",
        KubernetesBackend.Kind => "kind",
        _ => throw new ArgumentOutOfRangeException(nameof(backend), backend, "Unknown Kubernetes backend")
    };
}
