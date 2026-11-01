using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Metrics;

public sealed class LivePropertiesMetricsTests
{
    [LiveClusterFact("KUBEUI_LIVE_METRICS_POD")]
    public async Task pod_properties_display_prometheus_charts()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(90));
        var context = Environment.GetEnvironmentVariable(LiveClusterFactAttribute.ContextVariable)
            ?? throw new InvalidOperationException("Set KUBEUI_LIVE_CONTEXT to the cluster context.");
        var podName = Environment.GetEnvironmentVariable("KUBEUI_LIVE_METRICS_POD")!;
        var podNamespace = Environment.GetEnvironmentVariable("KUBEUI_LIVE_METRICS_NAMESPACE") ?? "default";
        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = KubernetesBackend.Live;
            config.Name = context;
        });
        workspace.Runtime.ActiveMetricsBackend.Type.ShouldBe(MetricsServiceType.Prometheus);
        var pod = await workspace.Runtime.Client!.CoreV1.ReadNamespacedPodAsync(podName, podNamespace, cancellationToken: timeout.Token);
        await AssertChartsAsync(workspace, pod, timeout.Token);
    }

    private static async Task AssertChartsAsync<T>(ClusterWorkspace workspace, T resource, CancellationToken cancellationToken)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var viewModel = Application.Current.GetRequiredTestService<ResourcePropertiesViewModel<T>>();
        viewModel.Initialize(workspace, resource);
        var view = new ResourcePropertiesView<T> { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(cancellationToken);
        var control = view.GetVisualDescendants().OfType<MetricsControl>().First();
        try
        {
            await TestWait.UntilAsync(() => control.Tabs.Any(tab => tab.Panels.Count > 0), TimeSpan.FromSeconds(45), cancellationToken: cancellationToken, beforePoll: () => Dispatcher.UIThread.RunJobs());
        }
        catch (TimeoutException)
        {
            Assert.Fail($"Backend={workspace.Runtime.ActiveMetricsBackend}; status={control.StatusText}; tabs={control.Tabs.Count}");
        }
        control.ShowTabs.ShouldBeTrue();
        control.SelectedPanel.ShouldNotBeNull();
        var lines = control.SelectedPanel.Series.OfType<StepLineSeries<DateTimePoint>>().ToArray();
        lines.ShouldNotBeEmpty();
        lines.ShouldContain(line => line.Values!.Any(point => point.Value.HasValue));
        var chart = control.GetVisualDescendants().OfType<CartesianChart>().Single();
        chart.IsVisible.ShouldBeTrue();
        chart.Bounds.Width.ShouldBeGreaterThan(0);
        chart.Bounds.Height.ShouldBeGreaterThan(0);
        var memory = control.GetVisualDescendants().OfType<ToggleButton>().Single(button => Equals(ToolTip.GetTip(button), "Memory"));
        memory.Command!.Execute(null);
        try
        {
            await TestWait.UntilAsync(() => control.SelectedTab?.Title == "Memory" && control.SelectedPanel?.Series.Count > 0 && !control.ShowStatus,
                TimeSpan.FromSeconds(20), cancellationToken: cancellationToken, beforePoll: () => Dispatcher.UIThread.RunJobs());
        }
        catch (TimeoutException)
        {
            Assert.Fail($"Selected={control.SelectedTab?.Title}; panels={control.SelectedTab?.Panels.Count}; series={control.SelectedPanel?.Series.Count}; showStatus={control.ShowStatus}; status={control.StatusText}");
        }
        control.StatusText.ShouldBeNull();
    }

}
