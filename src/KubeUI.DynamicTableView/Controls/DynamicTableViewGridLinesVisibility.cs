namespace KubeUI.DynamicTableView;

/// <summary>Specifies which separators DynamicTableView draws between rows and columns.</summary>
[Flags]
public enum DynamicTableViewGridLinesVisibility
{
    /// <summary>Draws no row or column separators.</summary>
    None = 0,

    /// <summary>Draws horizontal separators between rows.</summary>
    Horizontal = 1,

    /// <summary>Draws vertical separators between columns.</summary>
    Vertical = 2,

    /// <summary>Draws both horizontal and vertical separators.</summary>
    All = 3
}
