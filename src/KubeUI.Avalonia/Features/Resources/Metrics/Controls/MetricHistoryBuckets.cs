using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Metrics.Controls;

internal sealed record MetricHistoryData(IReadOnlyList<MetricPoint> Cpu, IReadOnlyList<MetricPoint> Memory)
{
    public static MetricHistoryData Empty { get; } = new([], []);
}

internal enum MetricsLimitState
{
    Normal,
    Warning,
    Exceeded,
}

internal sealed record MetricsBarData(
    DateTimeOffset Start,
    DateTimeOffset End,
    double Value,
    double Height,
    double? Limit,
    MetricsLimitState LimitState);

internal static class MetricsHistoryBuckets
{
    public const int BucketCount = 12;
    public static readonly TimeSpan BucketSize = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan History = TimeSpan.FromHours(1);
    public const double ChartHeight = 20;

    public static IReadOnlyList<MetricsBarData> CreateBars(
        IReadOnlyList<MetricPoint> points,
        DateTimeOffset end,
        double? limit)
    {
        var start = end - History;
        var values = new double[BucketCount];
        var hasValues = new bool[BucketCount];

        foreach (var point in points)
        {
            if (point.Timestamp < start || point.Timestamp >= end)
            {
                continue;
            }

            var bucket = (int)((point.Timestamp - start).Ticks / BucketSize.Ticks);
            if (bucket is < 0 or >= BucketCount)
            {
                continue;
            }

            if (!hasValues[bucket] || point.Value > values[bucket])
            {
                values[bucket] = point.Value;
                hasValues[bucket] = true;
            }
        }

        var maximum = Math.Max(limit.GetValueOrDefault(), values.Max());
        if (maximum <= 0)
        {
            maximum = 1;
        }

        var bars = new MetricsBarData[BucketCount];
        for (var index = 0; index < BucketCount; index++)
        {
            var value = hasValues[index] ? values[index] : 0;
            var utilization = limit is > 0 ? value / limit.Value : 0;
            var state = limit is not > 0 || !hasValues[index]
                ? MetricsLimitState.Normal
                : utilization >= 1
                    ? MetricsLimitState.Exceeded
                    : utilization >= 0.8
                        ? MetricsLimitState.Warning
                        : MetricsLimitState.Normal;
            var height = hasValues[index] ? Math.Max(1, value / maximum * ChartHeight) : 0;
            var bucketStart = start + TimeSpan.FromTicks(BucketSize.Ticks * index);
            bars[index] = new MetricsBarData(
                bucketStart,
                bucketStart + BucketSize,
                value,
                height,
                limit is > 0 ? limit : null,
                state);
        }

        return bars;
    }
}
