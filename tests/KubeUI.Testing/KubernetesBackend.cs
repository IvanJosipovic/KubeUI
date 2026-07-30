namespace KubeUI.Testing;

public enum KubernetesBackend
{
    Fake,
    Kind,
}

public static class KubernetesBackendData
{
    public static IEnumerable<object[]> Enabled =>
        string.Equals(Environment.GetEnvironmentVariable("KUBEUI_RUN_KIND_TESTS"), "1", StringComparison.Ordinal)
            ? [new object[] { KubernetesBackend.Fake }, new object[] { KubernetesBackend.Kind }]
            : [new object[] { KubernetesBackend.Fake }];
}
