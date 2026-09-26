namespace KubeUI.DynamicTableView;

/// <summary>Controls how a DynamicData source publishes updates and restores selection.</summary>
public sealed class DynamicTableViewSourceOptions
{
    /// <summary>
    /// Gets or sets whether an update to an existing DynamicData key raises a collection Replace notification.
    /// When false, the view publishes Remove and Add notifications. The source still supplies the updated row object.
    /// Defaults to true.
    /// </summary>
    public bool UseReplaceForUpdates { get; init; } = true;

    /// <summary>
    /// Gets or sets how selected rows match after source changes. Key matches new row instances with the same
    /// DynamicData key. Reference matches only the same object instance and is intended for reference type rows.
    /// Defaults to Key.
    /// </summary>
    public DynamicTableViewSelectionIdentityMode SelectionIdentityMode { get; init; }
}
