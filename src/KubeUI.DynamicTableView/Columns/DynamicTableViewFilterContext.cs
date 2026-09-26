namespace KubeUI.DynamicTableView;

/// <summary>Context passed to a custom column filter flyout.</summary>
public sealed class DynamicTableViewFilterContext
{
    /// <summary>Creates a filter context.</summary>
    public DynamicTableViewFilterContext(DynamicTableViewColumn column, IDynamicTableViewSource source)
    {
        Column = column ?? throw new ArgumentNullException(nameof(column));
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>Gets the column being filtered.</summary>
    public DynamicTableViewColumn Column { get; }

    /// <summary>Gets the owning source.</summary>
    public IDynamicTableViewSource Source { get; }
}
