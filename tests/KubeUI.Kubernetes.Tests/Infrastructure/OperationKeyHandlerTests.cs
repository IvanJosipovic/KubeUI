using System.Net;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Infrastructure;

public sealed class OperationKeyHandlerTests
{
    [Fact]
    public async Task forbidden_resource_response_invokes_permission_invalidation()
    {
        using var api = new FakeKubernetesHttpApi();
        api.SetPermission("pods", "list", false, "default");
        var invalidations = 0;
        using var handler = new OperationKeyHandler(() => invalidations++)
        {
            InnerHandler = api,
        };
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://fake-kubernetes"),
        };

        using var response = await client.GetAsync(
            "/api/v1/namespaces/default/pods",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        invalidations.ShouldBe(1);
    }

    [Fact]
    public async Task forbidden_authorization_review_does_not_invoke_permission_invalidation()
    {
        using var api = new FakeKubernetesHttpApi
        {
            SelfSubjectRulesReviewStatusCode = HttpStatusCode.Forbidden,
        };
        var invalidations = 0;
        using var handler = new OperationKeyHandler(() => invalidations++)
        {
            InnerHandler = api,
        };
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://fake-kubernetes"),
        };

        using var response = await client.PostAsJsonAsync(
            "/apis/authorization.k8s.io/v1/selfsubjectrulesreviews",
            new
            {
                apiVersion = "authorization.k8s.io/v1",
                kind = "SelfSubjectRulesReview",
                spec = new { @namespace = "default" },
            },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        invalidations.ShouldBe(0);
    }

    [Fact]
    public async Task successful_resource_response_does_not_invoke_permission_invalidation()
    {
        using var api = new FakeKubernetesHttpApi();
        var invalidations = 0;
        using var handler = new OperationKeyHandler(() => invalidations++)
        {
            InnerHandler = api,
        };
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://fake-kubernetes"),
        };

        using var response = await client.GetAsync(
            "/api/v1/namespaces/default/pods",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        invalidations.ShouldBe(0);
    }
}
