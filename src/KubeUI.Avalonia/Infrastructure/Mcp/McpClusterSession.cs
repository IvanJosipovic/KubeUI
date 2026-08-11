using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Resources.Relationships;

namespace KubeUI.Avalonia.Infrastructure.Mcp;

public sealed record McpSupportedResourceInfo(
    string Name, string? Category, string Group, string ApiVersion, string FullApiVersion,
    string Kind, bool IsNamespaced, bool IsCustomResource, bool CanListAndWatch, bool PermissionsLoaded);

public sealed record McpResourceInfo(
    string ApiVersion, string Kind, string Name, string? Namespace, bool Ready);

public sealed record McpRelatedResourceInfo(
    string ApiVersion, string Kind, string Name, string? Namespace,
    string Relationship, string? RelationshipLabel);

public sealed record McpResourceGraphInfo(
    IReadOnlyList<McpResourceInfo> Resources,
    IReadOnlyList<McpRelatedResourceInfo> Relationships);

public interface IMcpClusterSession
{
    Task<IClusterRuntime> GetConnectedClusterAsync(string? clusterName);
    Task<IReadOnlyList<McpSupportedResourceInfo>> ListSupportedResourcesAsync(string? clusterName);
    Task<IReadOnlyList<IKubernetesObject<V1ObjectMeta>>> ListResourcesAsync(
        string? clusterName, string apiVersion, string kind, string? @namespace, int limit);
    Task<IReadOnlyList<McpRelatedResourceInfo>> ListRelatedResourcesAsync(
        string? clusterName, string apiVersion, string kind, string name, string? @namespace, int limit);
    Task<McpResourceGraphInfo> GetResourceGraphAsync(
        string? clusterName, string apiVersion, string kind, string name, string? @namespace, int limit);
}

internal sealed class McpClusterSession(
    IClusterRuntimeCatalog runtimeCatalog,
    ClusterWorkspaceCatalog workspaceCatalog,
    ILogger<McpClusterSession> logger) : IMcpClusterSession
{
    public async Task<IClusterRuntime> GetConnectedClusterAsync(string? clusterName)
    {
        var runtime = ResolveCluster(clusterName);
        var workspace = workspaceCatalog.GetCluster(runtime.Name)
            ?? throw new InvalidOperationException($"Cluster {runtime.Name} was not found.");
        if (!workspace.Runtime.Connected)
        {
            logger.LogInformation("Connecting cluster {ClusterName} for MCP request", workspace.Runtime.Name);
            await workspace.Connect().ConfigureAwait(false);
        }
        else
            await workspace.Connect().ConfigureAwait(false);
        if (!workspace.Runtime.Connected)
            throw new InvalidOperationException(workspace.Runtime.LastError ?? $"Unable to connect to cluster {workspace.Runtime.Name}.");
        return workspace.Runtime;
    }

    public async Task<IReadOnlyList<McpSupportedResourceInfo>> ListSupportedResourcesAsync(string? clusterName)
    {
        var cluster = await GetConnectedClusterAsync(clusterName).ConfigureAwait(false);
        var workspace = workspaceCatalog.GetCluster(cluster.Name);
        if (workspace is null)
            throw new InvalidOperationException($"Cluster {cluster.Name} is not backed by a KubeUI workspace.");
        return [.. workspace.GetResourceConfigs()
            .OrderBy(config => config.Category ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(config => config.Order)
            .ThenBy(config => config.Name, StringComparer.Ordinal)
            .Select(static config => new McpSupportedResourceInfo(
                config.Name, config.Category, config.Kind.Group, config.Kind.ApiVersion,
                config.Kind.GroupApiVersion, config.Kind.Kind, config.IsNamespaced,
                config.IsCustomResource, config.CanListAndWatch, config.PermissionsLoaded))];
    }

    public async Task<IReadOnlyList<IKubernetesObject<V1ObjectMeta>>> ListResourcesAsync(
        string? clusterName, string apiVersion, string kind, string? @namespace, int limit)
    {
        if (limit is < 1 or > 500)
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 500.");

        var cluster = await GetConnectedClusterAsync(clusterName).ConfigureAwait(false);
        var workspace = workspaceCatalog.GetCluster(cluster.Name)
            ?? throw new InvalidOperationException($"Cluster {cluster.Name} is not backed by a KubeUI workspace.");
        var (resourceType, resourceKind) = ResolveResourceType(cluster, workspace, apiVersion, kind);
        resourceType = resourceType
            ?? throw new InvalidOperationException($"Unable to resolve Kubernetes type for {apiVersion}/{kind}.");
        await cluster.SeedResource(resourceType).ConfigureAwait(false);
        if (!cluster.Objects.TryGetValue(resourceKind, out var value)
            || value is not IResourceContainer container)
            return [];

        return [.. container.Snapshot()
            .Where(item => @namespace is null || string.Equals(item.Metadata?.NamespaceProperty, @namespace, StringComparison.Ordinal))
            .OrderBy(item => item.Metadata?.NamespaceProperty, StringComparer.Ordinal)
            .ThenBy(item => item.Metadata?.Name, StringComparer.Ordinal)
            .Take(limit)];
    }

    public async Task<IReadOnlyList<McpRelatedResourceInfo>> ListRelatedResourcesAsync(
        string? clusterName, string apiVersion, string kind, string name, string? @namespace, int limit)
    {
        var graph = await BuildResourceGraphAsync(clusterName, apiVersion, kind, name, @namespace, limit).ConfigureAwait(false);
        return graph.Relationships;
    }

    public async Task<McpResourceGraphInfo> GetResourceGraphAsync(
        string? clusterName, string apiVersion, string kind, string name, string? @namespace, int limit)
    {
        return await BuildResourceGraphAsync(clusterName, apiVersion, kind, name, @namespace, limit).ConfigureAwait(false);
    }

    private async Task<McpResourceGraphInfo> BuildResourceGraphAsync(
        string? clusterName, string apiVersion, string kind, string name, string? @namespace, int limit)
    {
        if (limit is < 1 or > 500)
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 500.");

        var cluster = await GetConnectedClusterAsync(clusterName).ConfigureAwait(false);
        var workspace = workspaceCatalog.GetCluster(cluster.Name)
            ?? throw new InvalidOperationException($"Cluster {cluster.Name} is not backed by a KubeUI workspace.");
        var (resourceType, resourceKind) = ResolveResourceType(cluster, workspace, apiVersion, kind);
        resourceType = resourceType
            ?? throw new InvalidOperationException($"Unable to resolve Kubernetes type for {apiVersion}/{kind}.");
        await cluster.SeedResource(resourceType).ConfigureAwait(false);

        var resources = cluster.Objects.Values
            .OfType<IResourceContainer>()
            .SelectMany(static container => container.Snapshot())
            .Distinct()
            .ToArray();
        var selected = resources.FirstOrDefault(resource =>
            string.Equals(resource.ApiVersion, resourceKind.GroupApiVersion, StringComparison.Ordinal)
            && string.Equals(resource.Kind, kind, StringComparison.Ordinal)
            && string.Equals(resource.Name(), name, StringComparison.Ordinal)
            && string.Equals(resource.Namespace(), @namespace, StringComparison.Ordinal));
        if (selected is null)
            throw new InvalidOperationException($"Resource {apiVersion}/{kind} {@namespace}/{name} was not found.");

        var graph = new ResourceRelationshipBuilder().Build(resources, new HashSet<string>(StringComparer.Ordinal), hideNoise: false);
        var selectedIdentity = new ResourceIdentity(
            selected.ApiVersion ?? string.Empty, selected.Kind ?? string.Empty,
            selected.Namespace(), selected.Name() ?? string.Empty, selected.Uid());
        var related = graph.Relationships
            .Where(relationship => relationship.Source == selectedIdentity || relationship.Target == selectedIdentity)
            .Take(limit)
            .Select(relationship =>
            {
                var identity = relationship.Source == selectedIdentity ? relationship.Target : relationship.Source;
                return new McpRelatedResourceInfo(
                    identity.ApiVersion, identity.Kind, identity.Name, identity.Namespace,
                    relationship.Kind.ToString(), relationship.Label);
            })
            .ToArray();
        var relatedIdentities = related
            .Select(static item => new ResourceIdentity(item.ApiVersion, item.Kind, item.Namespace, item.Name, null))
            .ToHashSet();
        var graphResources = resources
            .Where(resource =>
            {
                var identity = new ResourceIdentity(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty,
                    resource.Namespace(), resource.Name() ?? string.Empty, resource.Uid());
                return identity == selectedIdentity || relatedIdentities.Any(item =>
                    item.ApiVersion == identity.ApiVersion && item.Kind == identity.Kind
                    && item.Namespace == identity.Namespace && item.Name == identity.Name);
            })
            .Select(resource => new McpResourceInfo(
                resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty,
                resource.Name() ?? string.Empty, resource.Namespace(), resource.Name() is not null))
            .ToArray();
        return new McpResourceGraphInfo(graphResources, related);
    }

    private IClusterRuntime ResolveCluster(string? clusterName)
    {
        if (!string.IsNullOrWhiteSpace(clusterName))
            return runtimeCatalog.GetCluster(clusterName) ?? throw new InvalidOperationException($"Cluster {clusterName} was not found.");
        return runtimeCatalog.GetDefault() ?? runtimeCatalog.Clusters.FirstOrDefault()
            ?? throw new InvalidOperationException("No clusters are configured in KubeUI.");
    }

    private static (Type? ResourceType, GroupApiVersionKind ResourceKind) ResolveResourceType(
        IClusterRuntime cluster,
        ClusterWorkspace workspace,
        string apiVersion,
        string kind)
    {
        var parts = apiVersion.Split('/', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var group = parts.Length == 2 ? parts[0] : string.Empty;
        var version = parts.Length == 2 ? parts[1] : apiVersion;
        var requestedKind = new GroupApiVersionKind(group, version, kind, string.Empty);
        var resourceType = cluster.ModelCatalog.GetResourceType(requestedKind);
        if (resourceType is not null)
            return (resourceType, requestedKind);

        // Agents sometimes pass only "v1" for a custom resource. Recover that
        // form only when KubeUI has one unambiguous registered kind/version.
        var candidates = workspace.GetResourceConfigs()
            .Select(config => config.Kind)
            .Where(candidate =>
                string.Equals(candidate.Kind, kind, StringComparison.Ordinal)
                && string.Equals(candidate.ApiVersion, version, StringComparison.Ordinal)
                && (string.IsNullOrEmpty(group)
                    || string.Equals(candidate.Group, group, StringComparison.Ordinal)))
            .Distinct()
            .ToArray();
        if (candidates.Length == 1)
        {
            var candidate = candidates[0];
            return (cluster.ModelCatalog.GetResourceType(candidate), candidate);
        }

        return (null, requestedKind);
    }
}
