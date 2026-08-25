using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Kubernetes.Resources.Relationships;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

/// <summary>Resolves graph seed prerequisites and pending references into resource configurations.</summary>
internal static class VisualizationSeedPlanner
{
    public static HashSet<GroupApiVersionKind> FindRequiredSeedKinds(
        ResourceRelationshipGraph graph,
        IReadOnlySet<UnresolvedResourceReference> pendingReferences,
        ClusterWorkspace? cluster)
    {
        HashSet<GroupApiVersionKind> requiredSeedKinds = [];
        if (cluster == null)
        {
            return requiredSeedKinds;
        }

        foreach (var prerequisite in graph.RequiredSeedPrerequisites)
        {
            var kind = prerequisite.Kind;
            var matchingConfigs = cluster.GetResourceConfigs()
                .Where(resourceConfig => resourceConfig.Kind == kind
                    || prerequisite.MatchAnyApiGroup && string.Equals(resourceConfig.Kind.Kind, kind.Kind, StringComparison.Ordinal)
                    || prerequisite.AllowServedVersionFallback && MatchesSeedKind(kind, resourceConfig.Kind))
                .ToArray();

            if (matchingConfigs.Length == 0 || !prerequisite.AllowServedVersionFallback)
            {
                foreach (var resourceConfig in matchingConfigs)
                {
                    requiredSeedKinds.Add(resourceConfig.Kind);
                }
            }
            else
            {
                var selectedConfig = matchingConfigs
                    .OrderByDescending(resourceConfig => resourceConfig.Kind.ApiVersion, ApiVersionComparer.Instance)
                    .First();
                requiredSeedKinds.Add(selectedConfig.Kind);
            }
        }

        foreach (var reference in pendingReferences)
        {
            foreach (var resourceConfig in cluster.GetResourceConfigs())
            {
                if (string.Equals(resourceConfig.Kind.Group, reference.ApiGroup, StringComparison.Ordinal)
                    && string.Equals(resourceConfig.Kind.Kind, reference.Kind, StringComparison.Ordinal)
                    && (reference.ApiVersion == null || string.Equals(resourceConfig.Kind.ApiVersion, reference.ApiVersion, StringComparison.Ordinal)))
                {
                    requiredSeedKinds.Add(resourceConfig.Kind);
                }
            }
        }

        return requiredSeedKinds;
    }

    public static bool MatchesSeedKind(GroupApiVersionKind prerequisite, GroupApiVersionKind resourceKind)
        => prerequisite == resourceKind
            || string.Equals(prerequisite.Group, resourceKind.Group, StringComparison.Ordinal)
                && string.Equals(prerequisite.Kind, resourceKind.Kind, StringComparison.Ordinal);

    private sealed class ApiVersionComparer : IComparer<string>
    {
        public static ApiVersionComparer Instance { get; } = new();

        public int Compare(string? x, string? y)
        {
            var xVersion = Parse(x);
            var yVersion = Parse(y);
            var comparison = xVersion.Major.CompareTo(yVersion.Major);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = xVersion.Stage.CompareTo(yVersion.Stage);
            return comparison != 0 ? comparison : xVersion.Minor.CompareTo(yVersion.Minor);
        }

        private static (int Major, int Stage, int Minor) Parse(string? apiVersion)
        {
            if (string.IsNullOrWhiteSpace(apiVersion))
            {
                return (0, 0, 0);
            }

            var version = apiVersion.AsSpan();
            var majorEnd = 1;
            while (majorEnd < version.Length && char.IsDigit(version[majorEnd]))
            {
                majorEnd++;
            }

            _ = int.TryParse(version[1..majorEnd], out var major);
            var stage = version[majorEnd..].StartsWith("beta", StringComparison.Ordinal) ? 1
                : version[majorEnd..].StartsWith("alpha", StringComparison.Ordinal) ? 0
                : 2;
            var minorStart = stage == 2 ? majorEnd : majorEnd + (stage == 1 ? 4 : 5);
            _ = int.TryParse(version[minorStart..], out var minor);
            return (major, stage, minor);
        }
    }
}
