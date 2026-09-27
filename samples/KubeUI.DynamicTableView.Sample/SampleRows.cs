namespace KubeUI.DynamicTableView.Sample;

internal static class SampleRows
{
    public static List<SampleRow> Create()
    {
        string[] workloads = ["api", "worker", "gateway", "metrics", "scheduler", "web"];
        var createdAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var rows = new List<SampleRow>(1000);

        for (var index = 0; index < 1000; index++)
        {
            var rowNumber = index + 1;
            rows.Add(new SampleRow(
                $"pod-{rowNumber:D4}",
                $"{workloads[index % workloads.Length]}-{rowNumber:D4}",
                index % 8,
                index % 7 != 0,
                0.05m + index % 95 / 100m,
                0.25f + index % 32 * 0.25f,
                createdAt.AddMinutes(index * 17)));
        }

        return rows;
    }
}
