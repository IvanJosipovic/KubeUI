using System.Reflection;
using System.Threading;
using k8s;
using k8s.Models;
using KubeUI.AI.Agents;
using KubeUI.AI.Permissions;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes.Serialization;
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
        session.Setup(x => x.ListResourcesAsync("dev", "v1", "Pod", "default", 10))
            .ReturnsAsync([
                new V1Pod
                {
                    Metadata = new V1ObjectMeta { Name = "api", NamespaceProperty = "default" }
                }
            ]);
        var tools = new McpTools(
            new Mock<IClusterRuntimeCatalog>().Object,
            session.Object,
            new Mock<IKubernetesYamlSerializer>().Object,
            new Mock<ISettingsService>().Object);

        var resources = await tools.ListResources("dev", "v1", "Pod", "default", 10);

        resources.ShouldBe([
            new McpResourceInfo("v1", "Pod", "api", "default", true)
        ]);
        session.VerifyAll();
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

    [Fact]
    public void endpoint_tool_uses_the_configured_port()
    {
        var tools = CreateTools(
            new Mock<IMcpClusterSession>(MockBehavior.Strict),
            settings: new Settings { McpServerEnabled = true, McpServerPort = 62888 });

        tools.GetEndpoint().ShouldBe("http://127.0.0.1:62888/mcp");
    }

    [Fact]
    public void endpoint_tool_uses_the_configured_port_without_bound_state()
    {
        var tools = CreateTools(
            new Mock<IMcpClusterSession>(MockBehavior.Strict),
            settings: new Settings { McpServerEnabled = true, McpServerPort = 62888 });

        tools.GetEndpoint().ShouldBe("http://127.0.0.1:62888/mcp");
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
        Settings? settings = null)
    {
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings ?? new Settings());
        return new(
            new Mock<IClusterRuntimeCatalog>().Object,
            session.Object,
            new Mock<IKubernetesYamlSerializer>().Object,
            settingsService.Object,
            permissionService,
            resourceNavigationService);
    }
}
