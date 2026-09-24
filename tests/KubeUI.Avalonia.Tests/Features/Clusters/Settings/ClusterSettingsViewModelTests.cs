using Avalonia.Headless.XUnit;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Azure.Core;
using Shouldly;
using AppResources = KubeUI.Avalonia.Assets.Resources;

namespace KubeUI.Avalonia.Tests.Features.Clusters.Settings;

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
    public async Task cluster_settings_view_renders_metrics_selectors()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var viewModel = services.GetRequiredService<ClusterSettingsViewModel>();
        viewModel.Initialize(workspace);

        var view = new ClusterSettingsView { ViewModel = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        view.GetVisualDescendants().OfType<ComboBox>().Count().ShouldBe(4);
        view.GetVisualDescendants()
            .OfType<TextBlock>()
            .ShouldContain(textBlock => textBlock.Text == viewModel.ActiveMetricsServiceText);
    }

    [AvaloniaFact]
    public async Task selecting_prometheus_provider_renders_and_persists_prometheus_settings()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var viewModel = services.GetRequiredService<ClusterSettingsViewModel>();
        viewModel.Initialize(workspace);

        var view = new ClusterSettingsView { ViewModel = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var selectors = view.GetVisualDescendants().OfType<ComboBox>().ToArray();
        selectors[0].SelectedItem = viewModel.MetricsServiceOptions.Single(option => option.Value == MetricsServiceType.Prometheus);
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        viewModel.ClusterSettings.MetricsSettings.MetricsServiceType.ShouldBe(MetricsServiceType.Prometheus);
        selectors[1].IsVisible.ShouldBeTrue();

        selectors[1].SelectedItem = viewModel.PrometheusProviderOptions.Single(option => option.Value == PrometheusProviderKind.Manual);
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        viewModel.ClusterSettings.MetricsSettings.PrometheusProviderKind.ShouldBe(PrometheusProviderKind.Manual);
        services.GetRequiredService<ISettingsService>()
            .Settings
            .GetClusterSettings(workspace.Runtime)
            .MetricsSettings
            .PrometheusProviderKind
            .ShouldBe(PrometheusProviderKind.Manual);

        var serviceNameRow = view.GetVisualDescendants()
            .OfType<Grid>()
            .Single(grid => grid.Children.OfType<Label>().Any(label => Equals(label.Content, AppResources.ClusterSettingsView_PrometheusServiceNameLabel)));
        serviceNameRow.IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task selecting_azure_monitor_provider_renders_workspace_selection()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var viewModel = services.GetRequiredService<ClusterSettingsViewModel>();
        viewModel.Initialize(workspace);
        viewModel.ClusterSettings.MetricsSettings.MetricsServiceType = MetricsServiceType.Prometheus;
        viewModel.ClusterSettings.MetricsSettings.PrometheusProviderKind = PrometheusProviderKind.AzureMonitor;

        var view = new ClusterSettingsView { ViewModel = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        viewModel.ShowAzureMonitorSettings.ShouldBeTrue();
        viewModel.ShowPrometheusServiceSettings.ShouldBeFalse();
        viewModel.ShowPrometheusDirectUrlSettings.ShouldBeFalse();
        view.GetVisualDescendants().OfType<ComboBox>().Count().ShouldBe(4);
        view.GetVisualDescendants()
            .OfType<Button>()
            .ShouldContain(button => Equals(button.Content, AppResources.ClusterSettingsView_AzureMonitorRefresh));
    }

    [AvaloniaFact]
    public async Task refreshing_azure_workspaces_selects_and_persists_workspace()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var settingsService = services.GetRequiredService<ISettingsService>();
        var azureService = new FakeAzureMonitorWorkspaceService();
        var viewModel = new ClusterSettingsViewModel(settingsService, azureService);
        viewModel.Initialize(workspace);
        viewModel.ClusterSettings.MetricsSettings.MetricsServiceType = MetricsServiceType.Prometheus;
        viewModel.ClusterSettings.MetricsSettings.PrometheusProviderKind = PrometheusProviderKind.AzureMonitor;

        await viewModel.RefreshAzureMonitorCommand.ExecuteAsync(TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(
            () => viewModel.AzureMonitorWorkspaces.Count == 1,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        viewModel.AzureMonitorStatusText.ShouldContain("dev@example.com");
        viewModel.SelectedAzureMonitorSubscription?.SubscriptionId.ShouldBe("subscription-1");
        viewModel.SelectedAzureMonitorWorkspace?.ResourceId.ShouldBe("/subscriptions/subscription-1/resourceGroups/rg/providers/Microsoft.Monitor/accounts/workspace-1");
        settingsService.Settings.GetClusterSettings(workspace.Runtime).MetricsSettings.AzureMonitorQueryEndpoint
            .ShouldBe("https://workspace-1.eastus.prometheus.monitor.azure.com");
    }

    [AvaloniaFact]
    public async Task refreshing_azure_workspaces_shows_cli_login_guidance_when_signed_out()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var viewModel = new ClusterSettingsViewModel(
            services.GetRequiredService<ISettingsService>(),
            new FakeAzureMonitorWorkspaceService(azureCliSignedIn: false));
        viewModel.Initialize(workspace);

        await viewModel.RefreshAzureMonitorCommand.ExecuteAsync(TestContext.Current.CancellationToken);

        viewModel.AzureMonitorStatusText.ShouldBe(AppResources.ClusterSettingsView_AzureMonitorNotSignedIn);
        viewModel.AzureMonitorSubscriptions.ShouldBeEmpty();
    }

    private sealed class FakeAzureMonitorWorkspaceService(bool azureCliSignedIn = true) : IAzureMonitorWorkspaceService
    {
        public Task<AzureAuthenticationStatus> GetAuthenticationStatusAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new AzureAuthenticationStatus { AzureCliSignedIn = azureCliSignedIn, Username = "dev@example.com", TenantId = "tenant-1" });

        public Task<IReadOnlyList<AzureMonitorSubscriptionInfo>> GetSubscriptionsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AzureMonitorSubscriptionInfo>>([
                new AzureMonitorSubscriptionInfo { SubscriptionId = "subscription-1", DisplayName = "Subscription 1" },
            ]);

        public Task<IReadOnlyList<AzureMonitorWorkspaceInfo>> GetWorkspacesAsync(string subscriptionId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AzureMonitorWorkspaceInfo>>([
                new AzureMonitorWorkspaceInfo
                {
                    ResourceId = "/subscriptions/subscription-1/resourceGroups/rg/providers/Microsoft.Monitor/accounts/workspace-1",
                    Name = "workspace-1",
                    Location = "eastus",
                    QueryEndpoint = "https://workspace-1.eastus.prometheus.monitor.azure.com",
                },
            ]);

        public ValueTask<AccessToken> GetPrometheusAccessTokenAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(new AccessToken("test-token", DateTimeOffset.UtcNow.AddMinutes(5)));
    }
}
