using Avalonia.Headless.XUnit;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Text.Json;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Avalonia.Resources;
using NodeResourceConfig = KubeUI.Avalonia.Resources.Core.v1.Node.V1NodeConfig;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using PodCpuHistoryCell = KubeUI.Avalonia.Resources.Workloads.v1.Pod.MetricsHistoryCPUCellView;
using PodMemoryHistoryCell = KubeUI.Avalonia.Resources.Workloads.v1.Pod.MetricsHistoryMemoryCellView;
using NodeCpuHistoryCell = KubeUI.Avalonia.Resources.Core.v1.Node.MetricsHistoryCPUCellView;
using NodeMemoryHistoryCell = KubeUI.Avalonia.Resources.Core.v1.Node.MetricsHistoryMemoryCellView;

namespace KubeUI.Avalonia.Tests.Resources.Workloads.v1.Pod;

public sealed class PodMetricCellTests
{
    [Fact]
    public void history_buckets_keep_peak_values_and_classify_warning_and_limit_samples()
    {
        var end = DateTimeOffset.Parse("2026-09-23T12:00:00Z");
        var start = end - MetricsHistoryBuckets.History;
        MetricPoint[] points =
        [
            new(start.AddMinutes(4), 0.79),
            new(start.AddMinutes(4).AddSeconds(30), 0.8),
            new(start.AddMinutes(5), 1.0),
            new(end, 5.0),
        ];

        var bars = MetricsHistoryBuckets.CreateBars(points, end, 1.0);

        bars.Count.ShouldBe(MetricsHistoryBuckets.BucketCount);
        bars[0].Value.ShouldBe(0.8);
        bars[0].LimitState.ShouldBe(MetricsLimitState.Warning);
        bars[1].Value.ShouldBe(1.0);
        bars[1].LimitState.ShouldBe(MetricsLimitState.Exceeded);
        bars[^1].Value.ShouldBe(0);
        bars[^1].Height.ShouldBe(0);

        MetricsHistoryBuckets.CreateBars(points, end, null)
            .ShouldAllBe(static bar => bar.LimitState == MetricsLimitState.Normal);
    }

    [AvaloniaFact]
    public async Task cpu_cell_renders_metric_history_as_bars_instead_of_a_text_value()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        var pod = CreatePod();
        var now = DateTime.UtcNow.AddMinutes(-2);
        fixture.UseMetricsServerSamples(
            CreatePodMetricSample(pod, now.AddMinutes(-5), "100m"),
            CreatePodMetricSample(pod, now.AddMinutes(-1), "450m"));
        var cell = fixture.CreateCpuCell(pod);
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        Dispatcher.UIThread.RunJobs();

        MetricBars(cell).ShouldNotBeEmpty();
        MetricBars(cell).Any(bar => IsThemeBrush(bar.Background, "PodStatusWarningBrush")
            && ToolTip.GetTip(bar) is { } tip && tip.ToString()!.Contains("90%", StringComparison.Ordinal)).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task history_bars_fill_the_cell_width_without_a_trailing_gap()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        var pod = CreatePod();
        fixture.UseMetricsServerSamples(CreatePodMetricSample(pod, DateTime.UtcNow, "100m"));
        var cell = fixture.CreateCpuCell(pod);
        cell.Width = 80;
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        Dispatcher.UIThread.RunJobs();

        var panel = cell.GetVisualDescendants().OfType<Grid>().Single();
        var bars = MetricBars(cell);
        double.IsNaN(panel.Height).ShouldBeTrue();
        bars.ShouldNotBeEmpty();
        cell.Margin.ShouldBe(new Thickness(4, 0));
        (bars[^1].Bounds.Right / panel.Bounds.Width).ShouldBeGreaterThan(0.95);
    }

    [AvaloniaFact]
    public async Task cpu_cell_renders_async_prometheus_value()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateMetricResult("cpuUsage", 1.234),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var pod = CreatePod();
        var cell = fixture.CreateCpuCell(pod);
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);

        await TestWait.UntilAsync(
            () => MetricBars(cell).Length > 0,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        MetricBars(cell).Any(bar => ToolTip.GetTip(bar) is { } tip && tip.ToString()!.Contains("1.23c", StringComparison.Ordinal)).ShouldBeTrue();
        queryClient.Queries.ShouldBe(2);
    }

    [AvaloniaFact]
    public async Task node_prometheus_history_matches_instance_label_when_node_label_is_missing()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateNodeMetricResultsWithInstanceLabel("10.0.0.1:9100", 0.5, 512d * 1024 * 1024),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var node = CreateNode("node-a");
        var cpuCell = fixture.CreateNodeCpuCell(node);
        var memoryCell = fixture.CreateNodeMemoryCell(node);
        using var window = Application.Current.CreateTestWindow(content: new StackPanel { Children = { cpuCell, memoryCell } });

        window.Show();
        cpuCell.Initialize(fixture.Workspace);
        memoryCell.Initialize(fixture.Workspace);

        await TestWait.UntilAsync(
            () => queryClient.Queries == 2,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        MetricBars(cpuCell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("0.5c", StringComparison.Ordinal) == true).ShouldBeTrue();
        MetricBars(memoryCell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("512", StringComparison.Ordinal) == true).ShouldBeTrue();
        queryClient.QueryTexts.ShouldContain(query =>
            query.Contains("node_cpu_seconds_total", StringComparison.Ordinal)
            && query.Contains("instance=~\"(node-a|10\\\\.0\\\\.0\\\\.1)(:[0-9]+)?\"", StringComparison.Ordinal));
    }

    [AvaloniaFact]
    public async Task prometheus_pod_name_regex_is_escaped_for_promql_string()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateMetricResult("cpuUsage", 1.234),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var cell = fixture.CreateCpuCell(CreatePod("apicurio.registry"));
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);

        await TestWait.UntilAsync(
            () => queryClient.QueryTexts.Count == 2,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        queryClient.QueryTexts.ShouldAllBe(query =>
            query.Contains("pod=~\"apicurio\\\\.registry\"", StringComparison.Ordinal));
    }

    private static MetricResultSet CreateNodeMetricResultsWithInstanceLabel(string instance, double cpu, double memory)
    {
        var labels = new Dictionary<string, string>(StringComparer.Ordinal) { ["instance"] = instance };
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-2);
        return new MetricResultSet
        {
            Metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal)
            {
                ["cpuUsage"] = [new MetricSeries { Name = "cpuUsage", Labels = labels, Points = [new MetricPoint(timestamp, cpu)] }],
                ["memoryUsage"] = [new MetricSeries { Name = "memoryUsage", Labels = labels, Points = [new MetricPoint(timestamp, memory)] }],
            },
        };
    }

    [AvaloniaFact]
    public async Task cpu_and_memory_cells_share_one_combined_prometheus_history_request()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateMetricResults(("cpuUsage", 0.2), ("memoryUsage", 128 * 1024 * 1024)),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var pod = CreatePod();
        var cpuCell = fixture.CreateCpuCell(pod);
        var memoryCell = fixture.CreateMemoryCell(pod);
        var content = new StackPanel
        {
            Children = { cpuCell, memoryCell },
        };
        using var window = Application.Current.CreateTestWindow(content: content);

        window.Show();
        cpuCell.Initialize(fixture.Workspace);
        memoryCell.Initialize(fixture.Workspace);

        await TestWait.UntilAsync(
            () => MetricBars(cpuCell).Length > 0 && MetricBars(memoryCell).Length > 0,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        queryClient.Queries.ShouldBe(2);
    }

    [AvaloniaFact]
    public async Task memory_cell_renders_async_prometheus_value()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateMetricResult("memoryUsage", 1_048_576),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var cell = fixture.CreateMemoryCell(CreatePod());
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);

        await TestWait.UntilAsync(
            () => MetricBars(cell).Length > 0,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        MetricBars(cell).Any(bar => ToolTip.GetTip(bar) is { } tip && tip.ToString()!.Contains("1 MB", StringComparison.Ordinal)).ShouldBeTrue();
        queryClient.Queries.ShouldBe(2);
    }

    [AvaloniaFact]
    public async Task cpu_cell_uses_newest_metrics_server_sample_when_history_is_retained()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        var pod = CreatePod();
        fixture.UseMetricsServerSamples(
            CreatePodMetricSample(pod, DateTime.UtcNow.AddMinutes(-30), "100m"),
            CreatePodMetricSample(pod, DateTime.UtcNow, "450m"));
        var cell = fixture.CreateCpuCell(pod);
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);

        MetricBars(cell).ShouldNotBeEmpty();
        fixture.QueryClient.Queries.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task memory_cell_highlights_usage_at_the_container_limit()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        var pod = CreatePod();
        fixture.UseMetricsServerSamples(CreatePodMetricSample(pod, DateTime.UtcNow.AddMinutes(-2), "100m", "512Mi"));
        var cell = fixture.CreateMemoryCell(pod);
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);

        MetricBars(cell).Any(bar => IsThemeBrush(bar.Background, "ContainerStatusErrorBrush")
            && ToolTip.GetTip(bar) is { } tip && tip.ToString()!.Contains("100%", StringComparison.Ordinal)).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task recycled_cell_replaces_previous_pod_history()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        var firstPod = CreatePod("first-pod");
        var secondPod = CreatePod("second-pod");
        fixture.UseMetricsServerSamples(
            CreatePodMetricSample(firstPod, DateTime.UtcNow, "100m"),
            CreatePodMetricSample(secondPod, DateTime.UtcNow, "300m"));
        var cell = fixture.CreateCpuCell(firstPod);
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        MetricBars(cell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("0.1c", StringComparison.Ordinal) == true).ShouldBeTrue();

        cell.DataContext = secondPod;

        MetricBars(cell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("0.3c", StringComparison.Ordinal) == true).ShouldBeTrue();
        MetricBars(cell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("0.1c", StringComparison.Ordinal) == true).ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task unavailable_backend_clears_rendered_history()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        var pod = CreatePod();
        fixture.UseMetricsServerSamples(CreatePodMetricSample(pod, DateTime.UtcNow, "100m"));
        var cell = fixture.CreateCpuCell(pod);
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        MetricBars(cell).ShouldNotBeEmpty();

        fixture.SetBackend(ActiveMetricsBackend.None);
        cell.Initialize(fixture.Workspace);

        MetricBars(cell).ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task detaching_one_cell_cancels_its_ui_update_without_canceling_the_shared_request()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateMetricResults(("cpuUsage", 0.2), ("memoryUsage", 128 * 1024 * 1024)),
            QueryGate = gate,
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var pod = CreatePod();
        var cpuCell = fixture.CreateCpuCell(pod);
        var memoryCell = fixture.CreateMemoryCell(pod);
        var content = new StackPanel { Children = { cpuCell, memoryCell } };
        using var window = Application.Current.CreateTestWindow(content: content);

        window.Show();
        cpuCell.Initialize(fixture.Workspace);
        memoryCell.Initialize(fixture.Workspace);
        await TestWait.UntilAsync(
            () => queryClient.Queries == 2,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        content.Children.Remove(cpuCell);
        gate.SetResult();
        await TestWait.UntilAsync(
            () => MetricBars(memoryCell).Length > 0,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        MetricBars(cpuCell).ShouldBeEmpty();
        MetricBars(memoryCell).ShouldNotBeEmpty();
        queryClient.Queries.ShouldBe(2);
    }

    [AvaloniaFact]
    public async Task pod_resource_columns_use_the_history_cell_controls()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        fixture.UseMetricsServerSamples(CreatePodMetricSample(CreatePod(), DateTime.UtcNow, "100m"));
        var config = Application.Current.GetTestServices().GetRequiredService<V1PodConfig>();
        config.Initialize(fixture.Workspace);

        var columns = config.Columns();

        columns.Single(column => column.Key == "cpu").CustomControl.ShouldBe(
            typeof(PodCpuHistoryCell));
        columns.Single(column => column.Key == "memory").CustomControl.ShouldBe(
            typeof(PodMemoryHistoryCell));
    }

    [AvaloniaFact]
    public async Task node_resource_columns_use_history_controls_when_metrics_are_available()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        fixture.UseMetricsServerSamples(CreatePodMetricSample(CreatePod(), DateTime.UtcNow, "100m"));
        var config = Application.Current.GetTestServices().GetRequiredService<NodeResourceConfig>();
        config.Initialize(fixture.Workspace);

        var columns = config.Columns();

        columns.Single(column => column.Key == "cpu").CustomControl.ShouldBe(
            typeof(NodeCpuHistoryCell));
        columns.Single(column => column.Key == "memory").CustomControl.ShouldBe(
            typeof(NodeMemoryHistoryCell));
        var cpuColumn = (ResourceListColumn<V1Node, decimal>)columns.Single(column => column.Key == "cpu");
        cpuColumn.Field(CreateNode()).ShouldBe(4m);
        cpuColumn.DisplayValue(CreateNode()).ShouldBe("4c");
    }

    [AvaloniaFact]
    public async Task node_capacity_columns_remain_text_when_metrics_are_unavailable()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        fixture.SetBackend(ActiveMetricsBackend.None);
        var config = Application.Current.GetTestServices().GetRequiredService<NodeResourceConfig>();
        config.Initialize(fixture.Workspace);

        var columns = config.Columns();

        columns.Single(column => column.Key == "cpu").CustomControl.ShouldBeNull();
        columns.Single(column => column.Key == "memory").CustomControl.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task node_cells_render_metrics_server_history_against_allocatable_capacity()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        var node = CreateNode();
        fixture.UseNodeMetricsSamples(CreateNodeMetricSample(node, DateTime.UtcNow.AddMinutes(-2), "1600m", "4Gi"));
        var cpuCell = fixture.CreateNodeCpuCell(node);
        var memoryCell = fixture.CreateNodeMemoryCell(node);
        var content = new StackPanel { Children = { cpuCell, memoryCell } };
        using var window = Application.Current.CreateTestWindow(content: content);

        window.Show();
        cpuCell.Initialize(fixture.Workspace);
        memoryCell.Initialize(fixture.Workspace);

        MetricBars(cpuCell).Any(bar => IsThemeBrush(bar.Background, "PodStatusWarningBrush")
            && ToolTip.GetTip(bar)?.ToString()?.Contains("80%", StringComparison.Ordinal) == true).ShouldBeTrue();
        MetricBars(memoryCell).Any(bar => IsThemeBrush(bar.Background, "ContainerStatusErrorBrush")
            && ToolTip.GetTip(bar)?.ToString()?.Contains("100%", StringComparison.Ordinal) == true).ShouldBeTrue();
        fixture.QueryClient.Queries.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task node_prometheus_history_uses_only_the_selected_nodes_series()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateNodeMetricResults("node-a", "0.5", "512Mi", "node-b", "3", "3Gi"),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var node = CreateNode("node-a");
        var cell = fixture.CreateNodeCpuCell(node);
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);

        await TestWait.UntilAsync(
            () => MetricBars(cell).Length > 0,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        MetricBars(cell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("0.5c", StringComparison.Ordinal) == true).ShouldBeTrue();
        MetricBars(cell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("3c", StringComparison.Ordinal) == true).ShouldBeFalse();
        queryClient.Queries.ShouldBe(2);
    }

    private static Border[] MetricBars(Control cell)
        => cell.GetVisualDescendants().OfType<Border>().Where(bar => ToolTip.GetTip(bar) != null).ToArray();

    private static bool IsThemeBrush(IBrush brush, string resourceKey)
    {
        foreach (var themeVariant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            Application.Current.TryGetResource(resourceKey, themeVariant, out var resource).ShouldBeTrue();
            var expectedColor = resource switch
            {
                ISolidColorBrush expectedBrush => expectedBrush.Color,
                Color color => color,
                _ => throw new InvalidOperationException($"Theme resource '{resourceKey}' is not a solid brush or color."),
            };

            if (brush is ISolidColorBrush actualBrush && actualBrush.Color == expectedColor)
            {
                return true;
            }
        }

        return false;
    }

    private static V1Pod CreatePod(string name = "metrics-pod")
    {
        return KubernetesJson.Deserialize<V1Pod>(JsonSerializer.Serialize(new
        {
            metadata = new { name, @namespace = "default" },
            spec = new
            {
                containers = new[]
                {
                    new
                    {
                        name = "app",
                        resources = new
                        {
                            limits = new Dictionary<string, string> { ["cpu"] = "500m", ["memory"] = "512Mi" },
                        },
                    },
                },
            },
        }));
    }

    private static V1Node CreateNode(string name = "metrics-node")
    {
        return KubernetesJson.Deserialize<V1Node>(JsonSerializer.Serialize(new
        {
            metadata = new { name },
            status = new
            {
                addresses = new[] { new { type = "InternalIP", address = "10.0.0.1" } },
                capacity = new Dictionary<string, string> { ["cpu"] = "4", ["memory"] = "8Gi" },
                allocatable = new Dictionary<string, string> { ["cpu"] = "2", ["memory"] = "4Gi" },
            },
        }));
    }

    private static NodeMetrics CreateNodeMetricSample(V1Node node, DateTime timestamp, string cpu, string memory)
    {
        return KubernetesJson.Deserialize<NodeMetrics>(JsonSerializer.Serialize(new
        {
            metadata = new { name = node.Name() },
            timestamp,
            window = "30s",
            usage = new Dictionary<string, string> { ["cpu"] = cpu, ["memory"] = memory },
        }));
    }

    private static PodMetrics CreatePodMetricSample(V1Pod pod, DateTime timestamp, string cpu, string memory = "128Mi")
    {
        return KubernetesJson.Deserialize<PodMetrics>(JsonSerializer.Serialize(new
        {
            metadata = new { name = pod.Name(), @namespace = pod.Namespace() },
            timestamp,
            window = "30s",
            containers = new[]
            {
                new
                {
                    name = "app",
                    usage = new Dictionary<string, string> { ["cpu"] = cpu, ["memory"] = memory },
                },
            },
        }));
    }

    private static MetricResultSet CreateMetricResult(string queryName, double value) => new()
    {
        Metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal)
        {
            [queryName] =
            [
                new MetricSeries
                {
                    Name = queryName,
                    Points = [new MetricPoint(DateTimeOffset.UtcNow.AddMinutes(-2), value)],
                },
            ],
        },
    };

    private static MetricResultSet CreateMetricResults(params (string Name, double Value)[] values)
    {
        return new MetricResultSet
        {
            Metrics = values.ToDictionary(
                static item => item.Name,
                static item => (IReadOnlyList<MetricSeries>)
                [
                    new MetricSeries
                    {
                        Name = item.Name,
                        Points = [new MetricPoint(DateTimeOffset.UtcNow.AddMinutes(-2), item.Value)],
                    },
                ],
                StringComparer.Ordinal),
        };
    }

    private static MetricResultSet CreateNodeMetricResults(
        string firstNode,
        string firstCpu,
        string firstMemory,
        string secondNode,
        string secondCpu,
        string secondMemory)
    {
        return new MetricResultSet
        {
            Metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal)
            {
                ["cpuUsage"] =
                [
                    CreateNodeSeries("cpuUsage", firstNode, double.Parse(firstCpu, System.Globalization.CultureInfo.InvariantCulture)),
                    CreateNodeSeries("cpuUsage", secondNode, double.Parse(secondCpu, System.Globalization.CultureInfo.InvariantCulture)),
                ],
                ["memoryUsage"] =
                [
                    CreateNodeSeries("memoryUsage", firstNode, firstMemory == "512Mi" ? 512d * 1024 * 1024 : 3d * 1024 * 1024 * 1024),
                    CreateNodeSeries("memoryUsage", secondNode, secondMemory == "512Mi" ? 512d * 1024 * 1024 : 3d * 1024 * 1024 * 1024),
                ],
            },
        };
    }

    private static MetricSeries CreateNodeSeries(string name, string node, double value)
    {
        return new MetricSeries
        {
            Name = name,
            Labels = new Dictionary<string, string>(StringComparer.Ordinal) { ["node"] = node },
            Points = [new MetricPoint(DateTimeOffset.UtcNow.AddMinutes(-2), value)],
        };
    }

    private sealed class FakePrometheusQueryClient : IPrometheusQueryClient
    {
        public MetricResultSet? Result { get; init; }

        public TaskCompletionSource? QueryGate { get; init; }

        public int Queries { get; private set; }

        public System.Collections.Concurrent.ConcurrentQueue<string> QueryTexts { get; } = new();

        public Task PrepareAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public async Task<PrometheusClientQueryRangeResponse?> QueryRangeAsync(
            Cluster cluster,
            ResolvedPrometheusEndpoint endpoint,
            string query,
            DateTimeOffset start,
            DateTimeOffset end,
            int stepSeconds,
            CancellationToken cancellationToken = default)
        {
            Queries++;
            QueryTexts.Enqueue(query);
            if (QueryGate is not null)
            {
                await QueryGate.Task.WaitAsync(cancellationToken);
            }

            var name = query.Contains("container_cpu_usage_seconds_total", StringComparison.Ordinal)
                || query.Contains("node_cpu_seconds_total", StringComparison.Ordinal)
                ? "cpuUsage"
                : query.Contains("container_memory_working_set_bytes", StringComparison.Ordinal)
                    || query.Contains("node_memory_MemTotal_bytes", StringComparison.Ordinal)
                    ? "memoryUsage"
                    : string.Empty;
            var metricSeries = Result is not null && Result.Metrics.TryGetValue(name, out var foundSeries)
                ? foundSeries
                : [];
            var result = metricSeries
                .Where(static series => series.Points.Count > 0)
                .Select(static series => new PrometheusClientQueryRangeResponse.ResultObject
                {
                    Metric = new Dictionary<string, string>(series.Labels, StringComparer.Ordinal),
                    Values = series.Points.Select(static point => (point.Timestamp, point.Value)).ToList(),
                })
                .ToArray();
            return new PrometheusClientQueryRangeResponse
            {
                Status = "success",
                Data = new PrometheusClientQueryRangeResponse.DataObject
                {
                    ResultType = "matrix",
                    Result = result,
                },
            };
        }

        public Task ResetAsync() => Task.CompletedTask;
    }

    private sealed class MetricCellFixture : IAsyncDisposable
    {
        private readonly MetricsService _metricsService;
        private readonly Cluster _cluster;

        private MetricCellFixture(
            Cluster cluster,
            ClusterWorkspace workspace,
            MetricsService metricsService,
            IUiRefreshClock refreshClock,
            FakePrometheusQueryClient queryClient)
        {
            _cluster = cluster;
            Workspace = workspace;
            _metricsService = metricsService;
            RefreshClock = refreshClock;
            QueryClient = queryClient;
        }

        public ClusterWorkspace Workspace { get; }

        public IUiRefreshClock RefreshClock { get; }

        public FakePrometheusQueryClient QueryClient { get; }

        public PodCpuHistoryCell CreateCpuCell(V1Pod pod)
            => new(RefreshClock, TimeProvider.System) { DataContext = pod };

        public PodMemoryHistoryCell CreateMemoryCell(V1Pod pod)
            => new(RefreshClock, TimeProvider.System) { DataContext = pod };

        public NodeCpuHistoryCell CreateNodeCpuCell(V1Node node)
            => new(RefreshClock, TimeProvider.System) { DataContext = node };

        public NodeMemoryHistoryCell CreateNodeMemoryCell(V1Node node)
            => new(RefreshClock, TimeProvider.System) { DataContext = node };

        public void UseMetricsServerSamples(params PodMetrics[] samples)
        {
            _metricsService.ActiveMetricsBackend = ActiveMetricsBackend.KubernetesMetricsServer;
            foreach (var sample in samples)
            {
                _metricsService.PodMetrics.Add(sample);
            }
        }

        public void UseNodeMetricsSamples(params NodeMetrics[] samples)
        {
            _metricsService.ActiveMetricsBackend = ActiveMetricsBackend.KubernetesMetricsServer;
            foreach (var sample in samples)
            {
                _metricsService.NodeMetrics.Add(sample);
            }
        }

        public void SetBackend(ActiveMetricsBackend backend)
        {
            _metricsService.ActiveMetricsBackend = backend;
            _metricsService.IsMetricsAvailable = backend.Type != MetricsServiceType.None;
        }

        public static async Task<MetricCellFixture> CreateAsync(FakePrometheusQueryClient queryClient)
        {
            var settings = new MetricCellSettingsStore();
            var services = Application.Current.GetTestServices();
            var metricsService = new MetricsService(
                NullLogger<MetricsService>.Instance,
                settings,
                [new ExternalPrometheusProvider()],
                queryClient);
            var cluster = new Cluster(
                NullLogger<Cluster>.Instance,
                services.GetRequiredService<ILoggerFactory>(),
                new ClusterModelCatalog(services.GetRequiredService<KubernetesModelCatalog>()),
                settings,
                services,
                new ImmediateThreadDispatcher(),
                metricsService)
            {
                Name = "metrics-cell-cluster",
                Client = new k8s.Kubernetes(new KubernetesClientConfiguration { Host = "http://localhost" }),
            };
            settings.MetricsSettings.MetricsServiceType = MetricsServiceType.Prometheus;
            settings.MetricsSettings.PrometheusProviderKind = PrometheusProviderKind.External;
            settings.MetricsSettings.PrometheusDirectUrl = "http://prometheus.example";
            await metricsService.InitializeAsync(cluster);
            var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(services, cluster);
            return new MetricCellFixture(
                cluster,
                workspace,
                metricsService,
                services.GetRequiredService<IUiRefreshClock>(),
                queryClient);
        }

        public async ValueTask DisposeAsync()
        {
            Workspace.Dispose();
            await _cluster.DisposeAsync();
            _metricsService.Dispose();
        }
    }

    private sealed class MetricCellSettingsStore : IClusterSettingsStore
    {
        public ClusterMetricsSettings MetricsSettings { get; } = new();

        public IReadOnlyCollection<string> KubeConfigPaths => [];

        public void AddKubeConfigPath(string path)
        {
        }

        public IReadOnlyCollection<string> GetClusterNamespaces(IClusterRuntime cluster) => [];

        public ClusterMetricsSettings GetClusterMetricsSettings(IClusterRuntime cluster) => MetricsSettings;

        public void Persist()
        {
        }
    }
}
