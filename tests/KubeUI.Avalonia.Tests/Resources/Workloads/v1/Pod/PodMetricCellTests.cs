using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Avalonia.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using NodeCpuHistoryCell = KubeUI.Avalonia.Resources.Core.v1.Node.MetricsHistoryCPUCellView;
using NodeMemoryHistoryCell = KubeUI.Avalonia.Resources.Core.v1.Node.MetricsHistoryMemoryCellView;
using NodeResourceConfig = KubeUI.Avalonia.Resources.Core.v1.Node.V1NodeConfig;
using PodCpuHistoryCell = KubeUI.Avalonia.Resources.Workloads.v1.Pod.MetricsHistoryCPUCellView;
using PodMemoryHistoryCell = KubeUI.Avalonia.Resources.Workloads.v1.Pod.MetricsHistoryMemoryCellView;

namespace KubeUI.Avalonia.Tests.Resources.Workloads.v1.Pod;

public sealed class PodMetricCellTests
{
    [Fact]
    public async Task prometheus_history_index_sums_container_series_by_pod_and_keeps_unlabeled_series()
    {
        var timestamp = DateTimeOffset.Parse("2026-10-08T12:00:00Z");
        var result = new MetricResultSet
        {
            Metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal)
            {
                ["cpuUsage"] =
                [
                    CreateLabeledSeries("cpuUsage", "pod-a", "default", timestamp, 0.2),
                    CreateLabeledSeries("cpuUsage", "pod-a", "default", timestamp, 0.3),
                    CreateLabeledSeries("cpuUsage", "pod-b", "default", timestamp, 0.8),
                    new MetricSeries
                    {
                        Name = "cpuUsage",
                        Points = [new MetricPoint(timestamp, 0.1)],
                    },
                    new MetricSeries
                    {
                        Name = "cpuUsage",
                        Labels = new Dictionary<string, string>(StringComparer.Ordinal) { ["pod"] = "pod-a" },
                        Points = [new MetricPoint(timestamp, 100)],
                    },
                ],
                ["memoryUsage"] =
                [
                    CreateLabeledSeries("memoryUsage", "pod-a", "default", timestamp, 100),
                    new MetricSeries
                    {
                        Name = "memoryUsage",
                        Points = [new MetricPoint(timestamp, 5)],
                    },
                ],
            },
        };
        var index = new PodPrometheusHistoryIndex();

        var podA = await index.GetHistoryAsync(result, "default", "pod-a", TestContext.Current.CancellationToken);
        var podB = await index.GetHistoryAsync(result, "default", "pod-b", TestContext.Current.CancellationToken);

        podA.Cpu.ShouldHaveSingleItem().Value.ShouldBe(0.6);
        podA.Memory.ShouldHaveSingleItem().Value.ShouldBe(105);
        podB.Cpu.ShouldHaveSingleItem().Value.ShouldBe(0.9);
        podB.Memory.ShouldHaveSingleItem().Value.ShouldBe(5);
        index.IndexBuildCount.ShouldBe(1);
    }

    [Fact]
    public async Task prometheus_history_index_bounds_retained_result_sets()
    {
        var index = new PodPrometheusHistoryIndex();
        MetricResultSet[] results =
        [
            .. Enumerable.Range(0, 5).Select(podIndex => CreatePodMetricResults(
                ($"pod-{podIndex}", 0.2, 100),
                ($"pod-{podIndex}", 0.4, 200),
                DateTimeOffset.UtcNow)),
        ];

        foreach (var (result, podIndex) in results.Select((result, podIndex) => (result, podIndex)))
        {
            await index.GetHistoryAsync(result, "default", $"pod-{podIndex}", TestContext.Current.CancellationToken);
        }

        await index.GetHistoryAsync(results[0], "default", "pod-0", TestContext.Current.CancellationToken);

        index.IndexBuildCount.ShouldBe(6);
    }

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

        var bars = MetricsHistoryBuckets.CreateBars(points, end, 1.0, 20);

        bars.Count.ShouldBe(MetricsHistoryBuckets.BucketCount);
        bars[0].Value.ShouldBe(0.8);
        bars[0].LimitState.ShouldBe(MetricsLimitState.Warning);
        bars[1].Value.ShouldBe(1.0);
        bars[1].LimitState.ShouldBe(MetricsLimitState.Exceeded);
        bars[^1].Value.ShouldBe(0);
        bars[^1].Height.ShouldBe(0);

        MetricsHistoryBuckets.CreateBars(points, end, null, 20)
            .ShouldAllBe(static bar => bar.LimitState == MetricsLimitState.Normal);
    }

    [Fact]
    public void history_bucket_edges_stay_fixed_between_refreshes()
    {
        var end = DateTimeOffset.Parse("2026-09-23T12:02:00Z");
        MetricPoint[] initialPoints = [new(end.AddSeconds(-15), 0.1)];
        MetricPoint[] updatedPoints = [.. initialPoints, new MetricPoint(end.AddSeconds(15), 0.2)];

        var initialBars = MetricsHistoryBuckets.CreateBars(initialPoints, end, 1, 20);
        var updatedBars = MetricsHistoryBuckets.CreateBars(updatedPoints, end.AddSeconds(30), 1, 20);

        for (var index = 0; index < MetricsHistoryBuckets.BucketCount - 1; index++)
        {
            updatedBars[index].ShouldBe(initialBars[index]);
        }

        updatedBars[^1].Start.ShouldBe(initialBars[^1].Start);
        updatedBars[^1].End.ShouldBe(initialBars[^1].End);
        updatedBars[^1].Value.ShouldBe(0.2);
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
        MetricBars(cell).Any(bar => IsThemeBrush(bar.Background, "PodStatusWarningBrush")).ShouldBeTrue();
        MetricBars(cell).Any(bar => TooltipShowsPercentage(bar, 0.9)).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task idle_refresh_tick_does_not_rebuild_metrics_server_bars()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        var pod = CreatePod();
        fixture.UseMetricsServerSamples(CreatePodMetricSample(pod, DateTime.UtcNow, "100m"));
        var cell = fixture.CreateCpuCell(pod);
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        Dispatcher.UIThread.RunJobs();
        fixture.TimeProvider.ResetGetUtcNowCount();

        fixture.RefreshClock.Tick();

        fixture.TimeProvider.GetUtcNowCount.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task prometheus_history_aggregation_runs_off_ui_thread()
    {
        var result = new MetricResultSet
        {
            Metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal)
            {
                ["cpuUsage"] =
                [
                    new MetricSeries
                    {
                        Name = "cpuUsage",
                        Points = [new MetricPoint(DateTimeOffset.UtcNow, 1)],
                    },
                ],
            },
        };
        var aggregationRanOnUiThread = false;

        var history = await MetricsHistoryAggregator.AggregateAsync(
            result,
            new object(),
            (_, _) =>
            {
                aggregationRanOnUiThread = Dispatcher.UIThread.CheckAccess();
                return true;
            },
            TestContext.Current.CancellationToken);

        aggregationRanOnUiThread.ShouldBeFalse();
        history.Cpu.Count.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task metrics_server_history_aggregation_runs_off_ui_thread()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        var pod = CreatePod();
        fixture.UseMetricsServerSamples(CreatePodMetricSample(pod, DateTime.UtcNow, "100m"));
        var cell = new ThreadCheckingPodHistoryCell(fixture.RefreshClock, fixture.TimeProvider)
        {
            DataContext = pod,
        };
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        var aggregationRanOnUiThread = await cell.AggregationStarted.Task.WaitAsync(TestContext.Current.CancellationToken);

        aggregationRanOnUiThread.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task metrics_server_history_ignores_work_completed_after_cell_is_hidden()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        var pod = CreatePod();
        fixture.UseMetricsServerSamples(CreatePodMetricSample(pod, DateTime.UtcNow, "100m"));
        var cell = new ThreadCheckingPodHistoryCell(fixture.RefreshClock, fixture.TimeProvider, blockFirstAggregation: true)
        {
            DataContext = pod,
        };
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        await cell.AggregationStarted.Task.WaitAsync(TestContext.Current.CancellationToken);

        try
        {
            cell.IsVisible = false;
            fixture.TimeProvider.Advance(TimeSpan.FromSeconds(30));
            fixture.ReplaceMetricsServerSamples(CreatePodMetricSample(
                pod,
                fixture.TimeProvider.GetUtcNow().AddSeconds(-1).UtcDateTime,
                "450m"));
            cell.IsVisible = true;
            await cell.SecondAggregationStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
            cell.ReleaseFirstAggregation.TrySetResult();

            await TestWait.UntilAsync(
                () => MetricBars(cell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("0.45c", StringComparison.Ordinal) == true),
                timeout: TimeSpan.FromSeconds(5),
                cancellationToken: TestContext.Current.CancellationToken,
                beforePoll: () => Dispatcher.UIThread.RunJobs());
            MetricBars(cell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("0.1c", StringComparison.Ordinal) == true).ShouldBeFalse();
        }
        finally
        {
            cell.ReleaseFirstAggregation.TrySetResult();
        }
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

        var panel = cell.GetVisualDescendants().OfType<Canvas>().Single();
        var bars = MetricBars(cell);
        var layoutBars = panel.Children.OfType<Border>().ToArray();
        double.IsNaN(panel.Height).ShouldBeTrue();
        bars.ShouldNotBeEmpty();
        cell.Margin.ShouldBe(new Thickness(4, 0));
        (Canvas.GetLeft(layoutBars[1]) - layoutBars[0].Width).ShouldBe(2);
        (bars[^1].Bounds.Right / panel.Bounds.Width).ShouldBeGreaterThan(0.95);
    }

    [AvaloniaFact]
    public async Task history_bars_use_the_available_cell_height()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        var pod = CreatePod();
        fixture.UseMetricsServerSamples(CreatePodMetricSample(pod, DateTime.UtcNow.AddMinutes(-1), "450m"));
        var cell = fixture.CreateCpuCell(pod);
        cell.Height = 64;
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        Dispatcher.UIThread.RunJobs();

        MetricBars(cell).Max(static bar => bar.Bounds.Height)
            .ShouldBeGreaterThan(cell.Bounds.Height * 0.85);
    }

    [AvaloniaFact]
    public async Task metrics_cell_does_not_increase_resource_table_row_height()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        var pod = CreatePod();
        fixture.UseMetricsServerSamples(CreatePodMetricSample(pod, DateTime.UtcNow.AddMinutes(-1), "450m"));
        PodCpuHistoryCell? metricCell = null;
        var metricTable = new DynamicTableView { ItemsSource = new[] { pod } };
        metricTable.Columns.Add(new TableViewColumn
        {
            Header = "CPU",
            CellTemplate = new FuncDataTemplate<V1Pod>((item, _) =>
            {
                metricCell = fixture.CreateCpuCell(item!);
                metricCell.Initialize(fixture.Workspace);
                return metricCell;
            })
        });
        metricTable.Columns.Add(new TableViewColumn
        {
            Header = "Name",
            CellTemplate = new FuncDataTemplate<V1Pod>((item, _) => new TextBlock { Text = item!.Name() })
        });
        var textTable = new DynamicTableView { ItemsSource = new[] { pod } };
        textTable.Columns.Add(new TableViewColumn
        {
            Header = "CPU",
            CellTemplate = new FuncDataTemplate<V1Pod>((_, _) => new TextBlock { Text = "450m" })
        });
        textTable.Columns.Add(new TableViewColumn
        {
            Header = "Name",
            CellTemplate = new FuncDataTemplate<V1Pod>((item, _) => new TextBlock { Text = item!.Name() })
        });
        var content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            Children = { metricTable, textTable }
        };
        Grid.SetColumn(textTable, 1);
        using var window = Application.Current.CreateTestWindow(content: content);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        metricCell.ShouldNotBeNull();
        metricCell!.Bounds.Height.ShouldBeGreaterThan(0);
        MetricBars(metricCell!).ShouldNotBeEmpty();
        var metricRowHeight = metricTable.GetVisualDescendants().OfType<TableViewRow>().Single().Bounds.Height;
        var textRowHeight = textTable.GetVisualDescendants().OfType<TableViewRow>().Single().Bounds.Height;
        Math.Abs(metricRowHeight - textRowHeight).ShouldBeLessThan(1);
    }

    [AvaloniaFact]
    public async Task hidden_prometheus_history_cell_does_not_query_until_visible()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateMetricResult("cpuUsage", 1.234),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var cell = fixture.CreateCpuCell(CreatePod());
        cell.IsVisible = false;
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        Dispatcher.UIThread.RunJobs();

        queryClient.Queries.ShouldBe(0);
        fixture.RefreshClock.SubscriberCount.ShouldBe(0);

        cell.IsVisible = true;
        await TestWait.UntilAsync(
            () => fixture.RefreshClock.SubscriberCount == 1,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        fixture.RefreshClock.Tick();
        await TestWait.UntilAsync(
            () => queryClient.Queries == 2,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    [AvaloniaFact]
    public async Task off_viewport_prometheus_history_cell_waits_until_scrolled_into_view()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateMetricResult("cpuUsage", 1.234),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var cell = fixture.CreateCpuCell(CreatePod());
        var content = new StackPanel
        {
            Children =
            {
                new Border { Height = 500 },
                cell,
            },
        };
        var scrollViewer = new ScrollViewer { Content = content };
        Rect? effectiveViewport = null;
        cell.EffectiveViewportChanged += (_, args) => effectiveViewport = args.EffectiveViewport;
        using var window = Application.Current.CreateTestWindow(content: scrollViewer);
        window.Width = 200;
        window.Height = 120;

        window.Show();
        cell.Initialize(fixture.Workspace);
        Dispatcher.UIThread.RunJobs();
        effectiveViewport.ShouldNotBeNull();
        effectiveViewport!.Value.Intersects(new Rect(cell.Bounds.Size)).ShouldBeFalse();
        queryClient.Queries.ShouldBe(0);
        fixture.RefreshClock.SubscriberCount.ShouldBe(0);

        scrollViewer.Offset = new Vector(0, 500);
        await TestWait.UntilAsync(
            () => queryClient.Queries == 2 && fixture.RefreshClock.SubscriberCount == 1,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    [AvaloniaFact]
    public async Task hidden_metrics_server_history_pauses_refresh_until_visible()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        var pod = CreatePod();
        fixture.UseMetricsServerSamples(CreatePodMetricSample(pod, DateTime.UtcNow, "100m"));
        var cell = fixture.CreateCpuCell(pod);
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        await WaitForMetricTooltipAsync(cell, "0.1c");
        fixture.RefreshClock.SubscriberCount.ShouldBe(1);

        cell.IsVisible = false;
        fixture.RefreshClock.SubscriberCount.ShouldBe(0);
        fixture.TimeProvider.Advance(TimeSpan.FromSeconds(30));
        fixture.AddMetricsServerSample(CreatePodMetricSample(pod, fixture.TimeProvider.GetUtcNow().AddSeconds(-1).UtcDateTime, "450m"));
        fixture.RefreshClock.Tick();

        MetricBars(cell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("0.1c", StringComparison.Ordinal) == true).ShouldBeTrue();
        MetricBars(cell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("0.45c", StringComparison.Ordinal) == true).ShouldBeFalse();

        cell.IsVisible = true;
        await TestWait.UntilAsync(
            () => fixture.RefreshClock.SubscriberCount == 1
                && MetricBars(cell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("0.45c", StringComparison.Ordinal) == true),
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    [AvaloniaFact]
    public async Task prometheus_history_cell_refreshes_every_minute()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateMetricResult("cpuUsage", 1.234),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var initialTime = fixture.TimeProvider.GetUtcNow();
        fixture.TimeProvider.SetUtcNow(new DateTimeOffset(
            initialTime.Year,
            initialTime.Month,
            initialTime.Day,
            initialTime.Hour,
            initialTime.Minute,
            0,
            TimeSpan.Zero));
        var cell = fixture.CreateCpuCell(CreatePod());
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        await TestWait.UntilAsync(
            () => queryClient.Queries == 2,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        fixture.TimeProvider.Advance(TimeSpan.FromSeconds(59));
        fixture.RefreshClock.Tick();
        Dispatcher.UIThread.RunJobs();
        queryClient.Queries.ShouldBe(2);

        fixture.TimeProvider.Advance(TimeSpan.FromSeconds(1));
        fixture.RefreshClock.Tick();
        await TestWait.UntilAsync(
            () => queryClient.Queries == 4,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    [AvaloniaFact]
    public async Task hidden_prometheus_history_cell_pauses_query_schedule_until_visible()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateMetricResult("cpuUsage", 1.234),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var cell = fixture.CreateCpuCell(CreatePod());
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        await TestWait.UntilAsync(
            () => queryClient.Queries == 2,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        cell.IsVisible = false;
        fixture.RefreshClock.SubscriberCount.ShouldBe(0);
        fixture.TimeProvider.Advance(TimeSpan.FromMinutes(1));
        fixture.RefreshClock.Tick();
        queryClient.Queries.ShouldBe(2);

        cell.IsVisible = true;
        await TestWait.UntilAsync(
            () => queryClient.Queries == 4 && fixture.RefreshClock.SubscriberCount == 1,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    [AvaloniaFact]
    public async Task prometheus_history_keeps_refresh_schedule_for_same_resource_update()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateMetricResult("cpuUsage", 1.234),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var initialTime = fixture.TimeProvider.GetUtcNow();
        fixture.TimeProvider.SetUtcNow(new DateTimeOffset(
            initialTime.Year,
            initialTime.Month,
            initialTime.Day,
            initialTime.Hour,
            initialTime.Minute,
            0,
            TimeSpan.Zero));
        var pod = CreatePod();
        var cell = fixture.CreateCpuCell(pod);
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        await TestWait.UntilAsync(
            () => queryClient.Queries == 2 && MetricBars(cell).Length > 0,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        fixture.TimeProvider.Advance(TimeSpan.FromSeconds(30));
        var updatedPod = CreatePod();
        updatedPod.Metadata!.ResourceVersion = "2";
        cell.DataContext = updatedPod;
        Dispatcher.UIThread.RunJobs();
        MetricBars(cell).ShouldNotBeEmpty();
        queryClient.Queries.ShouldBe(2);

        fixture.TimeProvider.Advance(TimeSpan.FromSeconds(29));
        fixture.RefreshClock.Tick();
        Dispatcher.UIThread.RunJobs();
        queryClient.Queries.ShouldBe(2);

        fixture.TimeProvider.Advance(TimeSpan.FromSeconds(1));
        fixture.RefreshClock.Tick();
        await TestWait.UntilAsync(
            () => queryClient.Queries == 4,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
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
    public async Task visible_pod_cells_in_same_namespace_share_prometheus_history_request()
    {
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-2);
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreatePodMetricResults(
                ("pod-a", 0.1, 128 * 1024 * 1024d),
                ("pod-b", 0.4, 256 * 1024 * 1024d),
                timestamp),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var podA = CreatePod("pod-a");
        var podB = CreatePod("pod-b");
        var cpuCellA = fixture.CreateCpuCell(podA);
        var memoryCellA = fixture.CreateMemoryCell(podA);
        var cpuCellB = fixture.CreateCpuCell(podB);
        var memoryCellB = fixture.CreateMemoryCell(podB);
        var content = new StackPanel
        {
            Children = { cpuCellA, memoryCellA, cpuCellB, memoryCellB },
        };
        using var window = Application.Current.CreateTestWindow(content: content);

        window.Show();
        cpuCellA.Initialize(fixture.Workspace);
        memoryCellA.Initialize(fixture.Workspace);
        cpuCellB.Initialize(fixture.Workspace);
        memoryCellB.Initialize(fixture.Workspace);

        await TestWait.UntilAsync(
            () => MetricBars(cpuCellA).Any(bar => ToolTip.GetTip(bar) is not null)
                && MetricBars(memoryCellA).Any(bar => ToolTip.GetTip(bar) is not null)
                && MetricBars(cpuCellB).Any(bar => ToolTip.GetTip(bar) is not null)
                && MetricBars(memoryCellB).Any(bar => ToolTip.GetTip(bar) is not null),
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        MetricBars(cpuCellA).Any(bar => TooltipShowsPercentage(bar, 0.2)).ShouldBeTrue();
        MetricBars(memoryCellA).Any(bar => TooltipShowsPercentage(bar, 0.25)).ShouldBeTrue();
        MetricBars(cpuCellB).Any(bar => TooltipShowsPercentage(bar, 0.8)).ShouldBeTrue();
        MetricBars(memoryCellB).Any(bar => TooltipShowsPercentage(bar, 0.5)).ShouldBeTrue();
        queryClient.Queries.ShouldBe(2);
        fixture.PrometheusHistoryIndex.IndexBuildCount.ShouldBe(1);
        queryClient.QueryTexts.ShouldAllBe(query => query.Contains("pod=~\".*\"", StringComparison.Ordinal));
    }

    [AvaloniaFact]
    public async Task recycled_prometheus_cell_reuses_indexed_namespace_history()
    {
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-2);
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreatePodMetricResults(
                ("pod-a", 0.1, 128 * 1024 * 1024d),
                ("pod-b", 0.4, 256 * 1024 * 1024d),
                timestamp),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var cell = fixture.CreateCpuCell(CreatePod("pod-a"));
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        await WaitForMetricTooltipAsync(cell, "0.1c");

        cell.DataContext = CreatePod("pod-b");
        await WaitForMetricTooltipAsync(cell, "0.4c");

        queryClient.Queries.ShouldBe(2);
        fixture.PrometheusHistoryIndex.IndexBuildCount.ShouldBe(1);
        MetricBars(cell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("0.1c", StringComparison.Ordinal) == true).ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task recycled_metrics_server_cell_reads_the_new_pods_retained_snapshot()
    {
        var podA = CreatePod("pod-a");
        var podB = CreatePod("pod-b");
        var sampleTime = DateTime.UtcNow.AddMinutes(-2);
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        fixture.UseMetricsServerSamples(
            CreatePodMetricSample(podA, sampleTime, "100m"),
            CreatePodMetricSample(podB, sampleTime, "450m"));
        var cell = fixture.CreateCpuCell(podA);
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        await WaitForMetricTooltipAsync(cell, "0.1c");

        cell.DataContext = podB;
        await WaitForMetricTooltipAsync(cell, "0.45c");

        fixture.QueryClient.Queries.ShouldBe(0);
        MetricBars(cell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("0.1c", StringComparison.Ordinal) == true).ShouldBeFalse();
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

        cpuCell.DataContext = CreateNode("node-a");
        Dispatcher.UIThread.RunJobs();
        queryClient.Queries.ShouldBe(2);
    }

    [AvaloniaFact]
    public async Task node_prometheus_history_refreshes_when_instance_target_changes()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateNodeMetricResultsWithInstanceLabel("10.0.0.1:9100", 0.5, 512d * 1024 * 1024),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var cell = fixture.CreateNodeCpuCell(CreateNode("node-a"));
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        await TestWait.UntilAsync(
            () => queryClient.Queries == 2,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        cell.DataContext = CreateNode("node-a", "10.0.0.2");
        await TestWait.UntilAsync(
            () => queryClient.Queries == 4,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        queryClient.QueryTexts.ShouldContain(query =>
            query.Contains("node_cpu_seconds_total", StringComparison.Ordinal)
            && query.Contains("instance=~\"(node-a|10\\\\.0\\\\.0\\\\.2)(:[0-9]+)?\"", StringComparison.Ordinal));
    }

    [AvaloniaFact]
    public async Task prometheus_pod_history_uses_namespace_query_for_cache_reuse()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateMetricResult("cpuUsage", 1.234, "apicurio.registry"),
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
            query.Contains("pod=~\".*\"", StringComparison.Ordinal));
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

        await WaitForMetricBarsAsync(cell);
        fixture.QueryClient.Queries.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task metrics_server_history_keeps_samples_until_refresh_after_same_resource_update()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        var pod = CreatePod(cpuLimit: "1");
        var now = DateTimeOffset.Parse("2026-09-23T12:02:00Z");
        fixture.UseMetricsServerSamples(CreatePodMetricSample(pod, now.AddMinutes(-2).UtcDateTime, "100m"));
        fixture.TimeProvider.SetUtcNow(now);
        var cell = fixture.CreateCpuCell(pod);
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);
        await WaitForMetricTooltipAsync(cell, "0.1c");

        var updatedPod = CreatePod(cpuLimit: "100m");
        updatedPod.Metadata!.ResourceVersion = "2";
        fixture.AddMetricsServerSample(CreatePodMetricSample(updatedPod, now.AddSeconds(29).UtcDateTime, "450m"));
        cell.DataContext = updatedPod;
        Dispatcher.UIThread.RunJobs();

        var bars = MetricBars(cell);
        bars.Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("0.1c", StringComparison.Ordinal) == true).ShouldBeTrue();
        bars.Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("0.45c", StringComparison.Ordinal) == true).ShouldBeFalse();
        bars.Any(bar => IsThemeBrush(bar.Background, "ContainerStatusErrorBrush")).ShouldBeTrue();
        bars.Any(bar => TooltipShowsPercentage(bar, 1)).ShouldBeTrue();

        var previousBarStates = cell.GetVisualDescendants().OfType<Border>().Select(bar => (
            bar.Height,
            bar.Background,
            ToolTip.GetTip(bar),
            Canvas.GetLeft(bar),
            Canvas.GetTop(bar))).ToArray();
        fixture.TimeProvider.Advance(TimeSpan.FromSeconds(30));
        fixture.RefreshClock.Tick();
        await WaitForMetricTooltipAsync(cell, "0.45c");
        var refreshedBars = cell.GetVisualDescendants().OfType<Border>().ToArray();
        MetricBars(cell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains("0.45c", StringComparison.Ordinal) == true).ShouldBeTrue();
        refreshedBars.Length.ShouldBe(previousBarStates.Length);

        List<int> changedBars = [];
        for (var index = 0; index < refreshedBars.Length; index++)
        {
            var refreshedBar = refreshedBars[index];
            var previousState = previousBarStates[index];
            if (refreshedBar.Height != previousState.Height
                || !Equals(refreshedBar.Background, previousState.Background)
                || !Equals(ToolTip.GetTip(refreshedBar), previousState.Item3)
                || Canvas.GetLeft(refreshedBar) != previousState.Item4
                || Canvas.GetTop(refreshedBar) != previousState.Item5)
            {
                changedBars.Add(index);
            }
        }

        changedBars.ShouldHaveSingleItem().ShouldBe(MetricsHistoryBuckets.BucketCount - 1);
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

        await WaitForMetricBarsAsync(cell);
        MetricBars(cell).Any(bar => IsThemeBrush(bar.Background, "ContainerStatusErrorBrush")).ShouldBeTrue();
        MetricBars(cell).Any(bar => TooltipShowsPercentage(bar, 1)).ShouldBeTrue();
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
        await WaitForMetricTooltipAsync(cell, "0.1c");

        cell.DataContext = secondPod;

        await WaitForMetricTooltipAsync(cell, "0.3c");
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
        await WaitForMetricBarsAsync(cell);

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
    public async Task node_resource_columns_use_history_controls_before_metrics_initialization()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        fixture.SetBackend(ActiveMetricsBackend.None);
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
    public async Task node_capacity_columns_keep_history_controls_when_metrics_are_unavailable()
    {
        await using var fixture = await MetricCellFixture.CreateAsync(new FakePrometheusQueryClient());
        fixture.SetBackend(ActiveMetricsBackend.None);
        var config = Application.Current.GetTestServices().GetRequiredService<NodeResourceConfig>();
        config.Initialize(fixture.Workspace);

        var columns = config.Columns();

        columns.Single(column => column.Key == "cpu").CustomControl.ShouldBe(typeof(NodeCpuHistoryCell));
        columns.Single(column => column.Key == "memory").CustomControl.ShouldBe(typeof(NodeMemoryHistoryCell));
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

        await TestWait.UntilAsync(
            () => MetricBars(cpuCell).Length > 0 && MetricBars(memoryCell).Length > 0,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        MetricBars(cpuCell).Any(bar => IsThemeBrush(bar.Background, "PodStatusWarningBrush")).ShouldBeTrue();
        MetricBars(cpuCell).Any(bar => TooltipShowsPercentage(bar, 0.8)).ShouldBeTrue();
        MetricBars(memoryCell).Any(bar => IsThemeBrush(bar.Background, "ContainerStatusErrorBrush")).ShouldBeTrue();
        MetricBars(memoryCell).Any(bar => TooltipShowsPercentage(bar, 1)).ShouldBeTrue();
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

    private static Task WaitForMetricBarsAsync(Control cell)
    {
        return TestWait.UntilAsync(
            () => MetricBars(cell).Length > 0,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

    private static Task WaitForMetricTooltipAsync(Control cell, string value)
    {
        return TestWait.UntilAsync(
            () => MetricBars(cell).Any(bar => ToolTip.GetTip(bar)?.ToString()?.Contains(value, StringComparison.Ordinal) == true),
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
    }

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

    private static bool TooltipShowsPercentage(Control bar, double fraction)
        => ToolTip.GetTip(bar)?.ToString()?.Contains(
            fraction.ToString("P0", System.Globalization.CultureInfo.CurrentCulture),
            StringComparison.Ordinal) == true;

    private static V1Pod CreatePod(string name = "metrics-pod", string cpuLimit = "500m")
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
                            limits = new Dictionary<string, string> { ["cpu"] = cpuLimit, ["memory"] = "512Mi" },
                        },
                    },
                },
            },
        }));
    }

    private static V1Node CreateNode(string name = "metrics-node", string address = "10.0.0.1")
    {
        return KubernetesJson.Deserialize<V1Node>(JsonSerializer.Serialize(new
        {
            metadata = new { name },
            status = new
            {
                addresses = new[] { new { type = "InternalIP", address } },
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

    private static MetricResultSet CreateMetricResult(string queryName, double value, string podName = "metrics-pod") => new()
    {
        Metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal)
        {
            [queryName] =
            [
                new MetricSeries
                {
                    Name = queryName,
                    Labels = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["pod"] = podName,
                        ["namespace"] = "default",
                    },
                    Points = [new MetricPoint(DateTimeOffset.UtcNow.AddMinutes(-2), value)],
                },
            ],
        },
    };

    private static MetricSeries CreateLabeledSeries(
        string name,
        string podName,
        string namespaceName,
        DateTimeOffset timestamp,
        double value)
    {
        return new MetricSeries
        {
            Name = name,
            Labels = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["pod"] = podName,
                ["namespace"] = namespaceName,
            },
            Points = [new MetricPoint(timestamp, value)],
        };
    }

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
                        Labels = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["pod"] = "metrics-pod",
                            ["namespace"] = "default",
                        },
                        Points = [new MetricPoint(DateTimeOffset.UtcNow.AddMinutes(-2), item.Value)],
                    },
                ],
                StringComparer.Ordinal),
        };
    }

    private static MetricResultSet CreatePodMetricResults(
        (string PodName, double Cpu, double Memory) first,
        (string PodName, double Cpu, double Memory) second,
        DateTimeOffset timestamp)
    {
        static MetricSeries CreateSeries(string name, string podName, double value, DateTimeOffset timestamp)
        {
            return new MetricSeries
            {
                Name = name,
                Labels = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["pod"] = podName,
                    ["namespace"] = "default",
                },
                Points = [new MetricPoint(timestamp, value)],
            };
        }

        return new MetricResultSet
        {
            Metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal)
            {
                ["cpuUsage"] =
                [
                    CreateSeries("cpuUsage", first.PodName, first.Cpu, timestamp),
                    CreateSeries("cpuUsage", second.PodName, second.Cpu, timestamp),
                ],
                ["memoryUsage"] =
                [
                    CreateSeries("memoryUsage", first.PodName, first.Memory, timestamp),
                    CreateSeries("memoryUsage", second.PodName, second.Memory, timestamp),
                ],
            },
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

    private sealed class ThreadCheckingPodHistoryCell : PodMetricsHistoryCellBase
    {
        private readonly bool _blockFirstAggregation;
        private int _seriesMatchCount;

        public ThreadCheckingPodHistoryCell(IUiRefreshClock refreshClock, TimeProvider timeProvider)
            : this(refreshClock, timeProvider, blockFirstAggregation: false)
        {
        }

        public ThreadCheckingPodHistoryCell(IUiRefreshClock refreshClock, TimeProvider timeProvider, bool blockFirstAggregation)
            : base(refreshClock, timeProvider, new PodPrometheusHistoryIndex())
        {
            _blockFirstAggregation = blockFirstAggregation;
        }

        public TaskCompletionSource<bool> AggregationStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> SecondAggregationStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource ReleaseFirstAggregation { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override bool IsMemoryMetric => false;

        protected override MetricRequest CreatePrometheusRequest(V1Pod pod, DateTimeOffset end) => new()
        {
            Category = MetricCategory.Pods,
            Queries = [],
        };

        protected override double? GetMetricLimit(V1Pod pod) => null;

        protected override bool MatchesSeries(V1Pod pod, MetricSeries series)
        {
            var matchCount = Interlocked.Increment(ref _seriesMatchCount);
            if (matchCount == 1)
            {
                AggregationStarted.TrySetResult(Dispatcher.UIThread.CheckAccess());
                if (_blockFirstAggregation)
                {
                    ReleaseFirstAggregation.Task.GetAwaiter().GetResult();
                }
            }
            else if (matchCount == 2)
            {
                SecondAggregationStarted.TrySetResult(Dispatcher.UIThread.CheckAccess());
            }

            return true;
        }
    }

    private sealed class MetricCellFixture : IAsyncDisposable
    {
        private readonly MetricsService _metricsService;
        private readonly Cluster _cluster;

        private MetricCellFixture(
            Cluster cluster,
            ClusterWorkspace workspace,
            MetricsService metricsService,
            TestUiRefreshClock refreshClock,
            TestTimeProvider timeProvider,
            FakePrometheusQueryClient queryClient)
        {
            _cluster = cluster;
            Workspace = workspace;
            _metricsService = metricsService;
            RefreshClock = refreshClock;
            TimeProvider = timeProvider;
            QueryClient = queryClient;
            PrometheusHistoryIndex = new PodPrometheusHistoryIndex();
        }

        public ClusterWorkspace Workspace { get; }

        public TestUiRefreshClock RefreshClock { get; }

        public TestTimeProvider TimeProvider { get; }

        public FakePrometheusQueryClient QueryClient { get; }

        public PodPrometheusHistoryIndex PrometheusHistoryIndex { get; }

        public PodCpuHistoryCell CreateCpuCell(V1Pod pod)
            => new(RefreshClock, TimeProvider, PrometheusHistoryIndex) { DataContext = pod };

        public PodMemoryHistoryCell CreateMemoryCell(V1Pod pod)
            => new(RefreshClock, TimeProvider, PrometheusHistoryIndex) { DataContext = pod };

        public NodeCpuHistoryCell CreateNodeCpuCell(V1Node node)
            => new(RefreshClock, TimeProvider) { DataContext = node };

        public NodeMemoryHistoryCell CreateNodeMemoryCell(V1Node node)
            => new(RefreshClock, TimeProvider) { DataContext = node };

        public void UseMetricsServerSamples(params PodMetrics[] samples)
        {
            _metricsService.ActiveMetricsBackend = ActiveMetricsBackend.KubernetesMetricsServer;
            TimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddSeconds(1));
            foreach (var sample in samples)
            {
                _metricsService.PodMetrics.Add(sample);
            }

            _metricsService.RebuildMetricSnapshots();
        }

        public void AddMetricsServerSample(PodMetrics sample)
        {
            _metricsService.PodMetrics.Add(sample);
            _metricsService.RebuildMetricSnapshots();
        }

        public void ReplaceMetricsServerSamples(params PodMetrics[] samples)
        {
            _metricsService.PodMetrics.Clear();
            foreach (var sample in samples)
            {
                _metricsService.PodMetrics.Add(sample);
            }

            _metricsService.RebuildMetricSnapshots();
        }

        public void UseNodeMetricsSamples(params NodeMetrics[] samples)
        {
            _metricsService.ActiveMetricsBackend = ActiveMetricsBackend.KubernetesMetricsServer;
            TimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddSeconds(1));
            foreach (var sample in samples)
            {
                _metricsService.NodeMetrics.Add(sample);
            }

            _metricsService.RebuildMetricSnapshots();
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
            var refreshClock = new TestUiRefreshClock();
            var timeProvider = new TestTimeProvider(DateTimeOffset.UtcNow);
            var metricsService = new MetricsService(
                NullLogger<MetricsService>.Instance,
                settings,
                [new ExternalPrometheusProvider()],
                queryClient,
                timeProvider);
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
                refreshClock,
                timeProvider,
                queryClient);
        }

        public async ValueTask DisposeAsync()
        {
            Workspace.Dispose();
            await _cluster.DisposeAsync();
            _metricsService.Dispose();
        }
    }

    private sealed class TestUiRefreshClock : IUiRefreshClock
    {
        private readonly List<Action> _callbacks = [];

        public int SubscriberCount => _callbacks.Count;

        public IDisposable Subscribe(Action callback)
        {
            _callbacks.Add(callback);
            return new RefreshSubscription(_callbacks, callback);
        }

        public void Tick()
        {
            foreach (var callback in _callbacks.ToArray())
            {
                callback();
            }
        }
    }

    private sealed class RefreshSubscription : IDisposable
    {
        private List<Action>? _callbacks;
        private readonly Action _callback;

        public RefreshSubscription(List<Action> callbacks, Action callback)
        {
            _callbacks = callbacks;
            _callback = callback;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _callbacks, null)?.Remove(_callback);
        }
    }

    private sealed class TestTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow;
        private int _getUtcNowCount;

        public TestTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public int GetUtcNowCount => _getUtcNowCount;

        public override DateTimeOffset GetUtcNow()
        {
            _getUtcNowCount++;
            return _utcNow;
        }

        public void ResetGetUtcNowCount()
        {
            _getUtcNowCount = 0;
        }

        public void Advance(TimeSpan duration)
        {
            _utcNow += duration;
        }

        public void SetUtcNow(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
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
