using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public sealed class PrometheusProviderQueryTests
{
    [Fact]
    public void BuildNodeQuery_escapes_address_for_prometheus_string_literal()
    {
        var provider = new ExternalPrometheusProvider();
        IReadOnlyDictionary<string, string> options = new Dictionary<string, string>
        {
            ["instance"] = "10\\.0\\.0\\.1",
            ["node"] = "node-1",
        };

        var query = provider.BuildQuery(MetricCategory.Nodes, "cpuUsage", options);

        query.ShouldContain("instance=~\"(10\\\\.0\\\\.0\\\\.1)(:[0-9]+)?\"");
        query.ShouldContain(") by (instance)");
        query.ShouldNotContain(") by (node)");
    }

    [Fact]
    public void BuildPodQuery_escapes_regex_backslashes_for_prometheus_string_literal()
    {
        var provider = new ExternalPrometheusProvider();
        IReadOnlyDictionary<string, string> options = new Dictionary<string, string>
        {
            ["pods"] = @"apicurio\.registry",
            ["namespace"] = "default",
            ["selector"] = "pod, namespace",
        };

        var query = provider.BuildQuery(MetricCategory.Pods, "cpuUsage", options);

        query.ShouldContain("pod=~\"apicurio\\\\.registry\"");
    }

    [Fact]
    public async Task OperatorProvider_propagates_cancellation_while_resolving_services()
    {
        var provider = new OperatorPrometheusProvider();
        using var client = new k8s.Kubernetes(new k8s.KubernetesClientConfiguration { Host = "http://localhost" });
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => provider.TryResolveServiceAsync(
            client,
            new ClusterMetricsSettings(),
            cancellation.Token));
    }

    [Theory]
    [InlineData("bytesSentSuccess", "^2[0-9]*")]
    [InlineData("bytesSentFailure", "^5[0-9]*")]
    public void BuildIngressQuery_uses_valid_RE2_digit_class(string queryName, string expectedStatusPattern)
    {
        var provider = new ExternalPrometheusProvider();
        IReadOnlyDictionary<string, string> options = new Dictionary<string, string>
        {
            ["ingress"] = "sample-ingress",
            ["namespace"] = "default",
        };

        var query = provider.BuildQuery(MetricCategory.Ingress, queryName, options);

        query.ShouldContain($"status=~\"{expectedStatusPattern}\"");
        query.ShouldNotContain("\\\\d");
    }
}
