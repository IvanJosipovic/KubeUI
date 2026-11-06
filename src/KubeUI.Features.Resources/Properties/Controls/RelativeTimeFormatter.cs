using k8s.Models;

namespace KubeUI.Features.Resources.Properties.Controls;

/// <summary>Formats Kubernetes resource timestamps as compact relative ages.</summary>
public static class RelativeTimeFormatter
{
    /// <summary>Formats a UTC timestamp as a compact age such as <c>2h</c> or <c>4m12s</c>.</summary>
    /// <param name="timestampUtc">Timestamp to format.</param>
    /// <param name="utcNow">Current UTC time used for the calculation.</param>
    /// <returns>The compact relative age.</returns>
    public static string FormatPrettyAge(DateTime timestampUtc, DateTime utcNow)
    {
        var timestamp = NormalizeUtc(timestampUtc);
        var delta = utcNow - timestamp;

        if (delta.TotalMilliseconds <= 0)
        {
            return "0ms";
        }

        if (delta.TotalDays >= 365)
        {
            return $"{delta.TotalDays / 365:N0}y";
        }

        if (delta.TotalDays >= 1)
        {
            return $"{delta.TotalDays:N0}d";
        }

        if (delta.TotalHours >= 1)
        {
            return $"{delta.TotalHours:N0}h";
        }

        if (delta.TotalMinutes >= 1)
        {
            return $"{delta.TotalMinutes:N0}m{delta.Seconds:N0}s";
        }

        if (delta.TotalSeconds >= 1)
        {
            return $"{delta.TotalSeconds:N0}s";
        }

        return $"{delta.TotalMilliseconds:N0}ms";
    }

    internal static string FormatPrettyLastSeen(DateTime? timestamp, DateTime utcNow)
    {
        if (!timestamp.HasValue)
        {
            return string.Empty;
        }

        var normalized = NormalizeUtc(timestamp.Value);
        var local = normalized.ToLocalTime();
        var localText = local.Date == utcNow.ToLocalTime().Date
            ? local.ToString("t")
            : local.ToString("g");

        return $"{FormatPrettyAge(normalized, utcNow)} ago ({localText})";
    }

    internal static DateTime? ResolveTimestamp(Corev1Event ev)
    {
        return ev.LastTimestamp ?? ev.EventTime ?? ev.Metadata?.CreationTimestamp;
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
