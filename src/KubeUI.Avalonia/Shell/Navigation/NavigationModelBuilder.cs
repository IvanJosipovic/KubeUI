using FluentIcons.Common;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Shell.Navigation;

/// <summary>Computes the desired navigation entries for a connected cluster.</summary>
internal sealed class NavigationModelBuilder
{
    private readonly NavigationSectionCatalog _sections;
    private readonly ILogger _logger;

    public NavigationModelBuilder(NavigationSectionCatalog sections, ILogger logger)
    {
        _sections = sections;
        _logger = logger;
    }

    /// <summary>Builds the full desired navigation entry set for a cluster.</summary>
    public IReadOnlyList<NavigationEntry> Build(ClusterWorkspace cluster)
    {
        List<NavigationEntry> entries = [];
        if (cluster.Runtime.Status != ClusterStatus.Connected)
        {
            return entries;
        }

        var canListDefinitions = CanListCustomResourceDefinitions(cluster);
        foreach (var resource in GetCurrentResources(cluster))
        {
            var available = resource.PermissionsLoaded && resource.CanListAndWatch;
            if (resource.IsCustomResource())
            {
                available &= canListDefinitions;
            }

            if (!available)
            {
                continue;
            }

            AddCanonical(entries, cluster, resource);
            AddAliases(entries, cluster, resource);
        }

        foreach (var provider in _sections.Providers)
        {
            foreach (var document in provider.Documents)
            {
                if (document.IsAvailable(new NavigationDocumentContext(cluster)))
                {
                    entries.Add(new DocumentEntry(
                        NavigationIds.Document(cluster, provider.Section.Id, document.Id),
                        document.Name,
                        document.Order,
                        document.Icon,
                        provider.Section,
                        [],
                        document.Id));
                }
            }
        }

        AddPortForwarders(entries, cluster);
        return entries;
    }

    private void AddCanonical(List<NavigationEntry> entries, ClusterWorkspace cluster, IResourceConfig resource)
    {
        var placement = resource.Navigation;
        if (resource.IsCustomResource()
            && !string.Equals(placement.SectionId, NavigationSections.CustomResourcesId, StringComparison.Ordinal))
        {
            placement = NavigationPlacement.InSection(
                NavigationSections.CustomResourcesId,
                NavigationTreeEditor.ConstructCustomResourceGroups(resource.Kind.Group));
        }

        var section = _sections.Find(placement.SectionId);
        entries.Add(new ResourceEntry(
            NavigationIds.Canonical(cluster, resource.Kind),
            placement.Name ?? resource.Name,
            placement.Order ?? resource.Order,
            null,
            section,
            placement.Groups,
            resource.Kind,
            IsAlias: false));
    }

    private void AddAliases(List<NavigationEntry> entries, ClusterWorkspace cluster, IResourceConfig resource)
    {
        var configuredPlacement = resource.Navigation;
        if (resource.IsCustomResource()
            && !string.Equals(configuredPlacement.SectionId, NavigationSections.CustomResourcesId, StringComparison.Ordinal))
        {
            var section = _sections.Find(configuredPlacement.SectionId);
            if (configuredPlacement.SectionId is not null && section is null)
            {
                throw new InvalidOperationException(
                    $"Custom resource navigation targets unknown section '{configuredPlacement.SectionId}'.");
            }

            entries.Add(new ResourceEntry(
                NavigationIds.Alias(cluster, configuredPlacement.SectionId ?? "root", resource.Kind),
                configuredPlacement.Name ?? resource.Name,
                configuredPlacement.Order ?? resource.Order,
                null,
                section,
                configuredPlacement.Groups,
                resource.Kind,
                IsAlias: true));
        }

        foreach (var provider in _sections.Providers)
        {
            var placement = provider.Place(resource);
            if (placement is null)
            {
                continue;
            }

            if (!string.Equals(placement.SectionId, provider.Section.Id, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Navigation placement for section '{provider.Section.Id}' targets '{placement.SectionId ?? "<root>"}'.");
            }

            entries.Add(new ResourceEntry(
                NavigationIds.Alias(cluster, provider.Section.Id, resource.Kind),
                placement.Name ?? resource.Name,
                placement.Order ?? resource.Order,
                null,
                provider.Section,
                placement.Groups,
                resource.Kind,
                IsAlias: true));
        }
    }

    private void AddPortForwarders(List<NavigationEntry> entries, ClusterWorkspace cluster)
    {
        var podKind = GroupApiVersionKind.From<V1Pod>();
        var podConfig = cluster.GetResourceConfigs().FirstOrDefault(config => config.Kind == podKind);
        if (podConfig is not { PermissionsLoaded: true, CanListAndWatch: true } || !CanCreatePortForward(cluster, podKind))
        {
            return;
        }

        entries.Add(new BuiltinEntry(
            NavigationIds.Builtin(cluster, NavigationTargets.PortForwarders),
            Assets.Resources.PortForwarderListView_Title!,
            -450,
            Icon.CloudFlow,
            _sections.Find(NavigationSections.NetworkId),
            [],
            NavigationTargets.PortForwarders));
    }

    private bool CanCreatePortForward(ClusterWorkspace cluster, GroupApiVersionKind podKind)
    {
        try
        {
            return cluster.Runtime.Permissions.CanIAnyNamespace(podKind, true, Verb.Create, "portforward");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to evaluate port forward permissions for cluster {Cluster}", cluster.Runtime.Name);
            return false;
        }
    }

    private static bool CanListCustomResourceDefinitions(ClusterWorkspace cluster)
    {
        var definitionConfig = cluster.GetResourceConfigs()
            .FirstOrDefault(config => config.Kind == GroupApiVersionKind.From<V1CustomResourceDefinition>());
        return definitionConfig is { PermissionsLoaded: true, CanListAndWatch: true };
    }

    private static IResourceConfig[] GetCurrentResources(ClusterWorkspace cluster)
    {
        var resources = new List<IResourceConfig>();
        var customResourceKinds = new HashSet<(string Group, string Kind)>();
        foreach (var definition in cluster.GetCustomResourceDefinitions())
        {
            resources.Add(definition);
            customResourceKinds.Add((definition.Kind.Group, definition.Kind.Kind));
        }

        foreach (var resource in cluster.GetResourceConfigs())
        {
            if (resource.IsCustomResource())
            {
                if (customResourceKinds.Add((resource.Kind.Group, resource.Kind.Kind)))
                {
                    resources.Add(resource);
                }
            }
            else
            {
                resources.Add(resource);
            }
        }

        return resources.ToArray();
    }
}
