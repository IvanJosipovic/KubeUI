using Shouldly;
using System.Text.Json;

namespace KubeUI.Kubernetes.Tests;

public sealed class AzureMonitorPrometheusProviderTests
{
    [Fact]
    public void Cluster_metrics_settings_do_not_serialize_bearer_token()
    {
        var serialized = JsonSerializer.Serialize(new ClusterMetricsSettings { PrometheusBearerToken = "secret-token" });

        serialized.ShouldNotContain("secret-token");
        serialized.ShouldNotContain("PrometheusBearerToken");
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
        endpoint.BearerToken.ShouldBeNull();
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
