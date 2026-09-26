using Avalonia.Interactivity;

namespace KubeUI.DynamicTableView;

internal sealed partial class DynamicTableViewHeader : TemplatedControl
{
    private readonly DynamicTableView _owner;
    private Button? _filterButton;

    public DynamicTableViewHeader(DynamicTableView owner, DynamicTableViewColumn column)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        Column = column ?? throw new ArgumentNullException(nameof(column));
        HeaderText = column.Header.ToString() ?? string.Empty;
        IsFilterEnabled = column.CanUserFilter;
        IsSortEnabled = column.CanUserSort;
        Classes.Add("dynamic-table-view-header");
    }

    public DynamicTableViewColumn Column { get; }

    public TableViewColumn NativeColumn { get; set; } = null!;

    [GeneratedDirectProperty]
    public partial string HeaderText { get; set; } = string.Empty;

    [GeneratedDirectProperty]
    public partial string SortText { get; set; } = string.Empty;

    [GeneratedDirectProperty]
    public partial bool IsFilterEnabled { get; set; }

    [GeneratedDirectProperty]
    public partial bool IsSortEnabled { get; set; }

    [GeneratedDirectProperty]
    public partial bool HasSort { get; set; }

    [GeneratedDirectProperty]
    public partial bool IsFiltered { get; set; }

    [GeneratedDirectProperty]
    public partial string FilterText { get; set; } = string.Empty;

    protected override Type StyleKeyOverride => typeof(DynamicTableViewHeader);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_filterButton is not null)
            _filterButton.Click -= FilterButtonOnClick;

        base.OnApplyTemplate(e);
        var sortButton = e.NameScope.Find<Button>("PART_SortButton");
        sortButton?.AddHandler(Button.ClickEvent, SortButtonOnClick);
        _filterButton = e.NameScope.Find<Button>("PART_FilterButton");
        if (_filterButton is not null && IsFilterEnabled)
        {
            FlyoutBase.SetAttachedFlyout(_filterButton, _owner.MakeFilterFlyout(Column));
            _filterButton.Click += FilterButtonOnClick;
        }
    }

    public void UpdateState(IReadOnlyList<DynamicTableViewSortDescriptor> sorts, IReadOnlyList<DynamicTableViewFilterDescriptor> filters)
    {
        var sortIndex = -1;
        DynamicTableViewSortDescriptor? descriptor = null;
        for (var i = 0; i < sorts.Count; i++)
        {
            if (!string.Equals(sorts[i].ColumnKey, Column.Key, StringComparison.Ordinal))
                continue;
            sortIndex = i;
            descriptor = sorts[i];
            break;
        }

        HasSort = descriptor is not null;
        SortText = descriptor is null
            ? string.Empty
            : (descriptor.Direction == ListSortDirection.Ascending ? "▲" : "▼") + (sorts.Count > 1 ? (sortIndex + 1).ToString(CultureInfo.InvariantCulture) : string.Empty);
        IsFiltered = filters.Any(filter => string.Equals(filter.ColumnKey, Column.Key, StringComparison.Ordinal));
        FilterText = IsFiltered ? "▾" : "▽";
        Classes.Set("filtered", IsFiltered);
    }

    private void SortButtonOnClick(object? sender, RoutedEventArgs e)
        => _owner.SortColumn(Column.Key);

    private void FilterButtonOnClick(object? sender, RoutedEventArgs e)
        => FlyoutBase.ShowAttachedFlyout(_filterButton!);
}
