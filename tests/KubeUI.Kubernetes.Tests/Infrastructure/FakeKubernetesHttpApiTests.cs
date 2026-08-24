using System.Net;
using System.Net.Http.Json;
using k8s;
using k8s.Models;
using KubeUI.Kubernetes;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Infrastructure;

public sealed class FakeKubernetesHttpApiTests
{
    [Fact]
    public async Task AggregatedDiscoveryClient_reuses_etags_without_replacing_cached_responses()
    {
        using var api = new FakeKubernetesHttpApi();
        api.CoreDiscoveryETag = "\"fake-discovery-core-test\"";
        api.GroupedDiscoveryETag = "\"fake-discovery-groups-test\"";
        using var client = KubernetesClientMaterializer.Create(
            new KubernetesClientConfiguration { Host = "http://fake-kubernetes" },
            api);
        var discovery = new KubernetesApiDiscoveryClient(client);

        await discovery.RefreshAsync(TestContext.Current.CancellationToken);
        var core = discovery.Core;
        var groups = discovery.Groups;

        await discovery.RefreshAsync(TestContext.Current.CancellationToken);

        discovery.Core.ShouldBeSameAs(core);
        discovery.Groups.ShouldBeSameAs(groups);
        api.RequestUris.Count(uri => uri?.AbsolutePath == "/api").ShouldBe(2);
        api.RequestUris.Count(uri => uri?.AbsolutePath == "/apis").ShouldBe(2);
    }

    [Fact]
    public async Task AggregatedDiscoveryClient_skips_a_concurrent_refresh()
    {
        using var api = new FakeKubernetesHttpApi();
        using var conditionHandler = new TestConditionHandler(TimeSpan.FromMilliseconds(100), throwOnConnect: false)
        {
            InnerHandler = api,
        };
        using var client = KubernetesClientMaterializer.Create(
            new KubernetesClientConfiguration { Host = "http://fake-kubernetes" },
            conditionHandler);
        var discovery = new KubernetesApiDiscoveryClient(client);

        var firstRefresh = discovery.RefreshAsync(TestContext.Current.CancellationToken);
        var secondRefresh = discovery.RefreshAsync(TestContext.Current.CancellationToken);
        await Task.WhenAll(firstRefresh, secondRefresh);

        api.RequestUris.Count(uri => uri?.AbsolutePath == "/api").ShouldBe(2);
        api.RequestUris.Count(uri => uri?.AbsolutePath == "/apis").ShouldBe(2);
    }

    [Fact]
    public async Task FakeDiscovery_uses_core_etag_for_trailing_slash()
    {
        using var api = new FakeKubernetesHttpApi
        {
            CoreDiscoveryETag = "\"core\"",
            GroupedDiscoveryETag = "\"grouped\"",
        };

        using var client = new HttpClient(api) { BaseAddress = new Uri("http://fake-kubernetes") };
        using var response = await client.GetAsync("/api/", TestContext.Current.CancellationToken);

        response.Headers.ETag!.Tag.ShouldBe("\"core\"");
    }

    [Fact]
    public async Task RealClientUsesFakeHttpTransportForCrud()
    {
        using var api = new FakeKubernetesHttpApi();

        using var client = KubernetesClientMaterializer.Create(
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
        using var client = KubernetesClientMaterializer.Create(new KubernetesClientConfiguration { Host = "http://fake-kubernetes" }, api);
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
    public async Task RbacResourcesAuthorizeConfiguredServiceAccount()
    {
        using var api = new FakeKubernetesHttpApi
        {
            UseRoleBasedAuthorization = true,
            AuthenticatedUser = KubernetesRbac.ServiceAccountUser,
        };
        foreach (IKubernetesObject resource in KubernetesRbac.ClusterWide(new RbacRule("namespaces", "list")))
        {
            api.Add(resource);
        }

        using var client = KubernetesClientMaterializer.Create(
            new KubernetesClientConfiguration { Host = "http://fake-kubernetes" },
            api);

        var namespaces = await client.ListNamespaceAsync(cancellationToken: TestContext.Current.CancellationToken);

        namespaces.ShouldNotBeNull();
    }

    [Fact]
    public async Task RequestCancellationStopsDelayedResponse()
    {
        using var api = new FakeKubernetesHttpApi();
        using var conditionHandler = new TestConditionHandler(TimeSpan.FromMinutes(1), throwOnConnect: false)
        {
            InnerHandler = api,
        };
        api.Register<V1Namespace>();

        using var client = new HttpClient(conditionHandler)
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
        using var response = await client.GetAsync(
            "/api/v1/namespaces/default/pods?watch=true",
            HttpCompletionOption.ResponseHeadersRead,
            TestContext.Current.CancellationToken);
        using var stream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        var buffer = new byte[4096];

        (await stream.ReadAsync(buffer, TestContext.Current.CancellationToken)).ShouldBeGreaterThan(0);
        api.Shutdown();

        await Should.ThrowAsync<OperationCanceledException>(
            () => stream.ReadAsync(buffer, TestContext.Current.CancellationToken).AsTask());
    }
}
