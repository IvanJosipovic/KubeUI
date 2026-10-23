using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Resources.Relationships;

namespace KubeUI.Avalonia.Infrastructure.Mcp;

public sealed record McpSupportedResourceInfo(
    string Name, string? Category, string Group, string ApiVersion, string FullApiVersion,
    string Kind, bool IsNamespaced, bool IsCustomResource, bool CanListAndWatch, bool PermissionsLoaded);

public sealed record McpResourceInfo(
    string ApiVersion, string Kind, string Name, string? Namespace, bool Ready);

/// <summary>One page of complete Kubernetes Event resources.</summary>
/// <param name="Events">Full Event objects in stable namespace, name, and UID order.</param>
/// <param name="NextCursor">Opaque cursor for the next page, or <see langword="null"/> when complete.</param>
public sealed record McpEventPage(
    IReadOnlyList<Corev1Event> Events,
    string? NextCursor);

public sealed record McpRelatedResourceInfo(
    string ApiVersion, string Kind, string Name, string? Namespace,
    string Relationship, string? RelationshipLabel);

public sealed record McpResourceGraphInfo(
    IReadOnlyList<McpResourceInfo> Resources,
    IReadOnlyList<McpRelatedResourceInfo> Relationships);

public interface IMcpClusterSession
{
    Task<IClusterRuntime> GetConnectedClusterAsync(string? clusterName);
    Task SeedResourceAsync(string? clusterName, GroupApiVersionKind resourceKind, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<McpSupportedResourceInfo>> ListSupportedResourcesAsync(string? clusterName);
    Task<McpResourcePage> ListResourcesAsync(
        string? clusterName, string apiVersion, string kind, string? @namespace, int limit,
        CancellationToken cancellationToken = default, string? cursor = null);
    Task<McpEventPage> ListEventsAsync(
        string? clusterName, string? @namespace, int limit,
        CancellationToken cancellationToken = default, string? cursor = null);
    Task<IReadOnlyList<McpRelatedResourceInfo>> ListRelatedResourcesAsync(
        string? clusterName, string apiVersion, string kind, string name, string? @namespace, int limit, CancellationToken cancellationToken = default);
    Task<McpResourceGraphInfo> GetResourceGraphAsync(
        string? clusterName, string apiVersion, string kind, string name, string? @namespace, int limit, CancellationToken cancellationToken = default);
}

internal sealed class McpClusterSession(
    IClusterRuntimeCatalog runtimeCatalog,
    ClusterWorkspaceCatalog workspaceCatalog,
    NavigationSectionCatalog navigationSections,
    ILogger<McpClusterSession> logger,
    IResourceRelationshipBuilder relationshipBuilder) : IMcpClusterSession
{
    private static readonly IComparer<IKubernetesObject<V1ObjectMeta>> ResourceOrder =
        Comparer<IKubernetesObject<V1ObjectMeta>>.Create(CompareResources);

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
            .Select(config => (Config: config, Section: navigationSections.Find(config.Navigation.SectionId)))
            .OrderBy(entry => entry.Section?.Order ?? int.MaxValue)
            .ThenBy(entry => entry.Section?.Name ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(entry => entry.Config.Order)
            .ThenBy(entry => entry.Config.Name, StringComparer.Ordinal)
            .Select(static entry => new McpSupportedResourceInfo(
                entry.Config.Name, entry.Section?.Name ?? string.Empty, entry.Config.Kind.Group, entry.Config.Kind.ApiVersion,
                entry.Config.Kind.GroupApiVersion, entry.Config.Kind.Kind, entry.Config.IsNamespaced,
                entry.Config.IsCustomResource, entry.Config.CanListAndWatch, entry.Config.PermissionsLoaded))];
    }

    public async Task SeedResourceAsync(string? clusterName, GroupApiVersionKind resourceKind, CancellationToken cancellationToken = default)
    {
        var cluster = await GetConnectedClusterAsync(clusterName).ConfigureAwait(false);
        var workspace = workspaceCatalog.GetCluster(cluster.Name)
            ?? throw new InvalidOperationException($"Cluster {cluster.Name} is not backed by a KubeUI workspace.");

        await workspace.GetResourceConfig(resourceKind).SeedResource(waitForReady: true, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<McpResourcePage> ListResourcesAsync(
        string? clusterName, string apiVersion, string kind, string? @namespace, int limit,
        CancellationToken cancellationToken = default, string? cursor = null)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (limit is < 1 or > 500)
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 500.");

        var decodedCursor = cursor is null ? null : McpResourceCursorCodec.Decode(cursor);
        if (decodedCursor is not null
            && (!string.Equals(decodedCursor.ApiVersion, apiVersion, StringComparison.Ordinal)
                || !string.Equals(decodedCursor.Kind, kind, StringComparison.Ordinal)
                || !string.Equals(decodedCursor.Namespace, @namespace, StringComparison.Ordinal)))
        {
            throw new ArgumentException("The cursor does not match this resource query.", nameof(cursor));
        }

        var cluster = await GetConnectedClusterAsync(clusterName).ConfigureAwait(false);
        if (decodedCursor is not null)
            McpResourceCursorCodec.EnsureMatches(decodedCursor, cluster.Name, apiVersion, kind, @namespace);

        var workspace = workspaceCatalog.GetCluster(cluster.Name)
            ?? throw new InvalidOperationException($"Cluster {cluster.Name} is not backed by a KubeUI workspace.");
        var resourceKind = ResolveResourceKind(cluster, workspace, apiVersion, kind);
        if (!cluster.ModelCatalog.Contains(resourceKind))
            throw new InvalidOperationException($"Unable to resolve Kubernetes resource for {apiVersion}/{kind}.");
        await workspace.GetResourceConfig(resourceKind).SeedResource(waitForReady: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (!cluster.Objects.TryGetValue(resourceKind, out var value)
            || value is not IResourceContainer container)
            return new McpResourcePage([], null);

        var after = decodedCursor is null
            ? (ResourceSortKey?)null
            : new ResourceSortKey(
                decodedCursor.LastNamespace ?? string.Empty,
                decodedCursor.LastName,
                decodedCursor.LastUid ?? string.Empty);
        var candidates = container.QueryTop(
            resource => (@namespace is null
                    || string.Equals(resource.Metadata?.NamespaceProperty, @namespace, StringComparison.Ordinal))
                && (after is null || CompareResourceToKey(resource, after.Value) > 0),
            ResourceOrder,
            limit + 1,
            cancellationToken);

        var resourceCount = Math.Min(limit, candidates.Count);
        var resources = new McpResourceInfo[resourceCount];
        for (var index = 0; index < resourceCount; index++)
        {
            var resource = candidates[index];
            resources[index] = new McpResourceInfo(
                apiVersion, kind, resource.Metadata?.Name ?? string.Empty,
                resource.Metadata?.NamespaceProperty, resource.Metadata?.Name is not null);
        }

        string? nextCursor = null;
        if (candidates.Count > limit)
        {
            var lastResource = candidates[resourceCount - 1];
            var metadata = lastResource.Metadata;
            nextCursor = McpResourceCursorCodec.Encode(
                cluster.Name, apiVersion, kind, @namespace,
                metadata?.NamespaceProperty, metadata?.Name ?? string.Empty, metadata?.Uid);
        }

        return new McpResourcePage(resources, nextCursor);
    }

    public async Task<McpEventPage> ListEventsAsync(
        string? clusterName,
        string? @namespace,
        int limit,
        CancellationToken cancellationToken = default,
        string? cursor = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (limit is < 1 or > 500)
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 500.");

        var decodedCursor = cursor is null ? null : McpResourceCursorCodec.Decode(cursor);
        if (decodedCursor is not null
            && (!string.Equals(decodedCursor.ApiVersion, "v1", StringComparison.Ordinal)
                || !string.Equals(decodedCursor.Kind, "Event", StringComparison.Ordinal)
                || !string.Equals(decodedCursor.Namespace, @namespace, StringComparison.Ordinal)))
        {
            throw new ArgumentException("The cursor does not match this Event query.", nameof(cursor));
        }

        var cluster = await GetConnectedClusterAsync(clusterName).ConfigureAwait(false);
        if (decodedCursor is not null)
            McpResourceCursorCodec.EnsureMatches(decodedCursor, cluster.Name, "v1", "Event", @namespace);

        var workspace = workspaceCatalog.GetCluster(cluster.Name)
            ?? throw new InvalidOperationException($"Cluster {cluster.Name} is not backed by a KubeUI workspace.");
        var resourceKind = GroupApiVersionKind.From<Corev1Event>();
        await workspace.GetResourceConfig(resourceKind)
            .SeedResource(waitForReady: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (!cluster.Objects.TryGetValue(resourceKind, out var value)
            || value is not IResourceContainer container)
        {
            return new McpEventPage([], null);
        }

        var after = decodedCursor is null
            ? (ResourceSortKey?)null
            : new ResourceSortKey(
                decodedCursor.LastNamespace ?? string.Empty,
                decodedCursor.LastName,
                decodedCursor.LastUid ?? string.Empty);
        var candidates = container.QueryTop(
            resource => (@namespace is null
                    || string.Equals(resource.Metadata?.NamespaceProperty, @namespace, StringComparison.Ordinal))
                && (after is null || CompareResourceToKey(resource, after.Value) > 0),
            ResourceOrder,
            limit + 1,
            cancellationToken);

        var eventCount = Math.Min(limit, candidates.Count);
        var events = new Corev1Event[eventCount];
        for (var index = 0; index < eventCount; index++)
        {
            events[index] = (Corev1Event)candidates[index];
        }

        string? nextCursor = null;
        if (candidates.Count > limit)
        {
            var metadata = candidates[eventCount - 1].Metadata;
            nextCursor = McpResourceCursorCodec.Encode(
                cluster.Name, "v1", "Event", @namespace,
                metadata?.NamespaceProperty, metadata?.Name ?? string.Empty, metadata?.Uid);
        }

        return new McpEventPage(events, nextCursor);
    }

    public async Task<IReadOnlyList<McpRelatedResourceInfo>> ListRelatedResourcesAsync(
        string? clusterName, string apiVersion, string kind, string name, string? @namespace, int limit, CancellationToken cancellationToken = default)
    {
        var graph = await BuildResourceGraphAsync(clusterName, apiVersion, kind, name, @namespace, limit, cancellationToken).ConfigureAwait(false);
        return graph.Relationships;
    }

    public async Task<McpResourceGraphInfo> GetResourceGraphAsync(
        string? clusterName, string apiVersion, string kind, string name, string? @namespace, int limit, CancellationToken cancellationToken = default)
    {
        return await BuildResourceGraphAsync(clusterName, apiVersion, kind, name, @namespace, limit, cancellationToken).ConfigureAwait(false);
    }

    private async Task<McpResourceGraphInfo> BuildResourceGraphAsync(
        string? clusterName, string apiVersion, string kind, string name, string? @namespace, int limit, CancellationToken cancellationToken)
    {
        if (limit is < 1 or > 500)
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 500.");

        var cluster = await GetConnectedClusterAsync(clusterName).ConfigureAwait(false);
        var workspace = workspaceCatalog.GetCluster(cluster.Name)
            ?? throw new InvalidOperationException($"Cluster {cluster.Name} is not backed by a KubeUI workspace.");
        var resourceKind = ResolveResourceKind(cluster, workspace, apiVersion, kind);
        if (!cluster.ModelCatalog.Contains(resourceKind))
            throw new InvalidOperationException($"Unable to resolve Kubernetes resource for {apiVersion}/{kind}.");
        await workspace.GetResourceConfig(resourceKind).SeedResource(waitForReady: true, cancellationToken: cancellationToken).ConfigureAwait(false);

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

        var graph = relationshipBuilder.Build(resources, new HashSet<string>(StringComparer.Ordinal), hideNoise: false);
        foreach (var prerequisite in graph.RequiredSeedPrerequisites)
        {
            var config = workspace.GetResourceConfigs().FirstOrDefault(candidate =>
                candidate.Kind.Kind == prerequisite.Kind.Kind
                && (prerequisite.MatchAnyApiGroup || candidate.Kind.Group == prerequisite.Kind.Group)
                && (prerequisite.AllowServedVersionFallback || candidate.Kind.ApiVersion == prerequisite.Kind.ApiVersion));
            if (config is not null)
                await config.SeedResource(waitForReady: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        resources = cluster.Objects.Values
            .OfType<IResourceContainer>()
            .SelectMany(static container => container.Snapshot())
            .Distinct()
            .ToArray();
        graph = relationshipBuilder.Build(resources, new HashSet<string>(StringComparer.Ordinal), hideNoise: false);
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

    private static GroupApiVersionKind ResolveResourceKind(
        IClusterRuntime cluster,
        ClusterWorkspace workspace,
        string apiVersion,
        string kind)
    {
        var parts = apiVersion.Split('/', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var group = parts.Length == 2 ? parts[0] : string.Empty;
        var version = parts.Length == 2 ? parts[1] : apiVersion;
        if (cluster.ModelCatalog.TryGetResourceKind(apiVersion, kind, out var registeredKind))
            return registeredKind;

        var requestedKind = new GroupApiVersionKind(group, version, kind, string.Empty);

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
            return candidate;
        }

        return requestedKind;
    }

    private readonly record struct ResourceSortKey(string Namespace, string Name, string Uid);
    private static int CompareResources(
        IKubernetesObject<V1ObjectMeta> left,
        IKubernetesObject<V1ObjectMeta> right)
        => CompareResourceKeys(GetResourceSortKey(left), GetResourceSortKey(right));

    private static int CompareResourceToKey(IKubernetesObject<V1ObjectMeta> resource, ResourceSortKey key)
        => CompareResourceKeys(GetResourceSortKey(resource), key);

    private static ResourceSortKey GetResourceSortKey(IKubernetesObject<V1ObjectMeta> resource)
        => new(
            resource.Metadata?.NamespaceProperty ?? string.Empty,
            resource.Metadata?.Name ?? string.Empty,
            resource.Metadata?.Uid ?? string.Empty);

    private static int CompareResourceKeys(ResourceSortKey left, ResourceSortKey right)
    {
        var comparison = StringComparer.Ordinal.Compare(left.Namespace, right.Namespace);
        if (comparison != 0)
            return comparison;

        comparison = StringComparer.Ordinal.Compare(left.Name, right.Name);
        return comparison != 0
            ? comparison
            : StringComparer.Ordinal.Compare(left.Uid, right.Uid);
    }
}
