using Avalonia.Headless.XUnit;
using Shouldly;

namespace KubeUI.Features.Clusters.Tests.Settings;

public sealed class ClusterSettingsViewModelTests
{
    [AvaloniaFact]
    public async Task cluster_settings_default_debug_container_image_is_busybox()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var settings = services.GetRequiredService<ISettingsService>();
        settings.Settings.GetClusterSettings(workspace.Runtime).DebugContainerImage.ShouldBe(ClusterSettings.DefaultDebugContainerImage);
    }

    [AvaloniaFact]
    public async Task changing_debug_container_image_updates_persisted_cluster_settings()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var viewModel = services.GetRequiredService<ClusterSettingsViewModel>();

        viewModel.Initialize(workspace);
        await TestApplicationExtensions.WaitForUiAsync();

        viewModel.DebugContainerImage = "example.com/debug:1";

        services.GetRequiredService<ISettingsService>()
            .Settings
            .GetClusterSettings(workspace.Runtime)
            .DebugContainerImage
            .ShouldBe("example.com/debug:1");
    }

    [AvaloniaFact]
    public async Task metrics_settings_changes_are_debounced_before_persistence()
    {
        var settings = new Moq.Mock<IClusterPreferenceStore>();
        var azureMonitor = new Moq.Mock<IAzureMonitorWorkspaceService>();
        using var viewModel = new ClusterSettingsViewModel(settings.Object, azureMonitor.Object)
        {
            ClusterSettings = new ClusterSettings(),
        };

        viewModel.ClusterSettings.MetricsSettings.PrometheusDirectUrl = "http://prometheus.example";
        viewModel.ClusterSettings.MetricsSettings.PrometheusDirectUrl = "https://prometheus.example";

        settings.Verify(store => store.SaveSettings(), Moq.Times.Never);
        await TestWait.UntilAsync(
            () => settings.Invocations.Count(invocation => invocation.Method.Name == nameof(IClusterPreferenceStore.SaveSettings)) == 1,
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken);

        settings.Verify(store => store.SaveSettings(), Moq.Times.Once);
    }

    [AvaloniaFact]
    public async Task custom_header_edits_are_persisted_after_debounce()
    {
        var settings = new Moq.Mock<IClusterPreferenceStore>();
        var azureMonitor = new Moq.Mock<IAzureMonitorWorkspaceService>();
        using var viewModel = new ClusterSettingsViewModel(settings.Object, azureMonitor.Object)
        {
            ClusterSettings = new ClusterSettings(),
        };
        var header = new PrometheusHeader { Name = "X-Api-Key", Value = "first" };

        viewModel.ClusterSettings.MetricsSettings.PrometheusCustomHeaders.Add(header);
        header.Value = "updated-secret";

        await TestWait.UntilAsync(
            () => settings.Invocations.Count(invocation => invocation.Method.Name == nameof(IClusterPreferenceStore.SaveSettings)) == 1,
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken);

        settings.Verify(store => store.SaveSettings(), Moq.Times.Once);
    }

    [AvaloniaFact]
    public void insecure_authentication_warning_tracks_endpoint_scheme()
    {
        var settings = new Moq.Mock<IClusterPreferenceStore>();
        var azureMonitor = new Moq.Mock<IAzureMonitorWorkspaceService>();
        using var viewModel = new ClusterSettingsViewModel(settings.Object, azureMonitor.Object)
        {
            ClusterSettings = new ClusterSettings
            {
                MetricsSettings = new ClusterMetricsSettings
                {
                    MetricsServiceType = MetricsServiceType.Prometheus,
                    PrometheusDirectUrl = "http://prometheus.example",
                    PrometheusCustomHeaders = [new PrometheusHeader { Name = "X-Api-Key", Value = "secret" }],
                },
            },
        };

        viewModel.ShowInsecureAuthenticationWarning.ShouldBeTrue();
        viewModel.ClusterSettings.MetricsSettings.PrometheusDirectUrl = "https://prometheus.example";
        viewModel.ShowInsecureAuthenticationWarning.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void authentication_conflict_warning_tracks_basic_auth_and_authorization_header()
    {
        var settings = new Moq.Mock<IClusterPreferenceStore>();
        var azureMonitor = new Moq.Mock<IAzureMonitorWorkspaceService>();
        var authorizationHeader = new PrometheusHeader { Name = "authorization", Value = "Bearer token" };
        using var viewModel = new ClusterSettingsViewModel(settings.Object, azureMonitor.Object)
        {
            ClusterSettings = new ClusterSettings
            {
                MetricsSettings = new ClusterMetricsSettings
                {
                    MetricsServiceType = MetricsServiceType.Prometheus,
                    PrometheusBasicAuthUsername = "user",
                    PrometheusBasicAuthPassword = "password",
                    PrometheusCustomHeaders = [authorizationHeader],
                },
            },
        };

        viewModel.ShowPrometheusAuthenticationConflictWarning.ShouldBeTrue();

        authorizationHeader.Name = "X-Api-Key";
        viewModel.ShowPrometheusAuthenticationConflictWarning.ShouldBeFalse();

        authorizationHeader.Name = "Authorization";
        viewModel.ClusterSettings.MetricsSettings.PrometheusBasicAuthUsername = null;
        viewModel.ClusterSettings.MetricsSettings.PrometheusBasicAuthPassword = null;
        viewModel.ShowPrometheusAuthenticationConflictWarning.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void manual_prometheus_direct_url_selects_direct_endpoint_mode()
    {
        var settings = new Moq.Mock<IClusterPreferenceStore>();
        var azureMonitor = new Moq.Mock<IAzureMonitorWorkspaceService>();
        using var viewModel = new ClusterSettingsViewModel(settings.Object, azureMonitor.Object)
        {
            ClusterSettings = new ClusterSettings
            {
                MetricsSettings = new ClusterMetricsSettings
                {
                    MetricsServiceType = MetricsServiceType.Prometheus,
                    PrometheusProviderKind = PrometheusProviderKind.Manual,
                    PrometheusDirectUrl = "https://prometheus.example",
                },
            },
        };

        viewModel.ShowPrometheusServiceSettings.ShouldBeFalse();
        viewModel.ShowPrometheusDirectUrlSettings.ShouldBeTrue();

        viewModel.ClusterSettings.MetricsSettings.PrometheusDirectUrl = null;

        viewModel.ShowPrometheusServiceSettings.ShouldBeTrue();
        viewModel.ShowPrometheusDirectUrlSettings.ShouldBeTrue();
    }
}
