using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Avalonia.Resources.Core.v1.Namespace;
using KubeUI.Avalonia.Resources.Core.v1.Node;
using KubeUI.Avalonia.Resources.Network.v1.Ingress;
using KubeUI.Avalonia.Resources.Storage.v1.PersistentVolumeClaim;
using KubeUI.Avalonia.Resources.Workloads.v1.DaemonSet;
using KubeUI.Avalonia.Resources.Workloads.v1.Deployment;
using KubeUI.Avalonia.Resources.Workloads.v1.Job;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod;
using KubeUI.Avalonia.Resources.Workloads.v1.ReplicaSet;
using KubeUI.Avalonia.Resources.Workloads.v1.StatefulSet;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
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

        var grid = control.Content.ShouldBeOfType<Grid>();
        grid.Children.OfType<Grid>().Single().Children.OfType<ComboBox>().ShouldHaveSingleItem();
        grid.Children.OfType<Border>().ShouldHaveSingleItem();
        grid.Children.OfType<TextBlock>().ShouldHaveSingleItem();
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

        var buttons = control.GetVisualDescendants().OfType<ToggleButton>().ToArray();
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

    private static V1Pod CreatePod() => new()
    {
        Metadata = Metadata("metrics-pod", "default"),
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

        public Exception? ExceptionToThrow { get; init; }

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
            if (WaitForRelease)
            {
                await _release.Task.WaitAsync(cancellationToken);
            }

            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            return null;
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

        private MetricsControlFixture(Cluster cluster, ClusterWorkspace workspace, MetricsService metricsService, IUiRefreshClock refreshClock)
        {
            _cluster = cluster;
            Workspace = workspace;
            _metricsService = metricsService;
            _refreshClock = refreshClock;
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

        public async ValueTask DisposeAsync()
        {
            Workspace.Dispose();
            await _cluster.DisposeAsync();
            _metricsService.Dispose();
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
}
