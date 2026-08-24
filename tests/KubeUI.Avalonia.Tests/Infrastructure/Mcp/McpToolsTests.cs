using System.Reflection;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.AI.Agents;
using KubeUI.AI.Permissions;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;
using Moq;
using ModelContextProtocol.Server;
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
    public void get_endpoint_prefers_live_endpoint_over_configured_settings()
    {
        var settingsService = new Mock<ISettingsService>(MockBehavior.Strict);
        settingsService.SetupGet(service => service.Settings).Returns(new Settings
        {
            McpServerEnabled = true,
            McpServerPort = 62888
        });
        var serverState = new McpServerState();
        serverState.SetEndpoint("http://127.0.0.1:54321/mcp");
        var tools = CreateTools(
            new Mock<IMcpClusterSession>(MockBehavior.Strict),
            settingsService: settingsService.Object,
            serverState: serverState);

        tools.GetEndpoint().ShouldBe("http://127.0.0.1:54321/mcp");
    }

    [Fact]
    public void get_endpoint_uses_configured_endpoint_when_server_state_has_no_live_endpoint()
    {
        var settingsService = new Mock<ISettingsService>(MockBehavior.Strict);
        settingsService.SetupGet(service => service.Settings).Returns(new Settings
        {
            McpServerEnabled = true,
            McpServerPort = 62888
        });
        var tools = CreateTools(
            new Mock<IMcpClusterSession>(MockBehavior.Strict),
            settingsService: settingsService.Object);

        tools.GetEndpoint().ShouldBe("http://127.0.0.1:62888/mcp");
    }

    private static McpTools CreateTools(
        Mock<IMcpClusterSession> session,
        IAgentPermissionService? permissionService = null,
        IResourceNavigationService? resourceNavigationService = null,
        ISettingsService? settingsService = null,
        McpServerState? serverState = null)
        => new(
            new Mock<IClusterRuntimeCatalog>().Object,
            session.Object,
            new Mock<IKubernetesYamlSerializer>().Object,
            settingsService ?? new Mock<ISettingsService>().Object,
            permissionService,
            resourceNavigationService,
            serverState);
}
