namespace KubeUI.DynamicTableView;

/// <summary>Controls how a DynamicData source restores selection.</summary>
public sealed class DynamicTableViewSourceOptions
{
    /// <summary>
    /// Gets or sets how selected rows match after source changes. Key matches new row instances with the same
    /// DynamicData key. Reference matches only the same object instance and is intended for reference type rows.
    /// Defaults to Key.
    /// </summary>
    public DynamicTableViewSelectionIdentityMode SelectionIdentityMode { get; init; }
}
