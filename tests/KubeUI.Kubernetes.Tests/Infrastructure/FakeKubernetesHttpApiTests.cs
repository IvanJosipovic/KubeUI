using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using k8s;
using k8s.Models;
using KubeUI.Kubernetes.Serialization;
using Shouldly;

#pragma warning disable CA2000

namespace KubeUI.Kubernetes.Tests.Infrastructure;

public sealed class FakeKubernetesHttpApiTests
{
    private static readonly JsonSerializerOptions s_testJsonSerializerOptions = new()
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    private static JsonContent CreateJsonContent<T>(T value)
        => JsonContent.Create(value, options: s_testJsonSerializerOptions);

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
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/");
        request.Headers.Accept.ParseAdd("application/json;g=apidiscovery.k8s.io;v=v2beta1;as=APIGroupDiscoveryList");
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

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

        var nodes = await KubernetesMetricsClient.GetKubernetesNodesMetricsAsync(client, TestContext.Current.CancellationToken);
        var pods = await KubernetesMetricsClient.GetKubernetesPodsMetricsAsync(client, TestContext.Current.CancellationToken);
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
    public async Task NamespaceCollectionDoesNotIncludeResourcesFromNestedNamespaces()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Add(new V1ServiceAccount
        {
            ApiVersion = V1ServiceAccount.KubeApiVersion,
            Kind = V1ServiceAccount.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = "nested-resource",
                NamespaceProperty = "default",
            },
        });
        using var httpClient = new HttpClient(api)
        {
            BaseAddress = new Uri("http://fake-kubernetes"),
        };

        var json = await httpClient.GetStringAsync(
            "/api/v1/namespaces",
            TestContext.Current.CancellationToken);
        var namespaces = KubernetesJson.Deserialize<V1NamespaceList>(json);

        namespaces.Items.ShouldContain(item => item.Name() == "default");
        namespaces.Items.ShouldNotContain(item => item.Name() == "nested-resource");
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
            "/api/v1/namespaces/default/pods?watch=true&allowWatchBookmarks=true",
            HttpCompletionOption.ResponseHeadersRead,
            TestContext.Current.CancellationToken);
        using var stream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        var buffer = new byte[4096];

        (await stream.ReadAsync(buffer, TestContext.Current.CancellationToken)).ShouldBeGreaterThan(0);
        api.Shutdown();

        await Should.ThrowAsync<OperationCanceledException>(
            () => stream.ReadAsync(buffer, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task DiscoveryServesLegacyVersionsAndGroupResourceLists()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Add(CreateDeployment("web"));
        using var client = CreateHttpClient(api);

        var versions = JsonNode.Parse(await client.GetStringAsync("/api", TestContext.Current.CancellationToken))!.AsObject();
        versions["kind"]!.GetValue<string>().ShouldBe("APIVersions");
        versions["versions"]!.AsArray().Select(node => node!.GetValue<string>()).ShouldContain("v1");

        var group = JsonNode.Parse(await client.GetStringAsync("/apis/apps/v1", TestContext.Current.CancellationToken))!.AsObject();
        group["kind"]!.GetValue<string>().ShouldBe("APIResourceList");
        group["groupVersion"]!.GetValue<string>().ShouldBe("apps/v1");
        var names = group["resources"]!.AsArray().Select(node => node!["name"]!.GetValue<string>()).ToArray();
        names.ShouldContain("deployments");
        names.ShouldContain("deployments/status");
        names.ShouldContain("deployments/scale");
        var scale = group["resources"]!.AsArray()
            .Single(node => node!["name"]!.GetValue<string>() == "deployments/scale");
        scale!["kind"]!.GetValue<string>().ShouldBe("Scale");
        group["resources"]!.AsArray()
            .Single(node => node!["name"]!.GetValue<string>() == "deployments")!["singularName"]!
            .GetValue<string>()
            .ShouldBe("deployment");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/apis");
        request.Headers.Accept.ParseAdd("application/json;g=apidiscovery.k8s.io;v=v2beta1;as=APIGroupDiscoveryList");
        using var aggregatedResponse = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var aggregated = JsonNode.Parse(await aggregatedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))!.AsObject();
        var appsGroup = aggregated["items"]!.AsArray()
            .Single(node => node!["metadata"]!["name"]!.GetValue<string>() == "apps");
        var appsVersion = appsGroup!["versions"]!.AsArray()
            .Single(node => node!["version"]!.GetValue<string>() == "v1");
        var deployment = appsVersion!["resources"]!.AsArray()
            .Single(node => node!["resource"]!.GetValue<string>() == "deployments");
        var scaleResponse = deployment!["subresources"]!.AsArray()
            .Single(node => node!["subresource"]!.GetValue<string>() == "scale")!["responseKind"]!.AsObject();
        scaleResponse["group"]!.GetValue<string>().ShouldBe("autoscaling");
        scaleResponse["version"]!.GetValue<string>().ShouldBe("v1");
        scaleResponse["kind"]!.GetValue<string>().ShouldBe("Scale");
    }

    [Fact]
    public async Task ListHonorsSelectorsAndPaginatesWithContinueToken()
    {
        using var api = new FakeKubernetesHttpApi();
        for (var index = 0; index < 5; index++)
        {
            api.Add(new V1ConfigMap
            {
                Metadata = new V1ObjectMeta
                {
                    Name = $"cm-{index}",
                    NamespaceProperty = "default",
                    Labels = new Dictionary<string, string> { ["tier"] = index % 2 == 0 ? "web" : "db" },
                },
            });
        }

        using var client = CreateHttpClient(api);
        var firstPage = JsonNode.Parse(await client.GetStringAsync(
            "/api/v1/namespaces/default/configmaps?labelSelector=tier%3Dweb&limit=2",
            TestContext.Current.CancellationToken))!.AsObject();
        firstPage["kind"]!.GetValue<string>().ShouldBe("ConfigMapList");
        firstPage["apiVersion"]!.GetValue<string>().ShouldBe("v1");
        firstPage["items"]!.AsArray().Count.ShouldBe(2);
        firstPage["metadata"]!["remainingItemCount"]!.GetValue<int>().ShouldBe(1);
        var continueToken = firstPage["metadata"]!["continue"]!.GetValue<string>();

        var secondPage = JsonNode.Parse(await client.GetStringAsync(
            $"/api/v1/namespaces/default/configmaps?labelSelector=tier%3Dweb&limit=2&continue={Uri.EscapeDataString(continueToken)}",
            TestContext.Current.CancellationToken))!.AsObject();
        secondPage["items"]!.AsArray().Count.ShouldBe(1);
        secondPage["metadata"]!["remainingItemCount"].ShouldBeNull();

        var fieldPage = JsonNode.Parse(await client.GetStringAsync(
            "/api/v1/namespaces/default/configmaps?fieldSelector=metadata.name%3Dcm-3",
            TestContext.Current.CancellationToken))!.AsObject();
        fieldPage["items"]!.AsArray().Single()!["metadata"]!["name"]!.GetValue<string>().ShouldBe("cm-3");
    }

    [Fact]
    public async Task ListUsesTypedListKindAndGroupVersion()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Add(CreateDeployment("web"));
        using var client = CreateHttpClient(api);

        var list = JsonNode.Parse(await client.GetStringAsync(
            "/apis/apps/v1/namespaces/default/deployments",
            TestContext.Current.CancellationToken))!.AsObject();

        list["kind"]!.GetValue<string>().ShouldBe("DeploymentList");
        list["apiVersion"]!.GetValue<string>().ShouldBe("apps/v1");
    }

    [Fact]
    public async Task CreateAndUpdateEnforceKubernetesStatusCodesAndDryRun()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Register<V1ConfigMap>();
        using var client = CreateHttpClient(api);

        var create = await client.PostAsync(
            "/api/v1/namespaces/default/configmaps",
            CreateJsonContent(new { apiVersion = "v1", kind = "ConfigMap", metadata = new { name = "cm-a", @namespace = "default" } }),
            TestContext.Current.CancellationToken);
        create.StatusCode.ShouldBe(HttpStatusCode.Created, await create.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var duplicate = await client.PostAsync(
            "/api/v1/namespaces/default/configmaps",
            CreateJsonContent(new { apiVersion = "v1", kind = "ConfigMap", metadata = new { name = "cm-a", @namespace = "default" } }),
            TestContext.Current.CancellationToken);
        duplicate.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var dryRun = await client.PostAsync(
            "/api/v1/namespaces/default/configmaps?dryRun=All",
            CreateJsonContent(new { apiVersion = "v1", kind = "ConfigMap", metadata = new { name = "cm-dry", @namespace = "default" } }),
            TestContext.Current.CancellationToken);
        dryRun.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await client.GetAsync("/api/v1/namespaces/default/configmaps/cm-dry", TestContext.Current.CancellationToken))
            .StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var missingNamespace = await client.PostAsync(
            "/api/v1/namespaces/missing/configmaps",
            CreateJsonContent(new { apiVersion = "v1", kind = "ConfigMap", metadata = new { name = "cm-b", @namespace = "missing" } }),
            TestContext.Current.CancellationToken);
        missingNamespace.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        (await client.PutAsync(
            "/api/v1/namespaces/default/configmaps/missing",
            CreateJsonContent(new { apiVersion = "v1", kind = "ConfigMap", metadata = new { name = "missing" } }),
            TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        (await client.PatchAsync(
            "/api/v1/namespaces/default/configmaps/missing",
            new StringContent("{\"metadata\":{\"labels\":{\"a\":\"b\"}}}", Encoding.UTF8, "application/merge-patch+json"),
            TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        (await client.DeleteAsync("/api/v1/namespaces/default/configmaps/missing", TestContext.Current.CancellationToken))
            .StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReplaceRequiresMatchingResourceVersion()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Register<V1ConfigMap>();
        using var client = CreateHttpClient(api);
        await client.PostAsync(
            "/api/v1/namespaces/default/configmaps",
            CreateJsonContent(new { apiVersion = "v1", kind = "ConfigMap", metadata = new { name = "cm-a", @namespace = "default" } }),
            TestContext.Current.CancellationToken);

        var stored = JsonNode.Parse(await client.GetStringAsync(
            "/api/v1/namespaces/default/configmaps/cm-a",
            TestContext.Current.CancellationToken))!.AsObject();
        var resourceVersion = stored["metadata"]!["resourceVersion"]!.GetValue<string>();

        var withoutVersion = await client.PutAsync(
            "/api/v1/namespaces/default/configmaps/cm-a",
            CreateJsonContent(new { apiVersion = "v1", kind = "ConfigMap", metadata = new { name = "cm-a", @namespace = "default" } }),
            TestContext.Current.CancellationToken);
        withoutVersion.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var stale = await client.PutAsync(
            "/api/v1/namespaces/default/configmaps/cm-a",
            CreateJsonContent(new
            {
                apiVersion = "v1",
                kind = "ConfigMap",
                metadata = new { name = "cm-a", @namespace = "default", resourceVersion = "999999" },
            }),
            TestContext.Current.CancellationToken);
        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var updated = await client.PutAsync(
            "/api/v1/namespaces/default/configmaps/cm-a",
            CreateJsonContent(new
            {
                apiVersion = "v1",
                kind = "ConfigMap",
                metadata = new { name = "cm-a", @namespace = "default", resourceVersion },
                data = new Dictionary<string, string> { ["a"] = "1" },
            }),
            TestContext.Current.CancellationToken);
        updated.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateAssignsServerUidInsteadOfAcceptingClientUid()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Register<V1ConfigMap>();
        using var client = CreateHttpClient(api);

        var response = await client.PostAsync(
            "/api/v1/namespaces/default/configmaps",
            CreateJsonContent(new
            {
                apiVersion = "v1",
                kind = "ConfigMap",
                metadata = new { name = "uid-contract", @namespace = "default", uid = "client-uid" },
            }),
            TestContext.Current.CancellationToken);
        var created = JsonNode.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))!.AsObject();

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        created["metadata"]!["uid"]!.GetValue<string>().ShouldNotBe("client-uid");
    }

    [Fact]
    public async Task PatchAndReplaceDryRunDoNotPersistOrAdvanceResourceVersion()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Register<V1ConfigMap>();
        using var client = CreateHttpClient(api);
        await client.PostAsync(
            "/api/v1/namespaces/default/configmaps",
            CreateJsonContent(new
            {
                apiVersion = "v1",
                kind = "ConfigMap",
                metadata = new { name = "dry-run-update", @namespace = "default" },
                data = new Dictionary<string, string> { ["state"] = "before" },
            }),
            TestContext.Current.CancellationToken);

        var original = JsonNode.Parse(await client.GetStringAsync(
            "/api/v1/namespaces/default/configmaps/dry-run-update",
            TestContext.Current.CancellationToken))!.AsObject();
        var originalVersion = original["metadata"]!["resourceVersion"]!.GetValue<string>();

        var patch = await client.PatchAsync(
            "/api/v1/namespaces/default/configmaps/dry-run-update?dryRun=All",
            new StringContent("{\"data\":{\"state\":\"patched\"}}", Encoding.UTF8, "application/merge-patch+json"),
            TestContext.Current.CancellationToken);
        patch.StatusCode.ShouldBe(HttpStatusCode.OK);
        var patchResult = JsonNode.Parse(await patch.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))!.AsObject();
        patchResult["data"]!["state"]!.GetValue<string>().ShouldBe("patched");

        original["data"]!["state"] = "replaced";
        var replace = await client.PutAsync(
            "/api/v1/namespaces/default/configmaps/dry-run-update?dryRun=All",
            CreateJsonContent(original),
            TestContext.Current.CancellationToken);
        replace.StatusCode.ShouldBe(HttpStatusCode.OK);

        var stored = JsonNode.Parse(await client.GetStringAsync(
            "/api/v1/namespaces/default/configmaps/dry-run-update",
            TestContext.Current.CancellationToken))!.AsObject();
        stored["data"]!["state"]!.GetValue<string>().ShouldBe("before");
        stored["metadata"]!["resourceVersion"]!.GetValue<string>().ShouldBe(originalVersion);
    }

    [Fact]
    public async Task PatchSupportsJsonPatchAndStrategicMergePatch()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Register<V1ConfigMap>();
        api.Register<V1Deployment>();
        using var client = CreateHttpClient(api);
        await client.PostAsync(
            "/api/v1/namespaces/default/configmaps",
            CreateJsonContent(new
            {
                apiVersion = "v1",
                kind = "ConfigMap",
                metadata = new { name = "cm-a", @namespace = "default" },
                data = new Dictionary<string, string> { ["a"] = "1", ["b"] = "2" },
            }),
            TestContext.Current.CancellationToken);

        var jsonPatched = await client.PatchAsync(
            "/api/v1/namespaces/default/configmaps/cm-a",
            new StringContent(
                "[{\"op\":\"replace\",\"path\":\"/data/a\",\"value\":\"9\"},{\"op\":\"remove\",\"path\":\"/data/b\"}]",
                Encoding.UTF8,
                "application/json-patch+json"),
            TestContext.Current.CancellationToken);
        jsonPatched.StatusCode.ShouldBe(HttpStatusCode.OK, await jsonPatched.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var jsonPatchedBody = JsonNode.Parse(await jsonPatched.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))!.AsObject();
        jsonPatchedBody["data"]!["a"]!.GetValue<string>().ShouldBe("9");
        jsonPatchedBody["data"]!["b"].ShouldBeNull();

        await client.PostAsync(
            "/apis/apps/v1/namespaces/default/deployments",
            CreateJsonContent(new
            {
                apiVersion = "apps/v1",
                kind = "Deployment",
                metadata = new { name = "web", @namespace = "default" },
                spec = new
                {
                    selector = new { matchLabels = new Dictionary<string, string> { ["app"] = "web" } },
                    template = new
                    {
                        metadata = new { labels = new Dictionary<string, string> { ["app"] = "web" } },
                        spec = new
                        {
                            containers = new object[]
                            {
                                new
                                {
                                    name = "app",
                                    image = "nginx:1.27",
                                    env = new object[] { new { name = "A", value = "1" } },
                                },
                            },
                        },
                    },
                },
            }),
            TestContext.Current.CancellationToken);

        var strategicPatched = await client.PatchAsync(
            "/apis/apps/v1/namespaces/default/deployments/web",
            new StringContent(
                "{\"spec\":{\"template\":{\"spec\":{\"containers\":[{\"name\":\"app\",\"env\":[{\"name\":\"A\",\"value\":\"2\"}]}]}}}}",
                Encoding.UTF8,
                "application/strategic-merge-patch+json"),
            TestContext.Current.CancellationToken);
        strategicPatched.StatusCode.ShouldBe(HttpStatusCode.OK);
        var strategicBody = JsonNode.Parse(await strategicPatched.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))!.AsObject();
        var container = strategicBody["spec"]!["template"]!["spec"]!["containers"]!.AsArray().Single()!;
        container["image"]!.GetValue<string>().ShouldBe("nginx:1.27");
        container["env"]!.AsArray().Single()!["value"]!.GetValue<string>().ShouldBe("2");
    }

    [Fact]
    public async Task DeleteHonorsFinalizersAndPreconditions()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Register<V1ConfigMap>();
        using var client = CreateHttpClient(api);
        await client.PostAsync(
            "/api/v1/namespaces/default/configmaps",
            CreateJsonContent(new
            {
                apiVersion = "v1",
                kind = "ConfigMap",
                metadata = new
                {
                    name = "cm-final",
                    @namespace = "default",
                    finalizers = new[] { "example.com/hold" },
                },
            }),
            TestContext.Current.CancellationToken);

        var precondition = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/namespaces/default/configmaps/cm-final")
        {
            Content = CreateJsonContent(new
            {
                apiVersion = "v1",
                kind = "DeleteOptions",
                preconditions = new { uid = "does-not-match" },
            }),
        };
        (await client.SendAsync(precondition, TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var deleted = await client.DeleteAsync("/api/v1/namespaces/default/configmaps/cm-final", TestContext.Current.CancellationToken);
        deleted.StatusCode.ShouldBe(HttpStatusCode.OK);
        var deletedBody = JsonNode.Parse(await deleted.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))!.AsObject();
        deletedBody["kind"]!.GetValue<string>().ShouldBe("ConfigMap");
        deletedBody["metadata"]!["deletionTimestamp"].ShouldNotBeNull();

        var pendingResponse = await client.GetAsync(
            "/api/v1/namespaces/default/configmaps/cm-final",
            TestContext.Current.CancellationToken);
        pendingResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pending = JsonNode.Parse(await pendingResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))!.AsObject();
        pending["metadata"]!["deletionTimestamp"].ShouldNotBeNull();

        pending["metadata"]!["finalizers"] = new JsonArray();
        var cleared = await client.PutAsync(
            "/api/v1/namespaces/default/configmaps/cm-final",
            CreateJsonContent(pending),
            TestContext.Current.CancellationToken);
        cleared.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await client.GetAsync("/api/v1/namespaces/default/configmaps/cm-final", TestContext.Current.CancellationToken))
            .StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StatusSubresourceUpdatesOnlyStatusAndPreservesGeneration()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Register<V1Deployment>();
        using var client = CreateHttpClient(api);
        await client.PostAsync(
            "/apis/apps/v1/namespaces/default/deployments",
            CreateJsonContent(new
            {
                apiVersion = "apps/v1",
                kind = "Deployment",
                metadata = new { name = "web", @namespace = "default" },
                spec = new
                {
                    selector = new { matchLabels = new Dictionary<string, string> { ["app"] = "web" } },
                    template = new
                    {
                        metadata = new { labels = new Dictionary<string, string> { ["app"] = "web" } },
                        spec = new
                        {
                            containers = new object[] { new { name = "app", image = "nginx:1.27" } },
                        },
                    },
                },
            }),
            TestContext.Current.CancellationToken);

        var patched = await client.PatchAsync(
            "/apis/apps/v1/namespaces/default/deployments/web/status",
            new StringContent("{\"status\":{\"replicas\":3,\"observedGeneration\":1}}", Encoding.UTF8, "application/merge-patch+json"),
            TestContext.Current.CancellationToken);
        patched.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = JsonNode.Parse(await patched.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))!.AsObject();
        body["status"]!["replicas"]!.GetValue<int>().ShouldBe(3);
        body["spec"]!["replicas"]!.GetValue<int>().ShouldBe(1);
        body["metadata"]!["generation"]!.GetValue<int>().ShouldBe(1);
    }

    [Fact]
    public async Task SelfSubjectAccessReviewEvaluatesNonResourceAttributes()
    {
        using var api = new FakeKubernetesHttpApi
        {
            UseRoleBasedAuthorization = true,
            AuthenticatedUser = KubernetesRbac.ServiceAccountUser,
        };
        api.Add(new V1ClusterRole
        {
            ApiVersion = "rbac.authorization.k8s.io/v1",
            Kind = V1ClusterRole.KubeKind,
            Metadata = new V1ObjectMeta { Name = "nonresource-reader" },
            Rules = [new V1PolicyRule { NonResourceURLs = ["/healthz/*"], Verbs = ["get"] }],
        });
        api.Add(new V1ClusterRoleBinding
        {
            ApiVersion = "rbac.authorization.k8s.io/v1",
            Kind = V1ClusterRoleBinding.KubeKind,
            Metadata = new V1ObjectMeta { Name = "nonresource-reader" },
            RoleRef = new()
            {
                ApiGroup = "rbac.authorization.k8s.io",
                Kind = "ClusterRole",
                Name = "nonresource-reader",
            },
            Subjects =
            [
                new Rbacv1Subject
                {
                    Kind = "ServiceAccount",
                    Name = "my-serviceaccount",
                    NamespaceProperty = "my-app",
                },
            ],
        });

        using var client = CreateHttpClient(api);
        var allowed = await client.PostAsync(
            "/apis/authorization.k8s.io/v1/selfsubjectaccessreviews",
            CreateJsonContent(new
            {
                apiVersion = "authorization.k8s.io/v1",
                kind = "SelfSubjectAccessReview",
                spec = new { nonResourceAttributes = new { path = "/healthz/ready", verb = "get" } },
            }),
            TestContext.Current.CancellationToken);
        var allowedBody = JsonNode.Parse(await allowed.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))!.AsObject();
        allowedBody["status"]!["allowed"]!.GetValue<bool>().ShouldBeTrue();

        var denied = await client.PostAsync(
            "/apis/authorization.k8s.io/v1/selfsubjectaccessreviews",
            CreateJsonContent(new
            {
                apiVersion = "authorization.k8s.io/v1",
                kind = "SelfSubjectAccessReview",
                spec = new { nonResourceAttributes = new { path = "/metrics", verb = "get" } },
            }),
            TestContext.Current.CancellationToken);
        var deniedBody = JsonNode.Parse(await denied.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))!.AsObject();
        deniedBody["status"]!["allowed"]!.GetValue<bool>().ShouldBeFalse();
    }

    [Fact]
    public async Task ResourceNameRbacRestrictionsApplyToIndividualRequests()
    {
        using var api = new FakeKubernetesHttpApi
        {
            UseRoleBasedAuthorization = true,
            AuthenticatedUser = KubernetesRbac.ServiceAccountUser,
        };
        api.Add(new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "allowed", NamespaceProperty = "default" },
        });
        api.Add(new V1ClusterRole
        {
            ApiVersion = "rbac.authorization.k8s.io/v1",
            Kind = V1ClusterRole.KubeKind,
            Metadata = new V1ObjectMeta { Name = "pod-reader" },
            Rules =
            [
                new V1PolicyRule
                {
                    ApiGroups = [""],
                    Resources = ["pods"],
                    ResourceNames = ["allowed"],
                    Verbs = ["get"],
                },
            ],
        });
        api.Add(new V1ClusterRoleBinding
        {
            ApiVersion = "rbac.authorization.k8s.io/v1",
            Kind = V1ClusterRoleBinding.KubeKind,
            Metadata = new V1ObjectMeta { Name = "pod-reader" },
            RoleRef = new()
            {
                ApiGroup = "rbac.authorization.k8s.io",
                Kind = "ClusterRole",
                Name = "pod-reader",
            },
            Subjects =
            [
                new Rbacv1Subject
                {
                    Kind = "ServiceAccount",
                    Name = "my-serviceaccount",
                    NamespaceProperty = "my-app",
                },
            ],
        });
        using var client = CreateHttpClient(api);

        (await client.GetAsync(
            "/api/v1/namespaces/default/pods/allowed",
            TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await client.GetAsync(
            "/api/v1/namespaces/default/pods/denied",
            TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UnknownResourcesReturnNotFoundInsteadOfInventingAnApi()
    {
        using var api = new FakeKubernetesHttpApi();
        using var client = CreateHttpClient(api);

        using var response = await client.GetAsync(
            "/api/v1/namespaces/default/widgets",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CustomResourceDefinitionRegistersServedVersionAndRemovesItOnDelete()
    {
        using var api = new FakeKubernetesHttpApi();
        api.Register<V1CustomResourceDefinition>();
        using var client = CreateHttpClient(api);

        var crd = await client.PostAsync(
            "/apis/apiextensions.k8s.io/v1/customresourcedefinitions",
            CreateJsonContent(new
            {
                apiVersion = "apiextensions.k8s.io/v1",
                kind = "CustomResourceDefinition",
                metadata = new { name = "widgets.example.test" },
                spec = new
                {
                    group = "example.test",
                    scope = "Cluster",
                    names = new { plural = "widgets", singular = "widget", kind = "Widget", listKind = "WidgetList", shortNames = new[] { "wdg" } },
                    versions = new[]
                    {
                        new
                        {
                            name = "v1",
                            served = true,
                            storage = true,
                            schema = new { openAPIV3Schema = new { type = "object" } },
                        },
                    },
                },
            }),
            TestContext.Current.CancellationToken);
        crd.StatusCode.ShouldBe(HttpStatusCode.Created);

        var discovery = JsonNode.Parse(await client.GetStringAsync(
            "/apis/example.test/v1",
            TestContext.Current.CancellationToken))!.AsObject();
        var widget = discovery["resources"]!.AsArray().Single();
        widget!["kind"]!.GetValue<string>().ShouldBe("Widget");
        widget["singularName"]!.GetValue<string>().ShouldBe("widget");
        widget["namespaced"]!.GetValue<bool>().ShouldBeFalse();

        var created = await client.PostAsync(
            "/apis/example.test/v1/widgets",
            CreateJsonContent(new
            {
                apiVersion = "example.test/v1",
                kind = "Widget",
                metadata = new { name = "sample" },
            }),
            TestContext.Current.CancellationToken);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);

        var deleted = await client.DeleteAsync(
            "/apis/apiextensions.k8s.io/v1/customresourcedefinitions/widgets.example.test",
            TestContext.Current.CancellationToken);
        deleted.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await client.GetAsync(
            "/apis/example.test/v1/widgets",
            TestContext.Current.CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static HttpClient CreateHttpClient(FakeKubernetesHttpApi api)
        => new(api) { BaseAddress = new Uri("http://fake-kubernetes") };

    private static V1Deployment CreateDeployment(string name) => new()
    {
        ApiVersion = "apps/v1",
        Kind = V1Deployment.KubeKind,
        Metadata = new V1ObjectMeta { Name = name, NamespaceProperty = "default" },
    };
}
