namespace KubeUI.DynamicTableView;

/// <summary>Typed column definition using trim-safe delegates.</summary>
public sealed class DynamicTableViewColumn<T> : DynamicTableViewColumn
{
    private readonly Func<T, object?> _valueSelector;
    private readonly Func<T, string?> _displaySelector;

    /// <summary>Creates a text or templated column.</summary>
    /// <param name="key">Stable column key.</param>
    /// <param name="header">Header content.</param>
    /// <param name="valueType">Type of the value returned by <paramref name="valueSelector"/>.</param>
    /// <param name="valueSelector">Gets the row value used by table operations.</param>
    /// <param name="displaySelector">Optionally formats the value for the fallback text cell.</param>
    /// <param name="cellTemplate">Optionally supplies custom cell content.</param>
    public DynamicTableViewColumn(
        string key,
        object header,
        Type valueType,
        Func<T, object?> valueSelector,
        Func<T, string?>? displaySelector = null,
        IDataTemplate? cellTemplate = null)
        : base(key, header, valueType, cellTemplate)
    {
        _valueSelector = valueSelector ?? throw new ArgumentNullException(nameof(valueSelector));
        _displaySelector = displaySelector ?? (item => _valueSelector(item)?.ToString());
    }

    /// <summary>Creates a column using a typed value selector.</summary>
    /// <param name="key">Stable column key.</param>
    /// <param name="header">Header content.</param>
    /// <param name="valueSelector">Gets the row value used by table operations.</param>
    /// <param name="displaySelector">Optionally formats the value for the fallback text cell.</param>
    /// <param name="cellTemplate">Optionally supplies custom cell content.</param>
    /// <typeparam name="TValue">Selected value type.</typeparam>
    public static DynamicTableViewColumn<T> Create<TValue>(
        string key,
        object header,
        Func<T, TValue> valueSelector,
        Func<T, string?>? displaySelector = null,
        IDataTemplate? cellTemplate = null)
    {
        ArgumentNullException.ThrowIfNull(valueSelector);
        return new DynamicTableViewColumn<T>(key, header, typeof(TValue), item => valueSelector(item), displaySelector, cellTemplate);
    }

    /// <summary>Gets or sets a custom filter flyout factory.</summary>
    public Func<DynamicTableViewFilterContext, Control>? FilterFlyoutFactory { get; set; }

    /// <summary>Gets or sets choices for a standard enum or Boolean filter.</summary>
    public IReadOnlyList<DynamicTableViewFilterChoice> FilterChoices { get; set; } = [];

    /// <summary>Creates an enum column and its trim-safe choices.</summary>
    public static DynamicTableViewColumn<T> CreateEnum<TEnum>(
        string key,
        object header,
        Func<T, TEnum> valueSelector,
        Func<T, string?>? displaySelector = null)
        where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(valueSelector);
        var column = Create(key, header, valueSelector, displaySelector);
        column.FilterChoices = Enum.GetValues<TEnum>()
            .Select(static value => new DynamicTableViewFilterChoice(value.ToString(), value))
            .ToArray();
        return column;
    }

    internal override object? GetValue(object item)
        => item is T typedItem ? _valueSelector(typedItem) : null;

    internal override string GetDisplayValue(object item)
        => item is T typedItem ? _displaySelector(typedItem) ?? string.Empty : string.Empty;

    internal override Func<DynamicTableViewFilterContext, Control>? GetFilterFlyoutFactory()
        => FilterFlyoutFactory;

    internal override IReadOnlyList<DynamicTableViewFilterChoice> GetFilterChoices()
        => FilterChoices;
}
