using System.Net;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Infrastructure;

public sealed class ClusterHttpResilienceTests
{
    [Theory]
    [InlineData("/api/v1/namespaces?watch=true&allowWatchBookmarks=true")]
    [InlineData("/api/v1/namespaces?allowWatchBookmarks=true&resourceVersion=1&watch=true")]
    public async Task watch_request_is_not_retried_on_transient_failure(string pathAndQuery)
    {
        var attempts = await SendThroughClusterRetryAsync(
            pathAndQuery,
            TestContext.Current.CancellationToken);

        attempts.ShouldBe(1);
    }

    [Fact]
    public async Task unary_request_is_retried_on_transient_failure()
    {
        var attempts = await SendThroughClusterRetryAsync(
            "/apis/authorization.k8s.io/v1/selfsubjectaccessreviews",
            TestContext.Current.CancellationToken);

        attempts.ShouldBe(4);
    }

    [Fact]
    public async Task request_without_operation_key_is_retried_on_transient_failure()
    {
        var attempts = await SendThroughClusterRetryAsync(
            "/api/v1/namespaces",
            TestContext.Current.CancellationToken,
            setOperationKey: false);

        attempts.ShouldBe(4);
    }

    private static async Task<int> SendThroughClusterRetryAsync(
        string pathAndQuery,
        CancellationToken cancellationToken,
        bool setOperationKey = true)
    {
        var attempts = 0;
        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(ClusterHttpResilience.CreateRetryOptions())
            .Build();

        using var terminalHandler = new CountingHandler(() => attempts++);
        using var resilienceHandler = new ResilienceHandler(pipeline)
        {
            InnerHandler = terminalHandler,
        };
        using var client = new HttpClient(resilienceHandler);
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(new Uri("http://fake-kubernetes"), pathAndQuery));
        ResilienceContext? context = null;
        if (setOperationKey)
        {
            context = ResilienceContextPool.Shared.Get(
                $"{request.Method}:{request.RequestUri!.PathAndQuery}",
                cancellationToken);
            request.SetResilienceContext(context);
        }

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        }
        finally
        {
            if (context is not null)
            {
                ResilienceContextPool.Shared.Return(context);
            }
        }

        return attempts;
    }

    private sealed class CountingHandler(Action onSend) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            onSend();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        }
    }
}
