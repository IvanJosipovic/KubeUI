using KubeUI.Kubernetes;

namespace KubeUI.Documentation.Tests.Infra;

public sealed class WalkthroughPrometheusQueryClientTests
{
    [Theory]
    [InlineData("cpuUsage")]
    [InlineData("cpuRequests")]
    [InlineData("cpuLimits")]
    [InlineData("memoryUsage")]
    [InlineData("memoryRequests")]
    [InlineData("memoryLimits")]
    [InlineData("networkReceive")]
    [InlineData("networkTransmit")]
    [InlineData("fsUsage")]
    [InlineData("fsReads")]
    [InlineData("fsWrites")]
    public void Featured_pod_queries_return_deterministic_range_values(string metric)
    {
        var client = new WalkthroughPrometheusQueryClient();
        var start = DateTimeOffset.Parse("2026-09-29T11:00:00Z");
        var options = new Dictionary<string, string>
        {
            ["namespace"] = "default",
            ["pods"] = WalkthroughDemoResources.FeaturedPodName,
            ["selector"] = "pod, namespace",
        };
        var query = new ExternalPrometheusProvider().BuildQuery(MetricCategory.Pods, metric, options);

        var result = client.Query(query, start, start.AddHours(1), 60);
        var series = Assert.Single(result.Data.Result);

        Assert.Equal("success", result.Status);
        Assert.Equal("matrix", result.Data.ResultType);
        Assert.Equal(WalkthroughDemoResources.FeaturedPodName, series.Metric["pod"]);
        Assert.Equal("default", series.Metric["namespace"]);
        Assert.Equal(61, series.Values.Count);
        Assert.Equal(TimeSpan.FromHours(1), series.Values[^1].Timestamp - series.Values[0].Timestamp);
        Assert.All(series.Values, point => Assert.True(point.Value > 0, $"{metric} must have positive values throughout the hour."));
        for (var index = 0; index < series.Values.Count; index++)
        {
            var point = series.Values[index];
            Assert.Equal(start.AddMinutes(index), point.Timestamp);
            Assert.Equal(
                WalkthroughPrometheusQueryClient.SampleValue(
                    metric,
                    WalkthroughDemoResources.FeaturedPodName,
                    (int)(point.Timestamp.ToUnixTimeSeconds() / 60)),
                point.Value);
        }
    }

    [Theory]
    [InlineData("cpuUsage")]
    [InlineData("memoryUsage")]
    public void Every_pod_in_the_list_has_an_hour_of_timestamped_usage(string metric)
    {
        var client = new WalkthroughPrometheusQueryClient();
        var start = DateTimeOffset.Parse("2026-09-29T11:00:00Z");
        var query = new ExternalPrometheusProvider().BuildQuery(
            MetricCategory.Pods, metric,
            new Dictionary<string, string>
            {
                ["namespace"] = "default",
                ["pods"] = ".*",
                ["selector"] = "pod, namespace",
            });

        var result = client.Query(query, start, start.AddHours(1), 60);
        var expectedPods = KubeUIWalkthroughRecorder.CreateDemoResources().Resources
            .OfType<k8s.Models.V1Pod>()
            .Select(pod => pod.Metadata.Name)
            .OrderBy(name => name, StringComparer.Ordinal);

        Assert.Equal(expectedPods, result.Data.Result.Select(series => series.Metric["pod"]));
        foreach (var series in result.Data.Result)
        {
            Assert.Equal(61, series.Values.Count);
            Assert.Equal(TimeSpan.FromHours(1), series.Values[^1].Timestamp - series.Values[0].Timestamp);
            Assert.All(series.Values, point => Assert.True(point.Value > 0, $"{metric} for {series.Metric["pod"]} must be populated."));
        }
    }

    [Fact]
    public void Pod_selector_excludes_nonmatching_pods()
    {
        var client = new WalkthroughPrometheusQueryClient();
        var start = DateTimeOffset.Parse("2026-09-29T11:00:00Z");
        var query = new ExternalPrometheusProvider().BuildQuery(
            MetricCategory.Pods, "cpuUsage",
            new Dictionary<string, string>
            {
                ["namespace"] = "default",
                ["pods"] = "web-02-.*",
            });

        var result = client.Query(query, start, start.AddMinutes(1), 60);

        Assert.NotEmpty(result.Data.Result);
        Assert.All(result.Data.Result, series => Assert.StartsWith("web-02-", series.Metric["pod"]));
        Assert.All(result.Data.Result, series => Assert.Equal(2, series.Values.Count));
    }

    [Fact]
    public void Unknown_prometheus_query_fails_explicitly()
    {
        var client = new WalkthroughPrometheusQueryClient();
        var now = DateTimeOffset.Parse("2026-09-29T11:00:00Z");

        Assert.Throws<InvalidOperationException>(() => client.Query("unsupported query", now, now, 60));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            client.Query("unsupported query", now.AddMinutes(1), now, 60));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            client.Query("unsupported query", now, now, 0));
        Assert.Throws<InvalidOperationException>(() =>
            client.Query("container_cpu_usage_seconds_total", now, now, 60));
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        Assert.Throws<OperationCanceledException>(() =>
            client.Query("unsupported query", now, now, 60, cancelled.Token));
    }

    [Fact]
    public void Featured_pod_has_distinct_positive_usage_requests_and_limits()
    {
        var name = WalkthroughDemoResources.FeaturedPodName;
        Assert.Equal(0.18, WalkthroughPrometheusQueryClient.SampleValue("cpuUsage", name, 0));
        Assert.Equal(0.35, WalkthroughPrometheusQueryClient.SampleValue("cpuRequests", name, 0));
        Assert.Equal(0.65, WalkthroughPrometheusQueryClient.SampleValue("cpuLimits", name, 0));
        Assert.Equal(96 * 1024d * 1024d, WalkthroughPrometheusQueryClient.SampleValue("memoryUsage", name, 0));
        Assert.Equal(160 * 1024d * 1024d, WalkthroughPrometheusQueryClient.SampleValue("memoryRequests", name, 0));
        Assert.Equal(320 * 1024d * 1024d, WalkthroughPrometheusQueryClient.SampleValue("memoryLimits", name, 0));
        Assert.True(WalkthroughPrometheusQueryClient.SampleValue("cpuUsage", name, 1) > 0.18);
        Assert.True(WalkthroughPrometheusQueryClient.SampleValue("memoryUsage", name, 1) > 96 * 1024d * 1024d);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            WalkthroughPrometheusQueryClient.SampleValue("unknown", name, 0));
    }
}
