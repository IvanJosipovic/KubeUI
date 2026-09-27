namespace KubeUI.DynamicTableView.Sample;

public static class SampleColumnSelectors
{
    public static Func<SampleRow, object?> Name { get; } = static row => row.Name;

    public static Func<SampleRow, object?> Restarts { get; } = static row => row.Restarts;

    public static Func<SampleRow, object?> Ready { get; } = static row => row.Ready;

    public static Func<SampleRow, object?> CpuCores { get; } = static row => row.CpuCores;

    public static Func<SampleRow, object?> Memory { get; } = static row => row.Memory;

    public static Func<SampleRow, object?> CreatedAt { get; } = static row => row.CreatedAt;
}
