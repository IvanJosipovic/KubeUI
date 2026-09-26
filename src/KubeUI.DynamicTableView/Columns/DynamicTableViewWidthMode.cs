namespace KubeUI.DynamicTableView;

/// <summary>Column width sizing behavior.</summary>
public enum DynamicTableViewWidthMode
{
    /// <summary>Fit header and realized cells, growing as longer values appear.</summary>
    Auto,
    /// <summary>Fit header content only.</summary>
    Header,
    /// <summary>Fit realized cell content only.</summary>
    Cells,
    /// <summary>Use fixed pixel width.</summary>
    Pixel,
    /// <summary>Share remaining width proportionally.</summary>
    Star
}
