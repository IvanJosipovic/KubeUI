using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

/// <summary>Immutable, complete snapshot of resources served by a Kubernetes API server.</summary>
public sealed class ApiDiscoverySnapshot
{
    private static readonly Regex VersionPattern = new("^v(?<major>\\d+)(?<stage>alpha|beta)?(?<minor>\\d+)?$", RegexOptions.CultureInvariant);

    private ApiDiscoverySnapshot(IReadOnlyList<ApiDiscoveryResource> resources)
    {
        Resources = resources;
    }

    /// <summary>Gets all discovered root resource endpoints, including typed built-ins.</summary>
    public IReadOnlyList<ApiDiscoveryResource> Resources { get; }

    /// <summary>Gets one deterministic served version for each API group and kind.</summary>
    public IReadOnlyList<ApiDiscoveryResource> PreferredResources
    {
        get
        {
            var selected = Resources
                .GroupBy(static resource => (resource.Kind.Group, resource.Kind.Kind))
                .Select(static group => group
                    .OrderByDescending(static resource => string.Equals(resource.Freshness, "Current", StringComparison.Ordinal))
                    .ThenByDescending(static resource => GetVersionRank(resource.Kind.ApiVersion))
                    .ThenBy(static resource => resource.Kind.ApiVersion, StringComparer.Ordinal)
                    .First())
                .OrderBy(static resource => resource.Kind.Group, StringComparer.Ordinal)
                .ThenBy(static resource => resource.Kind.Kind, StringComparer.Ordinal)
                .ToArray();

            return Array.AsReadOnly(selected);
        }
    }

    /// <summary>Builds a complete snapshot from core and grouped discovery responses.</summary>
    /// <param name="core">Core `/api` discovery response.</param>
    /// <param name="groups">Grouped `/apis` discovery response.</param>
    public static ApiDiscoverySnapshot Create(
        V2beta1APIGroupDiscoveryList core,
        V2beta1APIGroupDiscoveryList groups)
    {
        ArgumentNullException.ThrowIfNull(core);
        ArgumentNullException.ThrowIfNull(groups);

        var resources = new List<ApiDiscoveryResource>();
        AddResources(core, resources, requireCoreGroup: true);
        AddResources(groups, resources, requireCoreGroup: false);

        var unique = resources
            .DistinctBy(static resource => resource.Kind)
            .OrderBy(static resource => resource.Kind.Group, StringComparer.Ordinal)
            .ThenBy(static resource => resource.Kind.ApiVersion, StringComparer.Ordinal)
            .ThenBy(static resource => resource.Kind.Kind, StringComparer.Ordinal)
            .ToArray();

        return new ApiDiscoverySnapshot(new ReadOnlyCollection<ApiDiscoveryResource>(unique));
    }

    private static void AddResources(
        V2beta1APIGroupDiscoveryList list,
        ICollection<ApiDiscoveryResource> resources,
        bool requireCoreGroup)
    {
        foreach (var group in list.items)
        {
            var groupName = group.metadata?.name ?? string.Empty;
            if (requireCoreGroup != string.IsNullOrEmpty(groupName))
            {
                continue;
            }

            foreach (var version in group.versions)
            {
                if (string.IsNullOrWhiteSpace(version.version))
                {
                    continue;
                }

                foreach (var resource in version.resources)
                {
                    if (string.IsNullOrWhiteSpace(resource.resource)
                        || resource.resource.Contains('/', StringComparison.Ordinal)
                        || resource.scope is not ("Namespaced" or "Cluster"))
                    {
                        continue;
                    }

                    var kind = resource.responseKind?.kind;
                    if (string.IsNullOrWhiteSpace(kind))
                    {
                        continue;
                    }

                    var responseKindIsEmpty = string.IsNullOrEmpty(resource.responseKind.group)
                        && string.IsNullOrEmpty(resource.responseKind.version);
                    var resourceGroup = responseKindIsEmpty ? groupName : resource.responseKind.group ?? groupName;
                    var resourceVersion = responseKindIsEmpty ? version.version : resource.responseKind.version ?? version.version;
                    if (!string.Equals(resourceGroup, groupName, StringComparison.Ordinal)
                        || !string.Equals(resourceVersion, version.version, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    resources.Add(new ApiDiscoveryResource(
                        new GroupApiVersionKind(resourceGroup, resourceVersion, kind, resource.resource),
                        string.Equals(resource.scope, "Namespaced", StringComparison.Ordinal),
                        version.freshness,
                        resource.verbs));
                }
            }
        }
    }

    private static (int StageRank, int Major, int Minor) GetVersionRank(string version)
    {
        var match = VersionPattern.Match(version);
        if (!match.Success || !int.TryParse(match.Groups["major"].Value, out var major))
        {
            return (0, 0, 0);
        }

        var stageRank = match.Groups["stage"].Value switch
        {
            "" => 3,
            "beta" => 2,
            "alpha" => 1,
            _ => 0,
        };
        var minor = int.TryParse(match.Groups["minor"].Value, out var parsedMinor) ? parsedMinor : 0;
        return (stageRank, major, minor);
    }
}
