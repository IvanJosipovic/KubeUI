namespace KubeUI.DynamicTableView;

/// <summary>Rows targeted by a right-click request.</summary>
public sealed class DynamicTableViewContextMenuEventArgs : EventArgs
{
    /// <summary>Creates context-menu request args.</summary>
    public DynamicTableViewContextMenuEventArgs(object clickedItem, IReadOnlyList<object> targetItems)
    {
        ClickedItem = clickedItem ?? throw new ArgumentNullException(nameof(clickedItem));
        TargetItems = targetItems ?? throw new ArgumentNullException(nameof(targetItems));
    }

    /// <summary>Gets the row under the pointer.</summary>
    public object ClickedItem { get; }

    /// <summary>Gets the selected rows or the clicked row.</summary>
    public IReadOnlyList<object> TargetItems { get; }
}
