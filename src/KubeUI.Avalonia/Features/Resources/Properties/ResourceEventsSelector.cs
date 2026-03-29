using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Controls;

internal static class ResourceEventsSelector
{
    public static ResourceEventItemViewModel[] SelectRecentEvents(
        ClusterWorkspaceViewModel cluster,
        IKubernetesObject<V1ObjectMeta> resource,
        DateTime utcNow,
        int limit = 5)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(resource);

        return SelectRecentEvents(cluster.GetResourceSourceCache<Corev1Event>().Items, resource, utcNow, limit);
    }

    public static ResourceEventItemViewModel[] SelectRecentEvents(
        IEnumerable<Corev1Event> events,
        IKubernetesObject<V1ObjectMeta> resource,
        DateTime utcNow,
        int limit = 5)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(resource);

        if (resource.Metadata?.Uid is not string resourceUid || string.IsNullOrWhiteSpace(resourceUid))
        {
            return [];
        }

        List<(Corev1Event Event, DateTime Timestamp)> matches = [];

        foreach (Corev1Event @event in events)
        {
            if (!string.Equals(@event.InvolvedObject?.Uid, resourceUid, StringComparison.Ordinal))
            {
                continue;
            }

            var timestamp = EventTimeFormatter.ResolveTimestamp(@event);
            matches.Add((@event, timestamp.HasValue ? NormalizeUtc(timestamp.Value) : DateTime.MinValue));
        }

        if (matches.Count == 0)
        {
            return [];
        }

        matches.Sort(static (left, right) =>
        {
            var compare = right.Timestamp.CompareTo(left.Timestamp);
            return compare != 0 ? compare : string.CompareOrdinal(right.Event.Name(), left.Event.Name());
        });

        var take = Math.Min(limit, matches.Count);
        var results = new ResourceEventItemViewModel[take];

        for (var i = 0; i < take; i++)
        {
            var @event = matches[i].Event;
            var timestamp = EventTimeFormatter.ResolveTimestamp(@event);

            results[i] = new ResourceEventItemViewModel(
                string.IsNullOrWhiteSpace(@event.Message) ? (@event.Reason ?? string.Empty) : @event.Message.Trim(),
                FormatSource(@event),
                @event.Count ?? 0,
                @event.InvolvedObject?.FieldPath ?? string.Empty,
                EventTimeFormatter.FormatPrettyLastSeen(timestamp, utcNow),
                string.Equals(@event.Type, "Warning", StringComparison.Ordinal));
        }

        return results;
    }

    private static string FormatSource(Corev1Event @event)
    {
        string? component = @event.Source?.Component ?? @event.ReportingComponent;
        string? host = @event.Source?.Host ?? @event.ReportingInstance;

        if (string.IsNullOrWhiteSpace(component))
        {
            return host ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(host))
        {
            return component;
        }

        return $"{component} {host}";
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
