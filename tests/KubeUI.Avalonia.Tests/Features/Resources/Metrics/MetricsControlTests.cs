using System.Collections.ObjectModel;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Infrastructure.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using Shouldly;
using System.Net;
using System.Text;
using System.Text.Json;
using AppResources = KubeUI.Avalonia.Assets.Resources;
using NamespacePropertiesView = KubeUI.Avalonia.Resources.Core.v1.Namespace.PropertiesView;
using NodePropertiesView = KubeUI.Avalonia.Resources.Core.v1.Node.PropertiesView;
using IngressPropertiesView = KubeUI.Avalonia.Resources.Network.v1.Ingress.PropertiesView;
using PersistentVolumeClaimPropertiesView = KubeUI.Avalonia.Resources.Storage.v1.PersistentVolumeClaim.PropertiesView;
using DaemonSetPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.DaemonSet.PropertiesView;
using DeploymentPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.Deployment.PropertiesView;
using JobPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.Job.PropertiesView;
using PodPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.Pod.PropertiesView;
using ReplicaSetPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.ReplicaSet.PropertiesView;
using StatefulSetPropertiesView = KubeUI.Avalonia.Resources.Workloads.v1.StatefulSet.PropertiesView;

namespace KubeUI.Avalonia.Tests.Features.Resources.Metrics;

public sealed class MetricsControlTests
{
    [AvaloniaFact]
    public void csharp_view_builds_without_axaml()
    {
        using var control = new MetricsControl();

        var section = control.Content.ShouldBeOfType<ExpandableSection>();
        section.Header.ShouldBe(AppResources.Shared_Metrics);
        var grid = section.Content.ShouldBeOfType<Grid>();
        grid.Children.OfType<StackPanel>().Single()
            .Children.OfType<StackPanel>().Single()
            .Children.OfType<ComboBox>().ShouldHaveSingleItem();
        grid.Children.OfType<Border>().ShouldHaveSingleItem();
        grid.Children.OfType<TextBlock>().ShouldHaveSingleItem();
    }

    [AvaloniaFact]
    public void metrics_chart_keeps_minimum_height_and_default_spacing()
    {
        using var control = new MetricsControl();

        var grid = control.Content.ShouldBeOfType<ExpandableSection>().Content.ShouldBeOfType<Grid>();
        var chartCard = grid.Children.OfType<Border>().ShouldHaveSingleItem();

        chartCard.MinHeight.ShouldBeGreaterThan(0);
        var defaultBorder = new Border();
        chartCard.Padding.ShouldBe(defaultBorder.Padding);
        chartCard.Margin.ShouldBe(defaultBorder.Margin);
        var chart = chartCard.Child.ShouldBeOfType<ResponsiveCartesianChart>();
        chart.Title.ShouldBeNull();
        var defaultChart = new ResponsiveCartesianChart();
        chart.Padding.ShouldBe(defaultChart.Padding);
        chart.Margin.ShouldBe(defaultChart.Margin);
        chart.DrawMargin.ShouldBe(defaultChart.DrawMargin);
    }

    [AvaloniaFact]
    public void metrics_chart_uses_twelve_hour_axis_and_tooltip_time()
    {
        using var panel = new MetricPanelViewModel { Title = "CPU" };
        var timestamp = new DateTime(2026, 1, 2, 15, 4, 0, DateTimeKind.Local);

        var axis = panel.XAxes.ShouldHaveSingleItem().ShouldBeOfType<DateTimeAxis>();
        axis.Labeler.ShouldNotBeNull();
        axis.Labeler!(timestamp.Ticks).ShouldBe("3:04 PM");

        panel.MergeSeries(
        [
            new MetricSeriesSnapshot("Usage", [new DateTimePoint(timestamp, 0.1)]),
        ]);

        var series = panel.Series.ShouldHaveSingleItem().ShouldBeOfType<LineSeries<DateTimePoint>>();
        series.XToolTipLabelFormatter.ShouldNotBeNull();
        series.YToolTipLabelFormatter.ShouldNotBeNull();
    }

    [Fact]
    public void metrics_tooltip_values_are_rounded_to_two_decimals_and_keep_compact_units()
    {
        MetricPanelViewModel.FormatTooltipValue(54_337_536).ShouldBe("54.34 M");
        MetricPanelViewModel.FormatTooltipValue(0.0123456).ShouldBe("0.01");
        MetricPanelViewModel.FormatTooltipValue(0.000000123456).ShouldBe("0.12 µ");
    }

    [AvaloniaFact]
    public void metric_refresh_preserves_series_values_and_existing_point_identity()
    {
        using var panel = new MetricPanelViewModel { Title = "CPU" };
        var firstTime = new DateTime(2026, 1, 2, 15, 0, 0, DateTimeKind.Local);
        var secondTime = firstTime.AddMinutes(1);
        var thirdTime = firstTime.AddMinutes(2);

        panel.MergeSeries(
        [
            new MetricSeriesSnapshot("Usage", [new DateTimePoint(firstTime, 1), new DateTimePoint(secondTime, 2)]),
        ]);

        var series = panel.Series.ShouldHaveSingleItem().ShouldBeOfType<LineSeries<DateTimePoint>>();
        var values = series.Values.ShouldBeOfType<ObservableCollection<DateTimePoint>>();
        var firstPoint = values[0];
        var stroke = series.Stroke;
        var fill = series.Fill;

        panel.MergeSeries(
        [
            new MetricSeriesSnapshot("Usage", [new DateTimePoint(firstTime, 1.5), new DateTimePoint(secondTime, 2.5), new DateTimePoint(thirdTime, 3)]),
        ]);

        panel.Series.ShouldHaveSingleItem().ShouldBeSameAs(series);
        series.Values.ShouldBeSameAs(values);
        values[0].ShouldBeSameAs(firstPoint);
        firstPoint.Value.ShouldBe(1.5);
        values.Count.ShouldBe(3);
        series.Stroke.ShouldBeSameAs(stroke);
        series.Fill.ShouldBeSameAs(fill);
    }

    [AvaloniaFact]
    public void metrics_chart_defaults_to_one_hour()
    {
        using var control = new MetricsControl();

        control.SelectedTimeRange.ShouldNotBeNull();
        control.SelectedTimeRange!.RangeSeconds.ShouldBe(60 * 60);
        control.TimeRangeOptions.ShouldContain(option => option.RangeSeconds == 60 * 60);
        control.TimeRangeOptions.ShouldNotContain(option => option.RangeSeconds == 24 * 60 * 60);
    }

    [AvaloniaFact]
    public void nonnegative_metric_values_do_not_extend_below_zero_axis()
    {
        using var panel = new MetricPanelViewModel { Title = "CPU" };
        panel.MergeSeries(
        [
            new MetricSeriesSnapshot(
                "Usage",
                [
                    new DateTimePoint(DateTime.UtcNow, 0),
                    new DateTimePoint(DateTime.UtcNow.AddMinutes(1), 0.1),
                ]),
        ]);

        var yAxis = panel.YAxes.ShouldHaveSingleItem().ShouldBeOfType<Axis>();
        yAxis.MinLimit.ShouldBe(0);
    }

    [AvaloniaFact]
    public void metric_series_keep_the_default_z_order()
    {
        using var panel = new MetricPanelViewModel { Title = "CPU" };
        panel.MergeSeries(
        [
            new MetricSeriesSnapshot("Usage", [new DateTimePoint(DateTime.UtcNow, 0.1)]),
            new MetricSeriesSnapshot("Requests", [new DateTimePoint(DateTime.UtcNow, 0.2)]),
            new MetricSeriesSnapshot("Limits", [new DateTimePoint(DateTime.UtcNow, 0.3)]),
        ]);

        var series = panel.Series.OfType<LineSeries<DateTimePoint>>().ToArray();
        var usage = series.Single(item => item.Name == AppResources.Metrics_Usage);
        var requests = series.Single(item => item.Name == AppResources.Metrics_Requests);
        var limits = series.Single(item => item.Name == AppResources.Metrics_Limits);

        usage.ZIndex.ShouldBe(requests.ZIndex);
        usage.ZIndex.ShouldBe(limits.ZIndex);
    }

    [AvaloniaFact]
    public void metrics_time_range_selector_has_a_visible_and_associated_label()
    {
        using var control = new MetricsControl();

        var content = control.Content.ShouldBeOfType<ExpandableSection>().Content.ShouldBeOfType<Grid>();
        var header = content.Children.OfType<StackPanel>().ShouldHaveSingleItem();
        var selectors = header.Children.OfType<StackPanel>().ShouldHaveSingleItem();
        var label = selectors.Children.OfType<Label>().ShouldHaveSingleItem();
        var selector = selectors.Children.OfType<ComboBox>().ShouldHaveSingleItem();
        var tabs = header.Children.OfType<ItemsControl>().ShouldHaveSingleItem();

        label.Content.ShouldBe(AppResources.MetricsControl_TimeRangeLabel);
        label.Target.ShouldBeSameAs(selector);
        AutomationProperties.GetLabeledBy(selector).ShouldBeSameAs(label);
        selectors.IsVisible.ShouldBeFalse();
        header.Children.IndexOf(tabs).ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task rendered_tab_selection_updates_selected_tab()
    {
        using var control = new MetricsControl();
        var first = new MetricTabViewModel { Title = "CPU" };
        var second = new MetricTabViewModel { Title = "Memory" };
        control.Tabs.Add(first);
        control.Tabs.Add(second);

        using var window = Application.Current.CreateTestWindow(content: control);
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var buttons = control.GetVisualDescendants().OfType<ToggleButton>().Where(button => button.Command != null).ToArray();
        buttons.Length.ShouldBe(2);
        buttons[1].Command!.Execute(null);
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        control.SelectedTab.ShouldBe(second);
        first.IsSelected.ShouldBeFalse();
        second.IsSelected.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task metrics_control_renders_unavailable_state_when_backend_is_disabled()
    {
        await using var fixture = await MetricsControlFixture.CreateAsync(initializePrometheus: false);
        var control = fixture.CreateControl(CreatePod());
        using var window = Application.Current.CreateTestWindow(content: control);

        window.Show();
        fixture.Initialize(control);

        await TestWait.UntilAsync(
            () => control.ShowStatus && control.StatusText == AppResources.MetricsControl_Unavailable,
            5000,
            TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        control.ShowTabs.ShouldBeFalse();
        control.StatusText.ShouldBe(AppResources.MetricsControl_Unavailable);
    }

    [AvaloniaFact]
    public async Task metrics_control_exposes_loading_then_empty_state()
    {
        var queryClient = new FakePrometheusQueryClient { WaitForRelease = true };
        await using var fixture = await MetricsControlFixture.CreateAsync(initializePrometheus: true, queryClient);
        var control = fixture.CreateControl(CreatePod());
        using var window = Application.Current.CreateTestWindow(content: control);

        window.Show();
        fixture.Initialize(control);

        await TestWait.UntilAsync(
            () => control.ShowStatus && control.StatusText == AppResources.MetricsControl_Loading,
            5000,
            TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        queryClient.Release();

        await TestWait.UntilAsync(
            () => control.ShowStatus && control.StatusText == AppResources.MetricsControl_NoMetrics,
            5000,
            TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        control.ShowTabs.ShouldBeTrue();
        control.StatusText.ShouldBe(AppResources.MetricsControl_NoMetrics);
    }

    [AvaloniaFact]
    public async Task metrics_control_renders_failure_state_for_query_exception()
    {
        var queryClient = new FakePrometheusQueryClient { ExceptionToThrow = new InvalidOperationException("query failed") };
        await using var fixture = await MetricsControlFixture.CreateAsync(initializePrometheus: true, queryClient);
        var control = fixture.CreateControl(CreatePod());
        using var window = Application.Current.CreateTestWindow(content: control);

        window.Show();
        fixture.Initialize(control);

        await TestWait.UntilAsync(
            () => control.ShowStatus && control.StatusText == AppResources.MetricsControl_LoadFailed,
            5000,
            TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        control.ShowTabs.ShouldBeTrue();
        control.StatusText.ShouldBe(AppResources.MetricsControl_LoadFailed);
    }

    [AvaloniaFact]
    public async Task metrics_control_renders_prometheus_series_and_preserves_sample_dates()
    {
        var firstTimestamp = DateTimeOffset.Parse("2026-01-02T03:04:05+00:00");
        var queryClient = new FakePrometheusQueryClient();
        var successResponse = CreateSuccessResponse(firstTimestamp, 1.25, 2.5);
        queryClient.ResponseFactory = query => query.Contains("container_cpu_usage_seconds_total", StringComparison.Ordinal)
            ? successResponse
            : CreateEmptyResponse();
        await using var fixture = await MetricsControlFixture.CreateAsync(initializePrometheus: true, queryClient);
        var control = fixture.CreateControl(CreatePod());
        using var window = Application.Current.CreateTestWindow(content: control);

        window.Show();
        fixture.Initialize(control);

        await TestWait.UntilAsync(
            () => queryClient.QueryCalls > 0,
            5000,
            TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        await TestWait.UntilAsync(
            () => control.ShowTabs
                && !control.ShowStatus
                && control.SelectedPanel?.Series.Count > 0,
            5000,
            TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        var series = control.SelectedPanel!.Series.ShouldHaveSingleItem().ShouldBeOfType<LineSeries<DateTimePoint>>();
        var points = series.Values!.OfType<DateTimePoint>().ToArray();
        points.ShouldContain(point => point.DateTime == firstTimestamp.LocalDateTime && point.Value == 1.25);
        points.ShouldContain(point => point.DateTime == firstTimestamp.AddMinutes(1).LocalDateTime && point.Value == 2.5);

        var chart = control.GetVisualDescendants().OfType<CartesianChart>().ShouldHaveSingleItem();
        chart.Series.ShouldContain(series);
    }

    [AvaloniaFact]
    public async Task metrics_control_orders_series_by_name_and_draws_usage_above_requests()
    {
        var queryClient = new FakePrometheusQueryClient();
        var successResponse = CreateSuccessResponse(DateTimeOffset.Parse("2026-01-02T03:04:05+00:00"), 1.25, 2.5);
        queryClient.ResponseFactory = query => query.Contains("container_cpu_usage_seconds_total", StringComparison.Ordinal)
            || query.Contains("kube_pod_container_resource_requests", StringComparison.Ordinal)
            ? successResponse
            : CreateEmptyResponse();
        await using var fixture = await MetricsControlFixture.CreateAsync(initializePrometheus: true, queryClient);
        var control = fixture.CreateControl(CreatePod());
        using var window = Application.Current.CreateTestWindow(content: control);

        window.Show();
        fixture.Initialize(control);

        await TestWait.UntilAsync(
            () => queryClient.QueryCalls > 0,
            5000,
            TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        await TestWait.UntilAsync(
            () => control.ShowTabs
                && !control.ShowStatus
                && control.SelectedPanel?.Series.Count >= 2,
            5000,
            TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        var seriesNames = control.SelectedPanel!.Series
            .OfType<LineSeries<DateTimePoint>>()
            .Select(static series => series.Name)
            .ToArray();
        seriesNames.ShouldBe([AppResources.Metrics_Requests, AppResources.Metrics_Usage]);

        var requestSeries = control.SelectedPanel.Series
            .OfType<LineSeries<DateTimePoint>>()
            .Single(series => series.Name == AppResources.Metrics_Requests);
        var usageSeries = control.SelectedPanel.Series
            .OfType<LineSeries<DateTimePoint>>()
            .Single(series => series.Name == AppResources.Metrics_Usage);
        usageSeries.ZIndex.ShouldBe(requestSeries.ZIndex);
        usageSeries.Fill.ShouldNotBeNull();
        requestSeries.Fill.ShouldNotBeNull();
        usageSeries.Stroke.ShouldNotBeNull();
        requestSeries.Stroke.ShouldNotBeNull();

        await TestWait.UntilAsync(
            () => usageSeries.Fill!.ZIndex > requestSeries.Fill!.ZIndex
                && usageSeries.Stroke!.ZIndex > requestSeries.Stroke!.ZIndex,
            5000,
            TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        usageSeries.Fill!.ZIndex.ShouldBeGreaterThan(requestSeries.Fill!.ZIndex);
        usageSeries.Stroke!.ZIndex.ShouldBeGreaterThan(requestSeries.Stroke!.ZIndex);
    }

    [AvaloniaFact]
    public async Task metrics_control_renders_kubernetes_metrics_server_series()
    {
        await using var fixture = await MetricsControlFixture.CreateMetricsServerAsync();
        var control = fixture.CreateControl(CreatePod());
        using var window = Application.Current.CreateTestWindow(content: control);

        window.Show();
        fixture.Initialize(control);

        await TestWait.UntilAsync(
            () => control.ShowTabs
                && !control.ShowStatus
                && control.SelectedPanel?.Series.Count > 0,
            5000,
            TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        control.Tabs.Count.ShouldBe(2);
        var tabItems = control.GetVisualDescendants()
            .OfType<ItemsControl>()
            .Single(items => ReferenceEquals(items.ItemsSource, control.Tabs));
        tabItems.Bounds.Left.ShouldBe(0d);
        var series = control.SelectedPanel!.Series.ShouldHaveSingleItem().ShouldBeOfType<LineSeries<DateTimePoint>>();
        var points = series.Values!.OfType<DateTimePoint>().ToArray();
        points.Length.ShouldBe(2);
        points.Select(static point => point.Value).ShouldBe([0.1d, 0.25d]);
        points[0].DateTime.ShouldBeInRange(DateTime.Now.AddMinutes(-2), DateTime.Now.AddMinutes(-1));
        points[1].DateTime.ShouldBeInRange(DateTime.Now.AddMinutes(-1), DateTime.Now.AddSeconds(5));
    }

    [AvaloniaFact]
    public async Task metrics_control_projects_retained_kubernetes_node_metrics_samples()
    {
        await using var fixture = await MetricsControlFixture.CreateMetricsServerAsync();
        var control = new MetricsControl
        {
            DataContext = new V1Node { Metadata = Metadata("metrics-node") },
        };
        using var window = Application.Current.CreateTestWindow(content: control);

        window.Show();
        fixture.Initialize(control);

        await TestWait.UntilAsync(
            () => control.ShowTabs
                && !control.ShowStatus
                && control.SelectedPanel?.Series.Count > 0,
            5000,
            TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        var series = control.SelectedPanel!.Series.ShouldHaveSingleItem().ShouldBeOfType<LineSeries<DateTimePoint>>();
        var points = series.Values!.OfType<DateTimePoint>().ToArray();
        points.Length.ShouldBe(2);
        points.Select(static point => point.Value).ShouldBe([0.2d, 0.4d]);
        points[0].DateTime.ShouldBeInRange(DateTime.Now.AddMinutes(-2), DateTime.Now.AddMinutes(-1));
        points[1].DateTime.ShouldBeInRange(DateTime.Now.AddMinutes(-1), DateTime.Now.AddSeconds(5));
    }

    [AvaloniaFact]
    public async Task metrics_control_projects_retained_kubernetes_pod_container_samples()
    {
        await using var fixture = await MetricsControlFixture.CreateMetricsServerAsync();
        var pod = CreatePod();
        var container = new V1Container { Name = "app", Image = "example/app:1" };
        var control = new MetricsControl { Pod = pod, Container = container };
        using var window = Application.Current.CreateTestWindow(content: control);

        window.Show();
        fixture.Initialize(control);

        await TestWait.UntilAsync(
            () => control.ShowTabs
                && !control.ShowStatus
                && control.SelectedPanel?.Series.Count > 0,
            5000,
            TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        var series = control.SelectedPanel!.Series.ShouldHaveSingleItem().ShouldBeOfType<LineSeries<DateTimePoint>>();
        var points = series.Values!.OfType<DateTimePoint>().ToArray();
        points.Length.ShouldBe(2);
        points.Select(static point => point.Value).ShouldBe([0.1d, 0.25d]);
    }

    [AvaloniaTheory]
    [InlineData("Deployment", 0.15d, 0.4d)]
    [InlineData("StatefulSet", 0.15d, 0.4d)]
    [InlineData("DaemonSet", 0.15d, 0.4d)]
    [InlineData("ReplicaSet", 0.15d, 0.4d)]
    [InlineData("Job", 0.15d, 0.4d)]
    [InlineData("Namespace", 0.65d, 0.9d)]
    public async Task metrics_control_aggregates_kubernetes_metrics_for_workload_pods(string resourceKind, double firstCpu, double secondCpu)
    {
        await using var fixture = await MetricsControlFixture.CreateMetricsServerAsync(includeWorkloadPods: true);
        await fixture.SeedPodResourcesAsync();
        var control = new MetricsControl { DataContext = CreateWorkloadResource(resourceKind) };
        using var window = Application.Current.CreateTestWindow(content: control);

        window.Show();
        fixture.Initialize(control);

        await TestWait.UntilAsync(
            () => control.ShowTabs
                && !control.ShowStatus
                && control.SelectedPanel?.Series.Count > 0,
            5000,
            TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        var points = control.SelectedPanel!.Series
            .ShouldHaveSingleItem()
            .ShouldBeOfType<LineSeries<DateTimePoint>>()
            .Values!
            .OfType<DateTimePoint>()
            .ToArray();
        points.Length.ShouldBe(2);
        points[0].Value!.Value.ShouldBe(firstCpu, tolerance: 0.000000001);
        points[1].Value!.Value.ShouldBe(secondCpu, tolerance: 0.000000001);

        var memoryPoints = control.Tabs
            .Single(tab => tab.Title == AppResources.Metrics_Memory)
            .Panels.ShouldHaveSingleItem()
            .Series.ShouldHaveSingleItem()
            .ShouldBeOfType<LineSeries<DateTimePoint>>()
            .Values!
            .OfType<DateTimePoint>()
            .ToArray();
        var expectedMemory = resourceKind == "Namespace"
            ? new[] { 738197504d, 1006632960d }
            : new[] { 201326592d, 469762048d };
        memoryPoints.Select(static point => point.Value!.Value).ShouldBe(expectedMemory);
    }

    [AvaloniaFact]
    public async Task supported_resource_property_views_insert_metrics_control()
    {
        var views = new Control[]
        {
            new NamespacePropertiesView { DataContext = new V1Namespace { Metadata = Metadata("namespace") } },
            new NodePropertiesView { DataContext = new V1Node { Metadata = Metadata("node") } },
            new DeploymentPropertiesView { DataContext = new V1Deployment { Metadata = Metadata("deployment", "default") } },
            new StatefulSetPropertiesView { DataContext = new V1StatefulSet { Metadata = Metadata("statefulset", "default") } },
            new DaemonSetPropertiesView { DataContext = new V1DaemonSet { Metadata = Metadata("daemonset", "default") } },
            new ReplicaSetPropertiesView { DataContext = new V1ReplicaSet { Metadata = Metadata("replicaset", "default") } },
            new JobPropertiesView { DataContext = new V1Job { Metadata = Metadata("job", "default") } },
            new PersistentVolumeClaimPropertiesView { DataContext = new V1PersistentVolumeClaim { Metadata = Metadata("pvc", "default") } },
            new IngressPropertiesView { DataContext = new V1Ingress { Metadata = Metadata("ingress", "default") } },
        };

        foreach (var view in views)
        {
            using var window = Application.Current.CreateTestWindow(content: view);
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

            view.GetVisualDescendants().OfType<MetricsControl>().ShouldHaveSingleItem();
            window.Close();
            await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);
        }
    }

    [AvaloniaFact]
    public async Task pod_container_templates_bind_pod_and_container_metrics()
    {
        var container = new V1Container { Name = "app", Image = "example/app:1" };
        var pod = CreatePod();
        pod.Spec = new V1PodSpec { Containers = [container] };
        var view = new PodPropertiesView { DataContext = pod };
        using var window = Application.Current.CreateTestWindow(content: view);

        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var controls = view.GetVisualDescendants().OfType<MetricsControl>().ToArray();
        controls.Length.ShouldBe(2);
        controls.ShouldContain(control => ReferenceEquals(control.Pod, pod) && ReferenceEquals(control.Container, container));
    }

    [AvaloniaFact]
    public async Task pod_properties_keep_overall_metrics_order_and_exclude_init_containers()
    {
        var initContainer = new V1Container { Name = "init", Image = "example/init:1" };
        var container = new V1Container { Name = "app", Image = "example/app:1" };
        var pod = CreatePod();
        pod.Spec = new V1PodSpec
        {
            InitContainers = [initContainer],
            Containers = [container],
        };
        var view = new PodPropertiesView { DataContext = pod };
        using var window = Application.Current.CreateTestWindow(content: view);

        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var root = view.GetVisualChildren().OfType<StackPanel>().ShouldHaveSingleItem();
        var overallMetrics = root.Children.OfType<MetricsControl>().ShouldHaveSingleItem();
        root.Children.IndexOf(overallMetrics).ShouldBe(5);

        var controls = view.GetVisualDescendants().OfType<MetricsControl>().ToArray();
        controls.Count(control => ReferenceEquals(control.Container, initContainer)).ShouldBe(0);
        controls.Count(control => ReferenceEquals(control.Container, container)).ShouldBe(1);
    }

    private static V1Pod CreatePod() => new()
    {
        Metadata = Metadata("metrics-pod", "default"),
    };

    private static object CreateWorkloadResource(string resourceKind)
    {
        var metadata = Metadata($"metrics-{resourceKind.ToLowerInvariant()}", "default");
        var selector = new V1LabelSelector
        {
            MatchLabels = new Dictionary<string, string>(StringComparer.Ordinal) { ["app"] = "metrics-workload" },
        };

        return resourceKind switch
        {
            "Deployment" => new V1Deployment { Metadata = metadata, Spec = new V1DeploymentSpec { Selector = selector } },
            "StatefulSet" => new V1StatefulSet { Metadata = metadata, Spec = new V1StatefulSetSpec { Selector = selector } },
            "DaemonSet" => new V1DaemonSet { Metadata = metadata, Spec = new V1DaemonSetSpec { Selector = selector } },
            "ReplicaSet" => new V1ReplicaSet { Metadata = metadata, Spec = new V1ReplicaSetSpec { Selector = selector } },
            "Job" => new V1Job { Metadata = metadata, Spec = new V1JobSpec { Selector = selector } },
            "Namespace" => new V1Namespace { Metadata = Metadata("default") },
            _ => throw new ArgumentOutOfRangeException(nameof(resourceKind), resourceKind, "Unsupported metrics test resource."),
        };
    }

    private static PrometheusClientQueryRangeResponse CreateSuccessResponse(DateTimeOffset firstTimestamp, double firstValue, double secondValue)
    {
        return new PrometheusClientQueryRangeResponse
        {
            Status = "success",
            Data = new PrometheusClientQueryRangeResponse.DataObject
            {
                ResultType = "matrix",
                Result =
                [
                    new PrometheusClientQueryRangeResponse.ResultObject
                    {
                        Metric = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["pod"] = "metrics-pod",
                        },
                        Values =
                        [
                            (firstTimestamp, firstValue),
                            (firstTimestamp.AddMinutes(1), secondValue),
                        ],
                    },
                ],
            },
        };
    }

    private static PrometheusClientQueryRangeResponse CreateEmptyResponse() => new()
    {
        Status = "success",
        Data = new PrometheusClientQueryRangeResponse.DataObject
        {
            ResultType = "matrix",
            Result = [],
        },
    };

    private static V1ObjectMeta Metadata(string name, string? @namespace = null) => new()
    {
        Name = name,
        NamespaceProperty = @namespace,
    };

    private sealed class FakePrometheusQueryClient : IPrometheusQueryClient
    {
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool WaitForRelease { get; init; }

        public int QueryCalls { get; private set; }

        public Exception? ExceptionToThrow { get; init; }

        public Func<string, PrometheusClientQueryRangeResponse?>? ResponseFactory { get; set; }

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
            QueryCalls++;
            if (WaitForRelease)
            {
                await _release.Task.WaitAsync(cancellationToken);
            }

            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            return ResponseFactory?.Invoke(query);
        }

        public Task ResetAsync()
        {
            _release.TrySetResult();
            return Task.CompletedTask;
        }

        public void Release() => _release.TrySetResult();
    }

    private sealed class MetricsControlFixture : IAsyncDisposable
    {
        private readonly MetricsService _metricsService;
        private readonly Cluster _cluster;
        private readonly IUiRefreshClock _refreshClock;
        private readonly IDisposable? _ownedTransport;

        private MetricsControlFixture(
            Cluster cluster,
            ClusterWorkspace workspace,
            MetricsService metricsService,
            IUiRefreshClock refreshClock,
            IDisposable? ownedTransport = null)
        {
            _cluster = cluster;
            Workspace = workspace;
            _metricsService = metricsService;
            _refreshClock = refreshClock;
            _ownedTransport = ownedTransport;
        }

        public ClusterWorkspace Workspace { get; }

        public static async Task<MetricsControlFixture> CreateAsync(bool initializePrometheus, FakePrometheusQueryClient? queryClient = null)
        {
            var services = Application.Current.GetTestServices();
            var settings = new MetricsSettingsStore();
            var metricsService = new MetricsService(
                NullLogger<MetricsService>.Instance,
                settings,
                [new ExternalPrometheusProvider()],
                queryClient ?? new FakePrometheusQueryClient());
            var cluster = new Cluster(
                NullLogger<Cluster>.Instance,
                services.GetRequiredService<ILoggerFactory>(),
                new ClusterModelCatalog(services.GetRequiredService<KubernetesModelCatalog>()),
                settings,
                services,
                new ImmediateThreadDispatcher(),
                metricsService)
            {
                Name = "metrics-control-cluster",
                Client = new k8s.Kubernetes(new KubernetesClientConfiguration { Host = "http://localhost" }),
            };

            if (initializePrometheus)
            {
                settings.MetricsSettings.MetricsServiceType = MetricsServiceType.Prometheus;
                settings.MetricsSettings.PrometheusProviderKind = PrometheusProviderKind.External;
                settings.MetricsSettings.PrometheusDirectUrl = "http://prometheus.example";
                await metricsService.InitializeAsync(cluster);
            }

            var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(services, cluster);
            return new MetricsControlFixture(
                cluster,
                workspace,
                metricsService,
                services.GetRequiredService<IUiRefreshClock>());
        }

        public static async Task<MetricsControlFixture> CreateMetricsServerAsync(bool includeWorkloadPods = false)
        {
            var services = Application.Current.GetTestServices();
            var settings = new MetricsSettingsStore();
            var transport = new FakeKubernetesHttpApi();
            if (includeWorkloadPods)
            {
                transport.Add(new V1Pod
                {
                    Metadata = new V1ObjectMeta
                    {
                        Name = "metrics-pod",
                        NamespaceProperty = "default",
                        Labels = new Dictionary<string, string>(StringComparer.Ordinal) { ["app"] = "metrics-workload" },
                    },
                });
                transport.Add(new V1Pod
                {
                    Metadata = new V1ObjectMeta
                    {
                        Name = "metrics-pod-2",
                        NamespaceProperty = "default",
                        Labels = new Dictionary<string, string>(StringComparer.Ordinal) { ["app"] = "metrics-workload" },
                    },
                });
                transport.Add(new V1Pod
                {
                    Metadata = new V1ObjectMeta
                    {
                        Name = "metrics-pod-other",
                        NamespaceProperty = "default",
                        Labels = new Dictionary<string, string>(StringComparer.Ordinal) { ["app"] = "other" },
                    },
                });
            }
            var metricsService = new MetricsService(
                NullLogger<MetricsService>.Instance,
                settings,
                [new ExternalPrometheusProvider()],
                new FakePrometheusQueryClient());
            var cluster = new Cluster(
                NullLogger<Cluster>.Instance,
                services.GetRequiredService<ILoggerFactory>(),
                new ClusterModelCatalog(services.GetRequiredService<KubernetesModelCatalog>()),
                settings,
                services,
                new ImmediateThreadDispatcher(),
                metricsService)
            {
                Name = "metrics-server-control-cluster",
                Client = new k8s.Kubernetes(
                    new KubernetesClientConfiguration { Host = "http://fake-kubernetes" },
                    new MetricsServerHandler(transport)),
            };

            settings.MetricsSettings.MetricsServiceType = MetricsServiceType.KubernetesMetricsServer;
            await metricsService.InitializeAsync(cluster);

            var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(services, cluster);
            return new MetricsControlFixture(
                cluster,
                workspace,
                metricsService,
                services.GetRequiredService<IUiRefreshClock>(),
                transport);
        }

        public MetricsControl CreateControl(V1Pod pod)
        {
            return new MetricsControl
            {
                DataContext = pod,
            };
        }

        public void Initialize(MetricsControl control)
        {
            control.Initialize(Workspace);
        }

        public async Task SeedPodResourcesAsync()
        {
            await _cluster.UpdateCanI<V1Pod>(Verb.List);
            await _cluster.UpdateCanI<V1Pod>(Verb.Watch);
            await _cluster.SeedResource<V1Pod>(true, TestContext.Current.CancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            Workspace.Dispose();
            await _cluster.DisposeAsync();
            _metricsService.Dispose();
            _ownedTransport?.Dispose();
            _ = _refreshClock;
        }
    }

    private sealed class MetricsSettingsStore : IClusterSettingsStore
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

    private sealed class MetricsServerHandler(FakeKubernetesHttpApi innerHandler) : DelegatingHandler(innerHandler)
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath.TrimEnd('/');
            if (path == "/apis")
            {
                return Task.FromResult(JsonResponse(
                    request,
                    """{"apiVersion":"v1","kind":"APIGroupList","groups":[{"name":"metrics.k8s.io","versions":[{"groupVersion":"metrics.k8s.io/v1beta1","version":"v1beta1"}],"preferredVersion":{"groupVersion":"metrics.k8s.io/v1beta1","version":"v1beta1"}}]}"""));
            }

            if (path == "/apis/metrics.k8s.io/v1beta1/nodes")
            {
                var now = DateTime.UtcNow;
                return Task.FromResult(JsonResponse(
                    request,
                    JsonSerializer.Serialize(new
                    {
                        apiVersion = "metrics.k8s.io/v1beta1",
                        kind = "NodeMetricsList",
                        items = new[]
                        {
                            new
                            {
                                metadata = new { name = "metrics-node" },
                                timestamp = now.AddMinutes(-1),
                                window = "30s",
                                usage = new Dictionary<string, string> { ["cpu"] = "200m", ["memory"] = "256Mi" },
                            },
                            new
                            {
                                metadata = new { name = "metrics-node" },
                                timestamp = now,
                                window = "30s",
                                usage = new Dictionary<string, string> { ["cpu"] = "400m", ["memory"] = "512Mi" },
                            },
                        },
                    })));
            }

            if (path == "/apis/metrics.k8s.io/v1beta1/pods")
            {
                var now = DateTime.UtcNow;
                return Task.FromResult(JsonResponse(
                    request,
                    JsonSerializer.Serialize(new
                    {
                        apiVersion = "metrics.k8s.io/v1beta1",
                        kind = "PodMetricsList",
                        items = new[]
                        {
                            CreatePodMetricsItem("metrics-pod", now.AddMinutes(-1), "100m", "128Mi"),
                            CreatePodMetricsItem("metrics-pod", now, "250m", "256Mi"),
                            CreatePodMetricsItem("metrics-pod-2", now.AddMinutes(-1), "50m", "64Mi"),
                            CreatePodMetricsItem("metrics-pod-2", now, "150m", "192Mi"),
                            CreatePodMetricsItem("metrics-pod-other", now.AddMinutes(-1), "500m", "512Mi"),
                            CreatePodMetricsItem("metrics-pod-other", now, "500m", "512Mi"),
                        },
                    })));
            }

            static object CreatePodMetricsItem(string name, DateTime timestamp, string cpu, string memory)
            {
                return new
                {
                    metadata = new { name, @namespace = "default" },
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
                };
            }

            return base.SendAsync(request, cancellationToken);
        }

        private static HttpResponseMessage JsonResponse(HttpRequestMessage request, string json)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };
        }
    }
}
