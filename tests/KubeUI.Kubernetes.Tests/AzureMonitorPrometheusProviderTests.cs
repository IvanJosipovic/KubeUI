using System.Text.Json;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public sealed class AzureMonitorPrometheusProviderTests
{
    [Fact]
    public void Cluster_metrics_settings_persist_static_authentication_credentials()
    {
        var serialized = JsonSerializer.Serialize(new ClusterMetricsSettings
        {
            PrometheusCustomHeaders = [new PrometheusHeader { Name = "Authorization", Value = "Bearer secret-token" }],
            PrometheusBasicAuthUsername = "metrics-user",
            PrometheusBasicAuthPassword = "secret-password",
        }, MetricsTestJsonContext.Default.ClusterMetricsSettings);

        serialized.ShouldContain("secret-token");
        serialized.ShouldContain("secret-password");
        serialized.ShouldContain("PrometheusCustomHeaders");
        serialized.ShouldContain("PrometheusBasicAuthPassword");

        var restored = JsonSerializer.Deserialize(serialized, MetricsTestJsonContext.Default.ClusterMetricsSettings);
        restored.ShouldNotBeNull();
        restored.PrometheusCustomHeaders.ShouldHaveSingleItem().Value.ShouldBe("Bearer secret-token");
        restored.PrometheusBasicAuthPassword.ShouldBe("secret-password");
        restored.PrometheusRequireHttpsForAuthentication.ShouldBeTrue();
    }

    [Fact]
    public async Task TryResolveServiceAsync_uses_selected_workspace_endpoint_and_azure_auth()
    {
        var provider = new AzureMonitorPrometheusProvider();
        var settings = new ClusterMetricsSettings
        {
            AzureMonitorWorkspaceId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Monitor/accounts/amw",
            AzureMonitorQueryEndpoint = "https://amw.eastus.prometheus.monitor.azure.com",
        };

        var endpoint = await provider.TryResolveServiceAsync(null!, settings);

        endpoint.ShouldNotBeNull();
        endpoint.ProviderKind.ShouldBe(PrometheusProviderKind.AzureMonitor);
        endpoint.DirectUrl.ShouldBe("https://amw.eastus.prometheus.monitor.azure.com");
        endpoint.UseAzureMonitorAuthentication.ShouldBeTrue();
        endpoint.CustomHeaders.ShouldBeNull();
    }

    [Theory]
    [InlineData(null, "https://amw.eastus.prometheus.monitor.azure.com")]
    [InlineData("/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Monitor/accounts/amw", "not a url")]
    [InlineData("/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Monitor/accounts/amw", "http://amw.eastus.prometheus.monitor.azure.com")]
    public async Task TryResolveServiceAsync_returns_null_without_valid_workspace_settings(string? workspaceId, string? endpointUrl)
    {
        var provider = new AzureMonitorPrometheusProvider();
        var settings = new ClusterMetricsSettings
        {
            AzureMonitorWorkspaceId = workspaceId,
            AzureMonitorQueryEndpoint = endpointUrl,
        };

        var endpoint = await provider.TryResolveServiceAsync(null!, settings);

        endpoint.ShouldBeNull();
    }
}
