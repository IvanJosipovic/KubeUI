using k8s;
using k8s.Models;
using KubeUI.Testing;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace KubeUI.Kubernetes.Tests.Infra;

public sealed class FakeKubernetesHttpApiTests
{
    [Fact]
    public async Task RealClientUsesFakeHttpTransportForCrud()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Register<V1Namespace>();

        using var client = new k8s.Kubernetes(
            new KubernetesClientConfiguration { Host = "http://fake-kubernetes" },
            api);

        var created = await client.CreateNamespaceAsync(new V1Namespace
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta { Name = "test" },
        }, cancellationToken: TestContext.Current.CancellationToken);

        var listed = await client.ListNamespaceAsync(cancellationToken: TestContext.Current.CancellationToken);

        created.Name().ShouldBe("test");
        listed.Items.ShouldContain(item => item.Name() == "test");
        api.RequestUris.ShouldContain(uri => uri != null && uri.AbsolutePath == "/api/v1/namespaces");
    }

    [Fact]
    public async Task GenericClientUsesFakeHttpTransportForCrud()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Register<V1Namespace>();
        using var client = new k8s.Kubernetes(new KubernetesClientConfiguration { Host = "http://fake-kubernetes" }, api);
        using var generic = client.GetGenericClient<V1Namespace>();

#pragma warning disable xUnit1051 // GenericClient.CreateAsync does not expose the token parameter by name in this client version.
        var created = await generic.CreateAsync(new V1Namespace
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta { Name = "test" },
        });
#pragma warning restore xUnit1051

        created.Name().ShouldBe("test");
    }

    [Fact]
    public async Task ResourceRequestReturnsForbiddenWhenPermissionIsDenied()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Register<V1Namespace>();
        api.SetPermission("namespaces", "create", false);

        using var client = new HttpClient(api)
        {
            BaseAddress = new Uri("http://fake-kubernetes"),
        };

        using var response = await client.PostAsJsonAsync(
            "/api/v1/namespaces",
            new V1Namespace
            {
                ApiVersion = V1Namespace.KubeApiVersion,
                Kind = V1Namespace.KubeKind,
                Metadata = new V1ObjectMeta { Name = "denied" },
            },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task NamespacedResourceRequestUsesNamespacePermission()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Register<V1Pod>();
        api.SetPermission("pods", "list", false, "restricted");

        using var client = new HttpClient(api)
        {
            BaseAddress = new Uri("http://fake-kubernetes"),
        };

        using var response = await client.GetAsync(
            "/api/v1/namespaces/restricted/pods",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RequestCancellationStopsDelayedResponse()
    {
        using var api = new FakeKubernetesHttpApi { ResponseDelay = TimeSpan.FromMinutes(1) };
        api.Register<V1Namespace>();

        using var client = new HttpClient(api)
        {
            BaseAddress = new Uri("http://fake-kubernetes"),
        };
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(
            () => client.GetAsync("/api/v1/namespaces", cancellation.Token));
    }

    [Fact]
    public async Task ShutdownClosesOutstandingWatchStreams()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Register<V1Pod>();

        using var client = new HttpClient(api)
        {
            BaseAddress = new Uri("http://fake-kubernetes"),
        };
        using HttpResponseMessage response = await client.GetAsync(
            "/api/v1/namespaces/default/pods?watch=true",
            HttpCompletionOption.ResponseHeadersRead,
            TestContext.Current.CancellationToken);
        using Stream stream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        byte[] buffer = new byte[4096];

        (await stream.ReadAsync(buffer, TestContext.Current.CancellationToken)).ShouldBeGreaterThan(0);
        api.Shutdown();

        await Should.ThrowAsync<OperationCanceledException>(
            () => stream.ReadAsync(buffer, TestContext.Current.CancellationToken).AsTask());
    }
}
