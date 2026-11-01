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
        using var service = new AzureMonitorWorkspaceService(credential);

        var status = await service.GetAuthenticationStatusAsync();

        status.AzureCliSignedIn.ShouldBeTrue();
        status.Username.ShouldBe("dev@example.com");
        status.TenantId.ShouldBe("tenant-id");
        credential.RequestedScopes.ShouldBe(["https://management.azure.com/.default"]);
    }

    [Fact]
    public async Task GetAuthenticationStatusAsync_reports_missing_cli_credentials()
    {
        using var service = new AzureMonitorWorkspaceService(new FakeTokenCredential("unused", new CredentialUnavailableException("az login required")));

        var status = await service.GetAuthenticationStatusAsync();

        status.AzureCliSignedIn.ShouldBeFalse();
        status.Username.ShouldBeNull();
    }

    [Fact]
    public async Task GetPrometheusAccessTokenAsync_requests_prometheus_audience()
    {
        var credential = new FakeTokenCredential("prometheus-token");
        using var service = new AzureMonitorWorkspaceService(credential);

        var token = await service.GetPrometheusAccessTokenAsync();

        token.Token.ShouldBe("prometheus-token");
        credential.RequestedScopes.ShouldBe(["https://prometheus.monitor.azure.com/.default"]);
    }

    [Fact]
    public async Task GetPrometheusAccessTokenAsync_reuses_unexpired_token()
    {
        var credential = new FakeTokenCredential("prometheus-token");
        using var service = new AzureMonitorWorkspaceService(credential);

        var firstToken = await service.GetPrometheusAccessTokenAsync();
        var secondToken = await service.GetPrometheusAccessTokenAsync();

        secondToken.ShouldBe(firstToken);
        credential.TokenRequestCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetPrometheusAccessTokenAsync_coalesces_concurrent_token_requests()
    {
        var acquisitionStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseAcquisition = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var credential = new FakeTokenCredential(
            "prometheus-token",
            acquisitionStarted: acquisitionStarted,
            releaseAcquisition: releaseAcquisition);
        using var service = new AzureMonitorWorkspaceService(credential);

        var firstRequest = service.GetPrometheusAccessTokenAsync().AsTask();
        await acquisitionStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        var concurrentRequests = Enumerable.Range(0, 7)
            .Select(_ => service.GetPrometheusAccessTokenAsync().AsTask())
            .Prepend(firstRequest)
            .ToArray();
        releaseAcquisition.SetResult();

        var tokens = await Task.WhenAll(concurrentRequests);

        tokens.Select(static token => token.Token).Distinct().ToArray().ShouldBe(["prometheus-token"]);
        credential.TokenRequestCount.ShouldBe(1);
    }

    private static string CreateToken(string json)
    {
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return $"header.{payload}.signature";
    }

    private sealed class FakeTokenCredential(
        string token,
        Exception? exception = null,
        TaskCompletionSource? acquisitionStarted = null,
        TaskCompletionSource? releaseAcquisition = null) : TokenCredential
    {
        public List<string> RequestedScopes { get; } = [];
        private int _tokenRequestCount;

        public int TokenRequestCount => Volatile.Read(ref _tokenRequestCount);

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            RequestedScopes.AddRange(requestContext.Scopes);
            Interlocked.Increment(ref _tokenRequestCount);
            if (exception is not null)
            {
                throw exception;
            }

            return new AccessToken(token, DateTimeOffset.UtcNow.AddHours(1));
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return GetTokenAsyncCore(requestContext, cancellationToken);
        }

        private async ValueTask<AccessToken> GetTokenAsyncCore(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            acquisitionStarted?.TrySetResult();
            if (releaseAcquisition is not null)
            {
                await releaseAcquisition.Task.WaitAsync(cancellationToken);
            }

            return GetToken(requestContext, cancellationToken);
        }
    }
}
