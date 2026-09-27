namespace KubeUI.DynamicTableView.Sample;

public sealed class SampleColumnDefinition
{
    public string Key { get; set; } = string.Empty;

    public object Header { get; set; } = string.Empty;

    public Type ValueType { get; set; } = typeof(string);

    public Func<SampleRow, object?>? ValueSelector { get; set; }

    public DynamicTableViewWidthMode WidthMode { get; set; } = DynamicTableViewWidthMode.Auto;

    public double Width { get; set; } = 1;

    public DynamicTableViewColumn<SampleRow> CreateColumn()
    {
        var valueSelector = ValueSelector
            ?? throw new InvalidOperationException($"Column '{Key}' has no value selector.");
        return new DynamicTableViewColumn<SampleRow>(Key, Header, ValueType, valueSelector)
        {
            WidthMode = WidthMode,
            Width = Width
        };
    }
}
