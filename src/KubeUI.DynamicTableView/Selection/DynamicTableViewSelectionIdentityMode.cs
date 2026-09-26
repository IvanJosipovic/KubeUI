namespace KubeUI.DynamicTableView;

/// <summary>Defines how the table matches a selected row after DynamicData changes.</summary>
public enum DynamicTableViewSelectionIdentityMode
{
    /// <summary>Matches new row instances by their DynamicData key.</summary>
    Key,

    /// <summary>Matches only the same object instance. Use with reference type rows.</summary>
    Reference
}
