using KubeUI.Avalonia.Resources.Workloads.v1.Pod;
using KubeUI.Kubernetes;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources.Workloads.v1.Pod;

/// <summary>
/// Covers the shared Prometheus history index that recycled Pod metric cells reuse.
/// </summary>
public sealed class PodPrometheusHistoryIndexTests
{
    [Fact]
    public async Task GetHistoryAsync_reuses_the_index_for_an_equivalent_result()
    {
        var index = new PodPrometheusHistoryIndex();
        var result = CreateResult();

        var first = await index.GetHistoryAsync(result, "default", "pod-1", TestContext.Current.CancellationToken);
        var second = await index.GetHistoryAsync(CreateResult(), "default", "pod-1", TestContext.Current.CancellationToken);

        first.Cpu.ShouldBe(second.Cpu);
        first.Memory.ShouldBe(second.Memory);
        // The cache is keyed by the content of the result, so the second result reuses the index.
        index.IndexBuildCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetHistoryAsync_builds_a_new_index_for_a_different_result()
    {
        var index = new PodPrometheusHistoryIndex();

        _ = await index.GetHistoryAsync(CreateResult(), "default", "pod-1", TestContext.Current.CancellationToken);
        _ = await index.GetHistoryAsync(CreateResult(value: 5), "default", "pod-1", TestContext.Current.CancellationToken);

        index.IndexBuildCount.ShouldBe(2);
    }

    [Fact]
    public async Task GetHistoryAsync_sums_the_shared_and_pod_specific_values()
    {
        var index = new PodPrometheusHistoryIndex();
        var result = new MetricResultSet
        {
            Metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal)
            {
                ["cpuUsage"] =
                [
                    new MetricSeries
                    {
                        Name = "cpuUsage",
                        Labels = new Dictionary<string, string>(StringComparer.Ordinal) { ["pod"] = "pod-1", ["namespace"] = "default" },
                        Points = [new MetricPoint(DateTimeOffset.UnixEpoch, 2)],
                    },
                    new MetricSeries
                    {
                        Name = "cpuUsage",
                        Labels = new Dictionary<string, string>(StringComparer.Ordinal),
                        Points = [new MetricPoint(DateTimeOffset.UnixEpoch, 3)],
                    },
                ],
            },
        };

        var history = await index.GetHistoryAsync(result, "default", "pod-1", TestContext.Current.CancellationToken);

        history.Cpu.ShouldHaveSingleItem();
        history.Cpu[0].Value.ShouldBe(5);
    }

    [Fact]
    public async Task GetHistoryAsync_returns_empty_history_for_an_unknown_pod()
    {
        var index = new PodPrometheusHistoryIndex();

        var history = await index.GetHistoryAsync(CreateResult(), "default", "pod-unknown", TestContext.Current.CancellationToken);

        history.Cpu.ShouldBeEmpty();
        history.Memory.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetHistoryAsync_ignores_series_that_only_carry_one_of_the_pod_labels()
    {
        var index = new PodPrometheusHistoryIndex();
        var result = new MetricResultSet
        {
            Metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal)
            {
                ["cpuUsage"] =
                [
                    new MetricSeries
                    {
                        Name = "cpuUsage",
                        Labels = new Dictionary<string, string>(StringComparer.Ordinal) { ["pod"] = "pod-1" },
                        Points = [new MetricPoint(DateTimeOffset.UnixEpoch, 2)],
                    },
                ],
            },
        };

        var history = await index.GetHistoryAsync(result, "default", "pod-1", TestContext.Current.CancellationToken);

        history.Cpu.ShouldBeEmpty();
    }

    private static MetricResultSet CreateResult(double value = 1) => new()
    {
        Metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal)
        {
            ["cpuUsage"] =
            [
                new MetricSeries
                {
                    Name = "cpuUsage",
                    Labels = new Dictionary<string, string>(StringComparer.Ordinal) { ["pod"] = "pod-1", ["namespace"] = "default" },
                    Points = [new MetricPoint(DateTimeOffset.UnixEpoch, value)],
                },
            ],
            ["memoryUsage"] =
            [
                new MetricSeries
                {
                    Name = "memoryUsage",
                    Labels = new Dictionary<string, string>(StringComparer.Ordinal) { ["pod"] = "pod-1", ["namespace"] = "default" },
                    Points = [new MetricPoint(DateTimeOffset.UnixEpoch, value)],
                },
            ],
        },
    };
}
