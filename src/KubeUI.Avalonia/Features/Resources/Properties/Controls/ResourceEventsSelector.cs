using k8s;
using k8s.Models;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

internal static class ResourceEventsSelector
{
    public static ResourceEventItem ToItem(Corev1Event @event, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var timestamp = EventTimeFormatter.ResolveTimestamp(@event);

        return new ResourceEventItem(
            string.IsNullOrWhiteSpace(@event.Message) ? (@event.Reason ?? string.Empty) : @event.Message.Trim(),
            FormatSource(@event),
            @event.Count ?? 0,
            @event.InvolvedObject?.FieldPath ?? string.Empty,
            EventTimeFormatter.FormatPrettyLastSeen(timestamp, utcNow),
            string.Equals(@event.Type, "Warning", StringComparison.Ordinal));
    }

    public static DateTime GetSortTimestamp(Corev1Event @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var timestamp = EventTimeFormatter.ResolveTimestamp(@event);
        return timestamp.HasValue ? NormalizeUtc(timestamp.Value) : DateTime.MinValue;
    }

    public static ResourceEventItem[] SelectRecentEvents(
        IEnumerable<Corev1Event> events,
        IKubernetesObject<V1ObjectMeta> resource,
        DateTime utcNow,
        int limit = 5)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(resource);

        List<(Corev1Event Event, DateTime Timestamp)> matches = [];

        foreach (Corev1Event @event in events)
        {
            if (!MatchesResource(@event, resource))
            {
                continue;
            }

            matches.Add((@event, GetSortTimestamp(@event)));
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
        var results = new ResourceEventItem[take];

        for (var i = 0; i < take; i++)
        {
            results[i] = ToItem(matches[i].Event, utcNow);
        }

        return results;
    }

    public static bool MatchesResource(Corev1Event @event, IKubernetesObject<V1ObjectMeta> resource)
    {
        if (@event.InvolvedObject == null || resource.Metadata == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(resource.Metadata.Uid) &&
            string.Equals(@event.InvolvedObject.Uid, resource.Metadata.Uid, StringComparison.Ordinal))
        {
            return true;
        }

        if (!string.Equals(@event.InvolvedObject.Name, resource.Metadata.Name, StringComparison.Ordinal))
        {
            return false;
        }

        if (!string.Equals(@event.InvolvedObject.NamespaceProperty, resource.Metadata.NamespaceProperty, StringComparison.Ordinal))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(@event.InvolvedObject.Kind) &&
            !string.Equals(@event.InvolvedObject.Kind, resource.Kind, StringComparison.Ordinal))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(@event.InvolvedObject.ApiVersion) &&
            !string.Equals(@event.InvolvedObject.ApiVersion, resource.ApiVersion, StringComparison.Ordinal))
        {
            return false;
        }

        return true;
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
