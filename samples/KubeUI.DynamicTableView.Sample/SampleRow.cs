namespace KubeUI.DynamicTableView.Sample;

public sealed record SampleRow(
    string Id,
    string Name,
    int Restarts,
    bool Ready,
    decimal CpuCores,
    float Memory,
    DateTimeOffset CreatedAt);
