namespace KubeUI.DynamicTableView;

/// <summary>Saved display settings for one column.</summary>
public sealed record DynamicTableViewColumnState(string Key, int Order, double Width);
