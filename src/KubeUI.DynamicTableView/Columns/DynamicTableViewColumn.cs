namespace KubeUI.DynamicTableView;

/// <summary>Base metadata for one dynamic table column.</summary>
public abstract class DynamicTableViewColumn
{
    /// <summary>Creates column metadata.</summary>
    protected DynamicTableViewColumn(string key, object header, Type valueType, IDataTemplate? cellTemplate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(header);
        ArgumentNullException.ThrowIfNull(valueType);
        Key = key;
        Header = header;
        ValueType = Nullable.GetUnderlyingType(valueType) ?? valueType;
        CellTemplate = cellTemplate;
    }

    /// <summary>Gets the stable column key.</summary>
    public string Key { get; }

    /// <summary>Gets or sets header content.</summary>
    public object Header { get; set; }

    /// <summary>Gets the field value type.</summary>
    public Type ValueType { get; }

    /// <summary>Gets or sets the cell template.</summary>
    public IDataTemplate? CellTemplate { get; set; }

    /// <summary>Gets or sets width behavior.</summary>
    public DynamicTableViewWidthMode WidthMode { get; set; } = DynamicTableViewWidthMode.Auto;

    /// <summary>Gets or sets fixed width or star weight.</summary>
    public double Width { get; set; } = 1;

    /// <summary>Gets or sets minimum width.</summary>
    public double MinWidth { get; set; } = 40;

    /// <summary>Gets or sets whether this column participates in search.</summary>
    public bool IsSearchable { get; set; } = true;

    /// <summary>Gets or sets whether users can sort this column.</summary>
    public bool CanUserSort { get; set; } = true;

    /// <summary>Gets or sets whether this column has a filter flyout.</summary>
    public bool CanUserFilter { get; set; } = true;

    internal abstract object? GetValue(object item);

    internal abstract string GetDisplayValue(object item);

    internal virtual Func<DynamicTableViewFilterContext, Control>? GetFilterFlyoutFactory() => null;

    internal virtual IReadOnlyList<DynamicTableViewFilterChoice> GetFilterChoices() => [];
}
