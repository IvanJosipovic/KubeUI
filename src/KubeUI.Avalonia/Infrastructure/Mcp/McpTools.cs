using System.ComponentModel;
using k8s;
using k8s.Models;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.AI.Agents;
using KubeUI.AI.Permissions;
using KubernetesClient.Informer.Client;
using ModelContextProtocol.Server;

namespace KubeUI.Avalonia.Infrastructure.Mcp;

public sealed record McpClusterInfo(string Name, string KubeConfigPath, bool IsDefault, bool Connected, string Status, string? LastError, bool RequiresNamespaceSelectionPrompt);

[McpServerToolType]
public sealed class McpTools(
    IClusterRuntimeCatalog clusterCatalog,
    IMcpClusterSession clusterSession,
    IKubernetesYamlSerializer yamlSerializer,
    Services.Settings.ISettingsService settingsService,
    IAgentPermissionService? permissionService = null,
    IResourceNavigationService? resourceNavigationService = null)
{
    private readonly IAgentPermissionService _permissionService = permissionService ?? new DenyByDefaultAgentPermissionService();
    private readonly IResourceNavigationService? _resourceNavigationService = resourceNavigationService;
    [McpServerTool(Name = "kubeui_list_clusters", Title = "List KubeUI clusters", Destructive = false, ReadOnly = true, Idempotent = true), Description("Lists Kubernetes clusters known to KubeUI.")]
    public IReadOnlyList<McpClusterInfo> ListClusters()
    {
        var defaultName = clusterCatalog.GetDefault()?.Name;
        return [.. clusterCatalog.Clusters.Select(cluster => ToClusterInfo(cluster, defaultName))];
    }

    [McpServerTool(Name = "kubeui_connect_cluster", Title = "Connect KubeUI cluster", Destructive = false, Idempotent = true), Description("Connects a KubeUI Kubernetes cluster.")]
    public async Task<McpClusterInfo> ConnectCluster([Description("Cluster name; omit for default.")] string? cluster = null)
    {
        var runtime = await clusterSession.GetConnectedClusterAsync(cluster).ConfigureAwait(false);
        return ToClusterInfo(runtime, clusterCatalog.GetDefault()?.Name);
    }

    [McpServerTool(Name = "kubeui_list_supported_resources", Title = "List supported Kubernetes resources", Destructive = false, ReadOnly = true, Idempotent = true), Description("Lists resources supported by KubeUI.")]
    public Task<IReadOnlyList<McpSupportedResourceInfo>> ListSupportedResources(string? cluster = null)
        => clusterSession.ListSupportedResourcesAsync(cluster);

    [McpServerTool(Name = "kubeui_list_resources", Title = "List Kubernetes resources", Destructive = false, ReadOnly = true, Idempotent = true), Description("Lists cached Kubernetes resources known to KubeUI. Call kubeui_list_supported_resources first and pass its FullApiVersion exactly, including the API group for non-core resources.")]
    public async Task<IReadOnlyList<McpResourceInfo>> ListResources(
        string? cluster, string apiVersion, string kind, string? @namespace = null, int limit = 100)
    {
        var resources = await clusterSession.ListResourcesAsync(cluster, apiVersion, kind, @namespace, limit).ConfigureAwait(false);
        return [.. resources.Select(resource => new McpResourceInfo(
            apiVersion, kind, resource.Metadata?.Name ?? string.Empty,
            resource.Metadata?.NamespaceProperty,
            resource.Metadata?.Name is not null))];
    }

    [McpServerTool(Name = "kubeui_list_events", Title = "List Kubernetes events", Destructive = false, ReadOnly = true, Idempotent = true), Description("Lists Kubernetes Events cached by KubeUI.")]
    public Task<IReadOnlyList<McpResourceInfo>> ListEvents(string? cluster = null, string? @namespace = null, int limit = 100)
        => ListResources(cluster, "v1", "Event", @namespace, limit);

    [McpServerTool(Name = "kubeui_related_resources", Title = "List related Kubernetes resources", Destructive = false, ReadOnly = true, Idempotent = true), Description("Lists resources directly related to a Kubernetes resource through KubeUI's relationship model.")]
    public Task<IReadOnlyList<McpRelatedResourceInfo>> ListRelatedResources(
        string? cluster, string apiVersion, string kind, string name, string? @namespace = null, int limit = 100)
        => clusterSession.ListRelatedResourcesAsync(cluster, apiVersion, kind, name, @namespace, limit);

    [McpServerTool(Name = "kubeui_resource_graph", Title = "Show Kubernetes resource graph", Destructive = false, ReadOnly = true, Idempotent = true), Description("Returns the selected Kubernetes resource and its directly related resources.")]
    public Task<McpResourceGraphInfo> GetResourceGraph(
        string? cluster, string apiVersion, string kind, string name, string? @namespace = null, int limit = 100)
        => clusterSession.GetResourceGraphAsync(cluster, apiVersion, kind, name, @namespace, limit);

    [McpServerTool(Name = "kubeui_diff_resource_yaml", Title = "Compare resource YAML", Destructive = false, ReadOnly = true, Idempotent = true), Description("Compares a live KubeUI resource YAML document with a proposed YAML document.")]
    public async Task<string> DiffResourceYaml(
        string? cluster, string apiVersion, string kind, string name, string? @namespace, string proposedYaml)
    {
        var currentYaml = await GetResourceYaml(cluster, apiVersion, kind, name, @namespace).ConfigureAwait(false);
        if (string.Equals(currentYaml, proposedYaml, StringComparison.Ordinal))
            return "No changes.";

        return $"Current resource:\n{currentYaml}\n\nProposed resource:\n{proposedYaml}";
    }

    [McpServerTool(Name = "kubeui_get_pod_logs", Title = "Get pod logs", Destructive = false, ReadOnly = true, Idempotent = true), Description("Gets logs for a Kubernetes Pod through the connected KubeUI cluster.")]
    public async Task<string> GetPodLogs(
        string? cluster, string @namespace, string pod, string? container = null, int? tailLines = 200, bool previous = false)
    {
        if (tailLines is < 1 or > 10000)
            throw new ArgumentOutOfRangeException(nameof(tailLines), "tailLines must be between 1 and 10000.");
        var runtime = await clusterSession.GetConnectedClusterAsync(cluster).ConfigureAwait(false);
        if (runtime.Client is null)
            throw new InvalidOperationException("The Kubernetes client is not connected.");
        await using var stream = await runtime.Client.CoreV1.ReadNamespacedPodLogAsync(
            pod, @namespace, container: container, tailLines: tailLines, previous: previous,
            cancellationToken: CancellationToken.None).ConfigureAwait(false);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    [McpServerTool(Name = "kubeui_get_resource_yaml", Title = "Get resource YAML", Destructive = false, ReadOnly = true, Idempotent = true), Description("Gets a Kubernetes resource as YAML.")]
    public async Task<string> GetResourceYaml(string? cluster, string apiVersion, string kind, string name, string? @namespace = null)
    {
        if (string.Equals(kind, "Secret", StringComparison.OrdinalIgnoreCase))
        {
            var permission = await _permissionService.RequestPermissionAsync(
                new AgentPermissionRequest("read_kubernetes_secret", $"{@namespace ?? "cluster-scoped"}/{name}", IsDestructive: true)).ConfigureAwait(false);
            if (!permission.Allowed)
                throw new UnauthorizedAccessException(permission.Reason ?? "Reading Kubernetes Secret data was denied.");
        }

        var runtime = await clusterSession.GetConnectedClusterAsync(cluster).ConfigureAwait(false);
        if (!runtime.ModelCatalog.TryGetResourceKind(apiVersion, kind, out var resourceKind))
            throw new InvalidOperationException($"Unable to resolve Kubernetes resource for {apiVersion}/{kind}.");
        await clusterSession.SeedResourceAsync(cluster, resourceKind).ConfigureAwait(false);
        var resource = runtime.Objects.TryGetValue(resourceKind, out var container)
            && container is IResourceContainer resourceContainer
            ? resourceContainer.Snapshot().FirstOrDefault(item =>
                string.Equals(item.Metadata?.Name, name, StringComparison.Ordinal)
                && string.Equals(item.Metadata?.NamespaceProperty, @namespace, StringComparison.Ordinal))
            : null;
        var typedResource = resource
            ?? throw new InvalidOperationException($"Resource {apiVersion}/{kind} {@namespace}/{name} was not found.");
        return yamlSerializer.Serialize(typedResource);
    }

    [McpServerTool(Name = "kubeui_get_mcp_endpoint", Title = "Get KubeUI MCP endpoint", Destructive = false, ReadOnly = true, Idempotent = true), Description("Returns local KubeUI MCP endpoint.")]
    public string GetEndpoint()
    {
        if (!settingsService.Settings.McpServerEnabled)
            throw new InvalidOperationException("The embedded MCP server is disabled in KubeUI settings.");
        return McpServerConfiguration.GetEndpoint(settingsService.Settings);
    }

    [McpServerTool(Name = "kubeui_open_resource_list", Title = "Open Kubernetes resource list", Destructive = false, ReadOnly = true, Idempotent = true), Description("Opens the KubeUI resource list for a Kubernetes resource type.")]
    public Task<bool> OpenResourceList(string? cluster, string apiVersion, string kind)
        => _resourceNavigationService?.OpenResourceListAsync(cluster, apiVersion, kind)
            ?? Task.FromException<bool>(new InvalidOperationException("KubeUI navigation is not available."));

    private static McpClusterInfo ToClusterInfo(IClusterRuntime cluster, string? defaultName) => new(
        cluster.Name, cluster.KubeConfigPath, string.Equals(cluster.Name, defaultName, StringComparison.Ordinal),
        cluster.Connected, cluster.Status.ToString(), cluster.LastError, false);
}
