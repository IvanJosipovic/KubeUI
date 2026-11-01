using KubeUI.Avalonia.Features.Resources.Metrics;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Kubernetes;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Metrics;

/// <summary>
/// Covers the pure metric helpers that shape the history bars and the Prometheus queries.
/// </summary>
public sealed class MetricsHistoryBucketsTests
{
    [Fact]
    public void CreateBars_aligns_the_window_to_five_minute_boundaries()
    {
        var end = new DateTimeOffset(2024, 1, 1, 12, 7, 30, TimeSpan.Zero);
        var points = new[]
        {
            new MetricPoint(new DateTimeOffset(2024, 1, 1, 12, 5, 0, TimeSpan.Zero), 3),
        };

        var bars = MetricsHistoryBuckets.CreateBars(points, end, limit: null, chartHeight: 100);

        bars.Count.ShouldBe(MetricsHistoryBuckets.BucketCount);
        // The exclusive end of the window is the next five-minute boundary after the requested end.
        bars[^1].End.ShouldBe(new DateTimeOffset(2024, 1, 1, 12, 10, 0, TimeSpan.Zero));
        bars[0].Start.ShouldBe(bars[^1].End - MetricsHistoryBuckets.History);
        bars.Select(bar => bar.Start).ShouldBe(bars.Select(bar => bar.Start).Order());
        bars.ShouldAllBe(bar => bar.End - bar.Start == MetricsHistoryBuckets.BucketSize);
    }

    [Fact]
    public void CreateBars_keeps_the_highest_value_of_each_bucket()
    {
        // The window is the hour that ends at the aligned end, so 13:00 makes 12:00 the first bucket.
        var bucketStart = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var points = new[]
        {
            new MetricPoint(bucketStart.AddSeconds(10), 1),
            new MetricPoint(bucketStart.AddSeconds(20), 7),
            new MetricPoint(bucketStart.AddSeconds(30), 3),
        };

        var bars = MetricsHistoryBuckets.CreateBars(points, bucketStart.AddHours(1), limit: null, chartHeight: 100);

        bars[0].Value.ShouldBe(7);
    }

    [Fact]
    public void CreateBars_ignores_samples_outside_the_window()
    {
        var windowEnd = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var points = new[]
        {
            new MetricPoint(windowEnd.AddMinutes(-65), 5),
            new MetricPoint(windowEnd, 9),
        };

        var bars = MetricsHistoryBuckets.CreateBars(points, windowEnd, limit: null, chartHeight: 100);

        bars.ShouldAllBe(bar => bar.Value == 0);
    }

    [Fact]
    public void CreateBars_reports_the_limit_state_for_each_bucket()
    {
        // The window is the hour that ends at the aligned end, so 13:00 makes 12:00 the first bucket.
        var bucketStart = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var points = new[]
        {
            new MetricPoint(bucketStart.AddSeconds(10), 50),
            new MetricPoint(bucketStart.AddMinutes(5).AddSeconds(10), 85),
            new MetricPoint(bucketStart.AddMinutes(10).AddSeconds(10), 100),
        };

        var bars = MetricsHistoryBuckets.CreateBars(points, bucketStart.AddHours(1), limit: 100, chartHeight: 100);

        bars[0].LimitState.ShouldBe(MetricsLimitState.Normal);
        bars[1].LimitState.ShouldBe(MetricsLimitState.Warning);
        bars[2].LimitState.ShouldBe(MetricsLimitState.Exceeded);
    }

    [Fact]
    public void CreateBars_marks_buckets_without_samples_as_normal()
    {
        // The window is the hour that ends at the aligned end, so 13:00 makes 12:00 the first bucket.
        var bucketStart = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var points = new[]
        {
            new MetricPoint(bucketStart.AddSeconds(10), 100),
        };

        var bars = MetricsHistoryBuckets.CreateBars(points, bucketStart.AddHours(1), limit: 100, chartHeight: 100);

        bars[1].LimitState.ShouldBe(MetricsLimitState.Normal);
        bars[1].Value.ShouldBe(0);
        bars[1].Height.ShouldBe(0);
    }
}

/// <summary>
/// Covers the Prometheus label selector translation of Kubernetes label selectors.
/// </summary>
public sealed class ResourceMetricsCatalogSelectorTests
{
    [Fact]
    public void BuildPrometheusLabelSelector_translates_match_labels()
    {
        var selector = new k8s.Models.V1LabelSelector
        {
            MatchLabels = new Dictionary<string, string>(StringComparer.Ordinal) { ["app"] = "myapp" },
        };

        var result = ResourceMetricsCatalog.BuildPrometheusLabelSelector(selector);

        result.ShouldBe("label_app=\"myapp\"");
    }

    [Fact]
    public void BuildPrometheusLabelSelector_returns_empty_for_a_null_selector()
    {
        ResourceMetricsCatalog.BuildPrometheusLabelSelector(null).ShouldBeEmpty();
    }

    [Fact]
    public void BuildPrometheusLabelSelector_returns_empty_when_match_labels_are_missing()
    {
        var selector = new k8s.Models.V1LabelSelector();

        ResourceMetricsCatalog.BuildPrometheusLabelSelector(selector).ShouldBeEmpty();
    }

    [Fact]
    public void BuildPrometheusLabelSelector_returns_empty_for_set_based_expressions()
    {
        var selector = new k8s.Models.V1LabelSelector
        {
            MatchExpressions =
            [
                new k8s.Models.V1LabelSelectorRequirement
                {
                    Key = "app",
                    OperatorProperty = "In",
                    Values = ["myapp"],
                },
            ],
        };

        ResourceMetricsCatalog.BuildPrometheusLabelSelector(selector).ShouldBeEmpty();
    }

    [Fact]
    public void BuildPrometheusLabelSelector_returns_empty_for_an_invalid_label_name()
    {
        var selector = new k8s.Models.V1LabelSelector
        {
            MatchLabels = new Dictionary<string, string>(StringComparer.Ordinal) { ["not-a-valid-name"] = "value" },
        };

        ResourceMetricsCatalog.BuildPrometheusLabelSelector(selector).ShouldBeEmpty();
    }
}
