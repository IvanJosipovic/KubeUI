using System.Text;
using Azure.Core;
using Azure.Identity;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public sealed class AzureMonitorWorkspaceServiceTests
{
    [Fact]
    public async Task GetAuthenticationStatusAsync_reports_cli_identity_claims()
    {
        var token = CreateToken("{\"preferred_username\":\"dev@example.com\",\"tid\":\"tenant-id\"}");
        var credential = new FakeTokenCredential(token);
        var service = new AzureMonitorWorkspaceService(credential);

        var status = await service.GetAuthenticationStatusAsync();

        status.AzureCliSignedIn.ShouldBeTrue();
        status.Username.ShouldBe("dev@example.com");
        status.TenantId.ShouldBe("tenant-id");
        credential.RequestedScopes.ShouldBe(["https://management.azure.com/.default"]);
    }

    [Fact]
    public async Task GetAuthenticationStatusAsync_reports_missing_cli_credentials()
    {
        var service = new AzureMonitorWorkspaceService(new FakeTokenCredential("unused", new CredentialUnavailableException("az login required")));

        var status = await service.GetAuthenticationStatusAsync();

        status.AzureCliSignedIn.ShouldBeFalse();
        status.Username.ShouldBeNull();
    }

    [Fact]
    public async Task GetPrometheusAccessTokenAsync_requests_prometheus_audience()
    {
        var credential = new FakeTokenCredential("prometheus-token");
        var service = new AzureMonitorWorkspaceService(credential);

        var token = await service.GetPrometheusAccessTokenAsync();

        token.Token.ShouldBe("prometheus-token");
        credential.RequestedScopes.ShouldBe(["https://prometheus.monitor.azure.com/.default"]);
    }

    private static string CreateToken(string json)
    {
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return $"header.{payload}.signature";
    }

    private sealed class FakeTokenCredential(string token, Exception? exception = null) : TokenCredential
    {
        public List<string> RequestedScopes { get; } = [];

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            RequestedScopes.AddRange(requestContext.Scopes);
            if (exception is not null)
            {
                throw exception;
            }

            return new AccessToken(token, DateTimeOffset.UtcNow.AddHours(1));
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(GetToken(requestContext, cancellationToken));
        }
    }
}
