using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using k8s;
using k8s.Models;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Infrastructure;

public sealed class FakeKubernetesHttpApiTests
{
    [Fact]
    public async Task KubernetesMetricsClientDeserializesNodeMetricsWhenReflectionIsDisabled()
    {
        JsonSerializer.IsReflectionEnabledByDefault.ShouldBeFalse();

        var response = """
            {
              "apiVersion": "metrics.k8s.io/v1beta1",
              "kind": "NodeMetricsList",
              "metadata": {},
              "items": [
                {
                  "metadata": { "name": "node-1" },
                  "timestamp": "2026-09-24T12:00:00Z",
                  "window": "30s",
                  "usage": { "cpu": "100m", "memory": "64Mi" }
                }
              ]
            }
            """;
        using var handler = new MetricsResponseHandler(response);
        using var client = KubernetesClientMaterializer.Create(
            new KubernetesClientConfiguration { Host = "http://fake-kubernetes" },
            handler);

        var metrics = await KubernetesMetricsClient.GetKubernetesNodesMetricsAsync(client, TestContext.Current.CancellationToken);

        metrics.Items.ShouldHaveSingleItem().Name().ShouldBe("node-1");
    }

    [Fact]
    public async Task KubernetesMetricsClientDeserializesPodMetricsWhenReflectionIsDisabled()
    {
        JsonSerializer.IsReflectionEnabledByDefault.ShouldBeFalse();

        var response = """
            {
              "apiVersion": "metrics.k8s.io/v1beta1",
              "kind": "PodMetricsList",
              "metadata": {},
              "items": [
                {
                  "metadata": { "name": "pod-1", "namespace": "default" },
                  "timestamp": "2026-09-24T12:00:00Z",
                  "window": "30s",
                  "containers": [
                    { "name": "app", "usage": { "cpu": "100m", "memory": "64Mi" } }
                  ]
                }
              ]
            }
            """;
        using var handler = new MetricsResponseHandler(response);
        using var client = KubernetesClientMaterializer.Create(
            new KubernetesClientConfiguration { Host = "http://fake-kubernetes" },
            handler);

        var metrics = await KubernetesMetricsClient.GetKubernetesPodsMetricsAsync(client, TestContext.Current.CancellationToken);

        metrics.Items.ShouldHaveSingleItem().Name().ShouldBe("pod-1");
    }

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
    public async Task AggregatedDiscoveryClient_traces_discovery_requests()
    {
        var activities = new ConcurrentQueue<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == Client.KubeInstrumentation.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => activities.Enqueue(activity),
        };
        ActivitySource.AddActivityListener(listener);

        using var api = new FakeKubernetesHttpApi();
        using var client = KubernetesClientMaterializer.Create(
            new KubernetesClientConfiguration { Host = "http://fake-kubernetes-tracing" },
            api);
        var discovery = new KubernetesApiDiscoveryClient(client);

        await discovery.RefreshAsync(TestContext.Current.CancellationToken);

        var discoveryActivities = activities
            .Where(activity => activity.OperationName == "kubernetes.discovery"
                && activity.GetTagItem("url.full") is string url
                && url.StartsWith("http://fake-kubernetes-tracing/", StringComparison.Ordinal))
            .ToArray();

        discoveryActivities.Length.ShouldBe(2);
        discoveryActivities.Select(activity => activity.GetTagItem("url.path")).ShouldBe(["/api", "/apis"]);
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

    private sealed class MetricsResponseHandler(string response) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new StringContent(response, Encoding.UTF8, "application/json"),
            });
        }
    }

    [Fact]
    public async Task FakeObjectsReceiveCreationTimestamps()
    {
        using var api = new FakeKubernetesHttpApi();
        using var client = KubernetesClientMaterializer.Create(
            new KubernetesClientConfiguration { Host = "http://fake-kubernetes" },
            api);

        var listed = await client.ListNamespaceAsync(cancellationToken: TestContext.Current.CancellationToken);

        var fakeNamespace = listed.Items.Single(@namespace => @namespace.Metadata.Name == "default");
        fakeNamespace.Metadata.CreationTimestamp.ShouldNotBeNull();

        var createdNamespace = await client.CreateNamespaceAsync(new V1Namespace
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta { Name = "created" },
        }, cancellationToken: TestContext.Current.CancellationToken);

        createdNamespace.Metadata.CreationTimestamp.ShouldNotBeNull();
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
    public async Task FakeTransportServesNodeAndPodMetricsResources()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Add(KubernetesJson.Deserialize<GenericKubernetesObject>("""
            {
              "apiVersion": "metrics.k8s.io/v1beta1",
              "kind": "NodeMetrics",
              "metadata": { "name": "node-1" },
              "timestamp": "2026-09-24T12:00:00Z",
              "window": "30s",
              "usage": { "cpu": "250m", "memory": "2Gi" }
            }
            """)!);
        api.Add(KubernetesJson.Deserialize<GenericKubernetesObject>("""
            {
              "apiVersion": "metrics.k8s.io/v1beta1",
              "kind": "PodMetrics",
              "metadata": { "name": "web", "namespace": "default" },
              "timestamp": "2026-09-24T12:00:00Z",
              "window": "30s",
              "containers": [
                { "name": "web", "usage": { "cpu": "50m", "memory": "64Mi" } }
              ]
            }
            """)!);

        using var client = KubernetesClientMaterializer.Create(
            new KubernetesClientConfiguration { Host = "http://fake-kubernetes" },
            api);

        var nodes = await client.GetKubernetesNodesMetricsAsync();
        var pods = await client.GetKubernetesPodsMetricsAsync();
        var apiGroups = await client.Apis.GetAPIVersionsAsync();

        var nodeMetric = nodes.Items.ShouldHaveSingleItem();
        nodeMetric.Metadata.Name.ShouldBe("node-1");
        nodeMetric.Usage["cpu"].ToString().ShouldBe("250m");
        nodeMetric.Usage["memory"].ToString().ShouldBe("2Gi");
        var podMetric = pods.Items.ShouldHaveSingleItem();
        podMetric.Metadata.Name.ShouldBe("web");
        var containerMetric = podMetric.Containers.ShouldHaveSingleItem();
        containerMetric.Usage["cpu"].ToString().ShouldBe("50m");
        containerMetric.Usage["memory"].ToString().ShouldBe("64Mi");
        apiGroups.Groups.ShouldContain(group => group.Name == "metrics.k8s.io");
        api.RequestUris.Select(uri => uri?.AbsolutePath.TrimEnd('/')).ShouldContain("/apis/metrics.k8s.io/v1beta1/nodes");
        api.RequestUris.Select(uri => uri?.AbsolutePath.TrimEnd('/')).ShouldContain("/apis/metrics.k8s.io/v1beta1/pods");
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
            KubernetesJsonStaticContext.Default.Options,
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
