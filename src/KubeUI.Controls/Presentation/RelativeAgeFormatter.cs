namespace KubeUI.Controls.Presentation;

/// <summary>Formats UTC timestamps as compact relative ages for shared UI presentation.</summary>
public static class RelativeAgeFormatter
{
    /// <summary>Formats a timestamp as a compact age such as <c>2h</c> or <c>4m12s</c>.</summary>
    /// <param name="timestampUtc">Timestamp to format; unspecified/local values are treated as UTC.</param>
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

    /// <summary>Formats a last-seen timestamp as relative age plus local clock/date text.</summary>
    /// <param name="timestampUtc">Timestamp to display, or null when no timestamp is available.</param>
    /// <param name="utcNow">Current UTC time used for the calculation.</param>
    /// <returns>Empty text for null, otherwise an age and local timestamp.</returns>
    public static string FormatPrettyLastSeen(DateTime? timestampUtc, DateTime utcNow)
    {
        if (!timestampUtc.HasValue)
        {
            return string.Empty;
        }

        var normalized = NormalizeUtc(timestampUtc.Value);
        var local = normalized.ToLocalTime();
        var localText = local.Date == utcNow.ToLocalTime().Date
            ? local.ToString("t")
            : local.ToString("g");

        return $"{FormatPrettyAge(normalized, utcNow)} ago ({localText})";
    }

    private static DateTime NormalizeUtc(DateTime value)
        => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
