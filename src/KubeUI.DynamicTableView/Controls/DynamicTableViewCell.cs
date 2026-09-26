namespace KubeUI.DynamicTableView;

internal sealed partial class DynamicTableViewCell : TemplatedControl
{
    [GeneratedDirectProperty]
    public partial string Text { get; set; } = string.Empty;

    private readonly DynamicTableViewColumn _column;

    public DynamicTableViewCell(DynamicTableViewColumn column)
    {
        _column = column ?? throw new ArgumentNullException(nameof(column));
        Classes.Add("dynamic-table-view-cell");
        DataContextChanged += (_, _) => UpdateText();
    }

    protected override Type StyleKeyOverride => typeof(DynamicTableViewCell);

    private void UpdateText()
        => Text = DataContext is { } item ? _column.GetDisplayValue(item) : string.Empty;
}
