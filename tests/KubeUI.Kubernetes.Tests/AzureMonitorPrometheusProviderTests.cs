using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public sealed class AzureMonitorPrometheusProviderTests
{
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
