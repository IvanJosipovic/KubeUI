using System.Reflection;
using System.Text;
using Avalonia.Headless.XUnit;
using k8s;
using k8s.Models;
using KubeUI.AI.Agents;
using KubeUI.AI.Mcp;
using KubeUI.AI.Permissions;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Mcp;

public sealed class McpToolsTests
{
    [Fact]
    public void mcp_tools_declare_safe_protocol_annotations()
    {
        var readOnlyTools = new Dictionary<string, string>
        {
            [nameof(McpTools.ListClusters)] = "List KubeUI clusters",
            [nameof(McpTools.ListSupportedResources)] = "List supported Kubernetes resources",
            [nameof(McpTools.ListResources)] = "List Kubernetes resources",
            [nameof(McpTools.ListEvents)] = "List Kubernetes events",
            [nameof(McpTools.ListRelatedResources)] = "List related Kubernetes resources",
            [nameof(McpTools.GetResourceGraph)] = "Show Kubernetes resource graph",
            [nameof(McpTools.DiffResourceYaml)] = "Compare resource YAML",
            [nameof(McpTools.GetPodLogs)] = "Get pod logs",
            [nameof(McpTools.GetResourceYaml)] = "Get resource YAML",
            [nameof(McpTools.GetEndpoint)] = "Get KubeUI MCP endpoint",
            [nameof(McpTools.OpenResourceList)] = "Open Kubernetes resource list"
        };

        foreach (var tool in readOnlyTools)
        {
            var attribute = typeof(McpTools).GetMethod(tool.Key)!.GetCustomAttribute<McpServerToolAttribute>()!;
            attribute.Title.ShouldBe(tool.Value);
            attribute.Destructive.ShouldBeFalse(tool.Key);
            attribute.ReadOnly.ShouldBeTrue(tool.Key);
            attribute.Idempotent.ShouldBeTrue(tool.Key);
        }

        var connect = typeof(McpTools).GetMethod(nameof(McpTools.ConnectCluster))!.GetCustomAttribute<McpServerToolAttribute>()!;
        connect.Title.ShouldBe("Connect KubeUI cluster");
        connect.Destructive.ShouldBeFalse();
        connect.ReadOnly.ShouldBeFalse();
        connect.Idempotent.ShouldBeTrue();
    }

    [Fact]
    public async Task list_resources_returns_resource_identity_without_object_contents()
    {
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        var expected = new McpResourcePage(
            [new McpResourceInfo("v1", "Pod", "api", "default", true)],
            "next-page");
        session.Setup(x => x.ListResourcesAsync("dev", "v1", "Pod", "default", 10))
            .ReturnsAsync(expected);
        var tools = new McpTools(
            new Mock<IClusterRuntimeCatalog>().Object,
            session.Object,
            new Mock<IKubernetesYamlSerializer>().Object,
            new Mock<ISettingsService>().Object,
            identityAccessor: CreateInternalIdentityAccessor());

        var resources = await tools.ListResources("dev", "v1", "Pod", "default", 10);

        resources.Resources.ShouldBe([
            new McpResourceInfo("v1", "Pod", "api", "default", true)
        ]);
        resources.NextCursor.ShouldBe("next-page");
        session.VerifyAll();
    }

    [Fact]
    public async Task list_resources_accepts_a_cursor_for_the_granted_default_cluster()
    {
        var dev = new Mock<IClusterRuntime>(MockBehavior.Strict);
        dev.SetupGet(x => x.Name).Returns("dev");
        var catalog = new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict);
        catalog.Setup(x => x.GetDefault()).Returns(dev.Object);
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        var cursor = McpResourceCursorCodec.Encode("dev", "v1", "Pod", null, "default", "api", "uid-api");
        var expected = new McpResourcePage(
            [new McpResourceInfo("v1", "Pod", "worker", "default", true)],
            null);
        session.Setup(x => x.ListResourcesAsync(
                null, "v1", "Pod", null, 100, It.IsAny<CancellationToken>(), cursor))
            .ReturnsAsync(expected);
        var explicitCursor = McpResourceCursorCodec.Encode("dev", "v1", "Pod", null, null, "worker", "uid-worker");
        session.Setup(x => x.ListResourcesAsync(
                "dev", "v1", "Pod", null, 100, It.IsAny<CancellationToken>(), explicitCursor))
            .ReturnsAsync(expected);
        var tools = new McpTools(
            catalog.Object,
            session.Object,
            new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object,
            new Mock<ISettingsService>(MockBehavior.Strict).Object,
            identityAccessor: CreateExternalIdentityAccessor("dev"));

        var page = await tools.ListResources(null, "v1", "Pod", cursor: cursor);
        var explicitPage = await tools.ListResources("dev", "v1", "Pod", cursor: explicitCursor);

        page.ShouldBe(expected);
        explicitPage.ShouldBe(expected);
        catalog.VerifyAll();
        session.VerifyAll();
    }

    [Fact]
    public void list_clusters_filters_results_to_the_external_client_grant()
    {
        var dev = CreateRuntime("dev");
        var prod = CreateRuntime("prod");
        var catalog = new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict);
        catalog.SetupGet(x => x.Clusters).Returns([dev.Object, prod.Object]);
        catalog.Setup(x => x.GetDefault()).Returns(dev.Object);
        var tools = new McpTools(
            catalog.Object,
            new Mock<IMcpClusterSession>(MockBehavior.Strict).Object,
            new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object,
            new Mock<ISettingsService>(MockBehavior.Strict).Object,
            identityAccessor: CreateExternalIdentityAccessor("dev"));

        var clusters = tools.ListClusters();

        clusters.ShouldBe([new McpClusterInfo("dev", true, true, "Connected", false)]);
        catalog.VerifyAll();
    }

    [Fact]
    public async Task empty_external_cluster_grant_lists_all_clusters()
    {
        var dev = CreateRuntime("dev");
        var prod = CreateRuntime("prod");
        var catalog = new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict);
        catalog.SetupGet(x => x.Clusters).Returns([dev.Object, prod.Object]);
        catalog.Setup(x => x.GetDefault()).Returns(dev.Object);
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        session.Setup(x => x.ListSupportedResourcesAsync("prod")).ReturnsAsync([]);
        var identity = new McpClientIdentity("external", "external tool", false,
            new HashSet<string>(StringComparer.Ordinal));
        var tools = new McpTools(
            catalog.Object,
            session.Object,
            new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object,
            new Mock<ISettingsService>(MockBehavior.Strict).Object,
            identityAccessor: new McpRequestIdentityAccessor(identity));

        tools.ListClusters().Select(cluster => cluster.Name).ShouldBe(["dev", "prod"]);
        (await tools.ListSupportedResources("prod")).ShouldBeEmpty();
        catalog.VerifyAll();
        session.VerifyAll();
    }

    [Fact]
    public void list_clusters_marks_no_cluster_default_when_none_is_configured()
    {
        var dev = CreateRuntime("dev");
        var catalog = new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict);
        catalog.SetupGet(x => x.Clusters).Returns([dev.Object]);
        catalog.Setup(x => x.GetDefault()).Returns((IClusterRuntime?)null);
        var tools = new McpTools(
            catalog.Object,
            new Mock<IMcpClusterSession>(MockBehavior.Strict).Object,
            new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object,
            new Mock<ISettingsService>(MockBehavior.Strict).Object,
            identityAccessor: CreateInternalIdentityAccessor());

        tools.ListClusters().ShouldBe([new McpClusterInfo("dev", false, true, "Connected", false)]);
        catalog.VerifyAll();
    }

    [Fact]
    public async Task connect_cluster_returns_the_connected_runtime_status()
    {
        var runtime = CreateRuntime("dev");
        var catalog = new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict);
        catalog.Setup(x => x.GetDefault()).Returns(runtime.Object);
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        session.Setup(x => x.GetConnectedClusterAsync("dev")).ReturnsAsync(runtime.Object);
        var tools = new McpTools(
            catalog.Object,
            session.Object,
            new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object,
            new Mock<ISettingsService>(MockBehavior.Strict).Object,
            identityAccessor: CreateInternalIdentityAccessor());

        var connected = await tools.ConnectCluster("dev");

        connected.ShouldBe(new McpClusterInfo("dev", true, true, "Connected", false));
        catalog.VerifyAll();
        session.VerifyAll();
    }

    [Fact]
    public async Task supported_resource_discovery_and_event_listing_use_the_scoped_cluster()
    {
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        var supportedResources = new[]
        {
            new McpSupportedResourceInfo("Pods", "Workloads", string.Empty, "v1", "v1", "Pod", true, false, true, true)
        };
        session.Setup(x => x.ListSupportedResourcesAsync("dev")).ReturnsAsync(supportedResources);
        session.Setup(x => x.ListResourcesAsync(
                "dev", "v1", "Event", "default", 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new McpResourcePage([], null));
        var tools = new McpTools(
            new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict).Object,
            session.Object,
            new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object,
            new Mock<ISettingsService>(MockBehavior.Strict).Object,
            identityAccessor: CreateExternalIdentityAccessor("dev"));

        var discovered = await tools.ListSupportedResources("dev");
        var events = await tools.ListEvents("dev", "default");

        discovered.ShouldBe(supportedResources);
        events.Resources.ShouldBeEmpty();
        events.NextCursor.ShouldBeNull();
        session.VerifyAll();
    }

    [Fact]
    public async Task external_default_cluster_is_allowed_only_when_explicitly_granted()
    {
        var runtime = CreateRuntime("dev");
        var catalog = new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict);
        catalog.Setup(x => x.GetDefault()).Returns(runtime.Object);
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        session.Setup(x => x.ListResourcesAsync(null, "v1", "Pod", null, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new McpResourcePage([], null));
        var tools = new McpTools(
            catalog.Object,
            session.Object,
            new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object,
            new Mock<ISettingsService>(MockBehavior.Strict).Object,
            identityAccessor: CreateExternalIdentityAccessor("dev"));

        (await tools.ListResources(null, "v1", "Pod")).Resources.ShouldBeEmpty();
        catalog.Verify(x => x.GetDefault(), Times.Once);
        session.VerifyAll();
    }

    [Fact]
    public async Task external_client_cannot_access_a_cluster_outside_its_grant()
    {
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        var identity = new McpClientIdentity("external", "external tool", false,
            new HashSet<string>(["dev"], StringComparer.Ordinal));
        var tools = new McpTools(
            new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict).Object,
            session.Object,
            new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object,
            new Mock<ISettingsService>(MockBehavior.Strict).Object,
            identityAccessor: new McpRequestIdentityAccessor(identity));

        await Should.ThrowAsync<UnauthorizedAccessException>(() => tools.ListResources("prod", "v1", "Pod"));

        session.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task resource_tools_validate_inputs_and_fail_closed_before_session_access()
    {
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        var tools = CreateTools(session);

        await Should.ThrowAsync<ArgumentException>(() => tools.ListResources("dev", "apps/v1\r\nInjected", "Deployment"));
        await Should.ThrowAsync<ArgumentException>(() => tools.ListResources("dev", "apps/v1", "1Deployment"));
        await Should.ThrowAsync<ArgumentException>(() => tools.ListResources("dev", "v1", "Pod", "UPPER"));
        await Should.ThrowAsync<ArgumentException>(() => tools.ListResources("dev", "v1", "Pod", string.Empty));
        await Should.ThrowAsync<ArgumentException>(() => tools.ListResources("dev", "v1", "Pod", new string('a', 64)));
        await Should.ThrowAsync<ArgumentException>(() => tools.ListResources("dev", "v1", "Pod", "-bad"));
        await Should.ThrowAsync<ArgumentException>(() => tools.ListRelatedResources("dev", "v1", "Pod", "bad_name"));
        await Should.ThrowAsync<ArgumentException>(() => tools.ListResources("dev\n", "v1", "Pod"));
        await Should.ThrowAsync<ArgumentException>(() => tools.ListResources(new string('d', 257), "v1", "Pod"));
        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => tools.ListResources("dev", "v1", "Pod", limit: 0));
        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => tools.ListResources("dev", "v1", "Pod", limit: 501));
        await Should.ThrowAsync<ArgumentException>(() => tools.ListResources(
            "dev", "v1", "Pod", cursor: new string('x', 4097)));
        Should.Throw<ArgumentOutOfRangeException>(() => tools.ListRelatedResources("dev", "v1", "Pod", "api", limit: 0));
        Should.Throw<ArgumentOutOfRangeException>(() => tools.GetResourceGraph("dev", "v1", "Pod", "api", limit: 501));
        await Should.ThrowAsync<ArgumentException>(() => tools.GetPodLogs("dev", "default", "bad_name"));
        await Should.ThrowAsync<ArgumentException>(() => tools.GetPodLogs("dev", "default", string.Empty));
        await Should.ThrowAsync<ArgumentException>(() => tools.GetPodLogs("dev", "default", new string('a', 254)));
        await Should.ThrowAsync<ArgumentException>(() => tools.GetPodLogs("dev", "bad-", "api"));
        await Should.ThrowAsync<ArgumentException>(() => tools.GetPodLogs("dev", "default", "api", container: "Upper"));
        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => tools.GetPodLogs("dev", "default", "api", tailLines: 0));

        session.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task missing_request_identity_and_ungranted_default_cluster_are_denied()
    {
        var noIdentity = new McpTools(
            new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict).Object,
            new Mock<IMcpClusterSession>(MockBehavior.Strict).Object,
            new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object,
            new Mock<ISettingsService>(MockBehavior.Strict).Object);

        await Should.ThrowAsync<UnauthorizedAccessException>(() => noIdentity.ListResources("dev", "v1", "Pod"));

        var catalog = new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict);
        catalog.Setup(x => x.GetDefault()).Returns((IClusterRuntime?)null);
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        var limited = new McpTools(
            catalog.Object,
            session.Object,
            new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object,
            new Mock<ISettingsService>(MockBehavior.Strict).Object,
            identityAccessor: CreateExternalIdentityAccessor("dev"));

        await Should.ThrowAsync<UnauthorizedAccessException>(() => limited.ListResources(null, "v1", "Pod"));

        catalog.VerifyAll();
        session.VerifyNoOtherCalls();
    }

    [Fact]
    public void cluster_listing_fails_closed_without_request_identity()
    {
        var cluster = CreateRuntime("dev");
        var catalog = new Mock<IClusterRuntimeCatalog>(MockBehavior.Strict);
        catalog.SetupGet(x => x.Clusters).Returns([cluster.Object]);
        catalog.Setup(x => x.GetDefault()).Returns((IClusterRuntime?)null);
        var tools = new McpTools(
            catalog.Object,
            new Mock<IMcpClusterSession>(MockBehavior.Strict).Object,
            new Mock<IKubernetesYamlSerializer>(MockBehavior.Strict).Object,
            new Mock<ISettingsService>(MockBehavior.Strict).Object);

        tools.ListClusters().ShouldBeEmpty();
        catalog.VerifyAll();
    }

    [Fact]
    public async Task open_resource_list_delegates_to_ui_navigation()
    {
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        var navigation = new Mock<IResourceNavigationService>(MockBehavior.Strict);
        navigation.Setup(x => x.OpenResourceListAsync("dev", "apps/v1", "Deployment"))
            .ReturnsAsync(true);
        var tools = CreateTools(session, resourceNavigationService: navigation.Object);

        var opened = await tools.OpenResourceList("dev", "apps/v1", "Deployment");

        opened.ShouldBeTrue();
        navigation.VerifyAll();
    }

    [Fact]
    public async Task open_resource_list_rejects_invalid_resource_identifiers_before_navigation()
    {
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        var navigation = new Mock<IResourceNavigationService>(MockBehavior.Strict);
        var tools = CreateTools(session, resourceNavigationService: navigation.Object);

        await Should.ThrowAsync<ArgumentException>(() => tools.OpenResourceList(
            "dev", "apps/v1\r\nInjected: true", "Deployment"));

        navigation.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task open_resource_list_reports_missing_navigation_service()
    {
        var tools = CreateTools(new Mock<IMcpClusterSession>(MockBehavior.Strict));

        await Should.ThrowAsync<InvalidOperationException>(() => tools.OpenResourceList("dev", "apps/v1", "Deployment"));
    }

    [Fact]
    public async Task diff_resource_yaml_honors_cancellation_before_cluster_access()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        var tools = CreateTools(session);

        await Should.ThrowAsync<OperationCanceledException>(() => tools.DiffResourceYaml(
            "dev", "v1", "ConfigMap", "app-config", "default", "data: {}",
            cancellationToken: cancellation.Token));

        session.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task diff_resource_yaml_rejects_oversized_proposed_yaml_before_cluster_access()
    {
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        var tools = CreateTools(session);

        await Should.ThrowAsync<InvalidOperationException>(() => tools.DiffResourceYaml(
            "dev", "v1", "ConfigMap", "app-config", "default", new string('x', 1024 * 1024 + 1)));

        session.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task related_resources_delegates_to_cluster_relationship_model()
    {
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        var expected = new[]
        {
            new McpRelatedResourceInfo("apps/v1", "ReplicaSet", "api", "default", "Owner", null)
        };
        session.Setup(x => x.ListRelatedResourcesAsync("dev", "v1", "Pod", "api", "default", 25))
            .ReturnsAsync(expected);
        var tools = CreateTools(session);

        var actual = await tools.ListRelatedResources("dev", "v1", "Pod", "api", "default", 25);

        actual.ShouldBe(expected);
        session.VerifyAll();
    }

    [Fact]
    public async Task resource_graph_delegates_to_cluster_relationship_model()
    {
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        var expected = new McpResourceGraphInfo(
            [new McpResourceInfo("v1", "Pod", "api", "default", true)],
            [new McpRelatedResourceInfo("apps/v1", "ReplicaSet", "api", "default", "Owner", null)]);
        session.Setup(x => x.GetResourceGraphAsync("dev", "v1", "Pod", "api", "default", 25))
            .ReturnsAsync(expected);
        var tools = CreateTools(session);

        var actual = await tools.GetResourceGraph("dev", "v1", "Pod", "api", "default", 25);

        actual.ShouldBe(expected);
        session.VerifyAll();
    }

    [Fact]
    public async Task secret_yaml_requires_an_explicit_agent_permission()
    {
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        var permission = new Mock<IAgentPermissionService>(MockBehavior.Strict);
        permission.Setup(x => x.RequestPermissionAsync(
                It.Is<AgentPermissionRequest>(request => request.Action == "read_kubernetes_secret"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AgentPermissionResult(false, "denied"));
        var tools = CreateTools(session, permission.Object);

        await Should.ThrowAsync<UnauthorizedAccessException>(() => tools.GetResourceYaml(
            "dev", "v1", "Secret", "credentials", "default"));

        permission.VerifyAll();
        session.VerifyNoOtherCalls();
    }

    [AvaloniaFact]
    public async Task secret_yaml_diff_returns_content_only_after_permission_is_granted()
    {
        var secret = new V1Secret
        {
            Metadata = new V1ObjectMeta { Name = "credentials", NamespaceProperty = "default" },
            Data = new Dictionary<string, byte[]> { ["password"] = Encoding.UTF8.GetBytes("canary") }
        };
        using var workspace = await Application.Current.CreateClusterAsync(config => config.Resources = [secret]);
        var services = Application.Current.GetTestServices();
        var permission = new Mock<IAgentPermissionService>(MockBehavior.Strict);
        permission.Setup(value => value.RequestPermissionAsync(
                It.Is<AgentPermissionRequest>(request => request.Action == "read_kubernetes_secret"
                    && request.Resource == "default/credentials"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AgentPermissionResult(true, "approved"));
        var identityAccessor = services.GetRequiredService<McpRequestIdentityAccessor>();
        var tools = new McpTools(
            services.GetRequiredService<IClusterRuntimeCatalog>(),
            services.GetRequiredService<IMcpClusterSession>(),
            services.GetRequiredService<IKubernetesYamlSerializer>(),
            services.GetRequiredService<ISettingsService>(),
            permission.Object,
            identityAccessor: identityAccessor);
        using var identity = identityAccessor.Enter(new McpClientIdentity(
            "test", "MCP integration test", true, new HashSet<string>(StringComparer.Ordinal)));

        var diff = await tools.DiffResourceYaml(
            workspace.Runtime.Name, "v1", "Secret", "credentials", "default", "apiVersion: v1\nkind: Secret\ndata: changed");

        diff.ShouldContain("Current resource:");
        diff.ShouldContain("kind: Secret");
        diff.ShouldContain("Proposed resource:");
        diff.ShouldContain("data: changed");
        permission.Verify(value => value.RequestPermissionAsync(
            It.Is<AgentPermissionRequest>(request => request.Action == "read_kubernetes_secret"
                && request.Resource == "default/credentials"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task pod_logs_propagates_cancellation_from_log_request()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var operations = new Mock<ICoreV1Operations>(MockBehavior.Strict);
        operations.Setup(x => x.ReadNamespacedPodLogWithHttpMessagesAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<int?>(),
                It.IsAny<bool?>(), It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(), cancellation.Token))
            .Returns(Task.FromCanceled<k8s.Autorest.HttpOperationResponse<Stream>>(cancellation.Token));
        var runtime = new Mock<IClusterRuntime>(MockBehavior.Strict);
        var client = new Mock<IKubernetes>(MockBehavior.Strict);
        client.SetupGet(x => x.CoreV1).Returns(operations.Object);
        runtime.SetupGet(x => x.Client).Returns(client.Object);
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        session.Setup(x => x.GetConnectedClusterAsync(null)).ReturnsAsync(runtime.Object);

        var tools = CreateTools(session);

        await Should.ThrowAsync<OperationCanceledException>(() => tools.GetPodLogs(
            null, "default", "api", cancellationToken: cancellation.Token));
        operations.VerifyAll();
    }

    [Theory]
    [InlineData(8, false)]
    [InlineData(1024 * 1024 + 1, true)]
    public async Task pod_log_output_is_read_and_bounded(int byteCount, bool shouldExceedLimit)
    {
        var operations = new Mock<ICoreV1Operations>(MockBehavior.Strict);
        operations.Setup(x => x.ReadNamespacedPodLogWithHttpMessagesAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<int?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<int?>(),
                It.IsAny<bool?>(), It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var body = new MemoryStream(Encoding.UTF8.GetBytes(new string('x', byteCount)));
                return new k8s.Autorest.HttpOperationResponse<Stream>
                {
                    Request = new HttpRequestMessage(),
                    Body = body,
                };
            });
        var client = new Mock<IKubernetes>(MockBehavior.Strict);
        client.SetupGet(x => x.CoreV1).Returns(operations.Object);
        var runtime = new Mock<IClusterRuntime>(MockBehavior.Strict);
        runtime.SetupGet(x => x.Client).Returns(client.Object);
        var session = new Mock<IMcpClusterSession>(MockBehavior.Strict);
        session.Setup(x => x.GetConnectedClusterAsync("dev")).ReturnsAsync(runtime.Object);
        var tools = CreateTools(session);

        if (shouldExceedLimit)
        {
            await Should.ThrowAsync<InvalidOperationException>(() => tools.GetPodLogs("dev", "default", "api"));
        }
        else
        {
            (await tools.GetPodLogs("dev", "default", "api")).ShouldBe(new string('x', byteCount));
        }

        operations.VerifyAll();
        client.VerifyAll();
        session.VerifyAll();
    }

    [Fact]
    public void endpoint_tool_reports_the_port_actually_bound()
    {
        var serverState = new McpServerState();
        serverState.SetBoundPort(62889);
        var tools = CreateTools(
            new Mock<IMcpClusterSession>(MockBehavior.Strict),
            settings: new Settings { McpServerEnabled = true, McpServerPort = 62888 },
            mcpServerState: serverState);

        tools.GetEndpoint().ShouldBe("http://127.0.0.1:62889/mcp");
    }

    [Fact]
    public void endpoint_tool_rejects_an_unbound_server()
    {
        var tools = CreateTools(
            new Mock<IMcpClusterSession>(MockBehavior.Strict),
            settings: new Settings { McpServerEnabled = true, McpServerPort = 62888 });

        Should.Throw<InvalidOperationException>(() => tools.GetEndpoint());
    }

    [Fact]
    public void endpoint_tool_rejects_a_disabled_mcp_server()
    {
        var tools = CreateTools(
            new Mock<IMcpClusterSession>(MockBehavior.Strict),
            settings: new Settings { McpServerEnabled = false });

        Should.Throw<InvalidOperationException>(() => tools.GetEndpoint());
    }

    private static McpTools CreateTools(
        Mock<IMcpClusterSession> session,
        IAgentPermissionService? permissionService = null,
        IResourceNavigationService? resourceNavigationService = null,
        Settings? settings = null,
        IMcpServerState? mcpServerState = null)
    {
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings ?? new Settings());
        return new(
            new Mock<IClusterRuntimeCatalog>().Object,
            session.Object,
            new Mock<IKubernetesYamlSerializer>().Object,
            settingsService.Object,
            permissionService,
            resourceNavigationService,
            CreateInternalIdentityAccessor(),
            mcpServerState);
    }

    private static McpRequestIdentityAccessor CreateInternalIdentityAccessor()
        => new(new McpClientIdentity("test", "test", true, new HashSet<string>(StringComparer.Ordinal)));

    private static McpRequestIdentityAccessor CreateExternalIdentityAccessor(params string[] clusters)
        => new(new McpClientIdentity("external", "external test", false,
            new HashSet<string>(clusters, StringComparer.Ordinal)));

    private static Mock<IClusterRuntime> CreateRuntime(string name)
    {
        var runtime = new Mock<IClusterRuntime>(MockBehavior.Strict);
        runtime.SetupGet(x => x.Name).Returns(name);
        runtime.SetupGet(x => x.Connected).Returns(true);
        runtime.SetupGet(x => x.Status).Returns(ClusterStatus.Connected);
        return runtime;
    }
}
