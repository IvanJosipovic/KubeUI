using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace KubeUI.DynamicTableView;

internal sealed partial class DynamicTableViewHeader : TemplatedControl
{
    private readonly DynamicTableView _owner;
    private Button? _filterButton;
    private Point _sortPointerDownPosition;
    private bool _sortPointerDown;

    public DynamicTableViewHeader(DynamicTableView owner, DynamicTableViewColumn column)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        Column = column ?? throw new ArgumentNullException(nameof(column));
        NativeColumn = _owner.GetNativeColumn(column.Key);
        HeaderText = column.Header.ToString() ?? string.Empty;
        IsFilterEnabled = column.CanUserFilter;
        IsSortEnabled = column.CanUserSort;
        Classes.Add("dynamic-table-view-header");
        _owner.UpdateHeaderState(this);
        AddHandler(InputElement.PointerPressedEvent, HeaderPointerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(InputElement.PointerMovedEvent, HeaderPointerMoved, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(InputElement.PointerReleasedEvent, HeaderPointerReleased, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    public DynamicTableViewColumn Column { get; }

    public TableViewColumn? NativeColumn { get; }

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
    public partial bool IsSortAscending { get; set; }

    [GeneratedDirectProperty]
    public partial bool IsSortDescending { get; set; }

    [GeneratedDirectProperty]
    public partial bool IsFiltered { get; set; }

    protected override Type StyleKeyOverride => typeof(DynamicTableViewHeader);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_filterButton is not null)
            _filterButton.Click -= FilterButtonOnClick;
        base.OnApplyTemplate(e);
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
        SortText = descriptor is null || sorts.Count <= 1
            ? string.Empty
            : (sortIndex + 1).ToString(CultureInfo.InvariantCulture);
        IsSortAscending = descriptor?.Direction == ListSortDirection.Ascending;
        IsSortDescending = descriptor?.Direction == ListSortDirection.Descending;
        IsFiltered = filters.Any(filter => string.Equals(filter.ColumnKey, Column.Key, StringComparison.Ordinal));
        Classes.Set("filtered", IsFiltered);
    }

    private void FilterButtonOnClick(object? sender, RoutedEventArgs e)
        => FlyoutBase.ShowAttachedFlyout(_filterButton!);

    private void HeaderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _sortPointerDown = IsSortEnabled &&
            e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed &&
            !IsWithinNamedControl(e.Source, "PART_FilterButton");
        if (_sortPointerDown)
            _sortPointerDownPosition = e.GetPosition(this);
    }

    private void HeaderPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_sortPointerDown || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        var position = e.GetPosition(this);
        var deltaX = position.X - _sortPointerDownPosition.X;
        var deltaY = position.Y - _sortPointerDownPosition.Y;
        if (deltaX * deltaX + deltaY * deltaY >= 25)
            _sortPointerDown = false;
    }

    private void HeaderPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var position = e.GetPosition(this);
        var shouldSort = _sortPointerDown && position.X >= 0 && position.X <= Bounds.Width &&
            position.Y >= 0 && position.Y <= Bounds.Height;
        _sortPointerDown = false;
        if (shouldSort)
            _owner.SortColumn(Column.Key);
    }

    private static bool IsWithinNamedControl(object? source, string name)
        => source is Visual visual && visual.GetSelfAndVisualAncestors().OfType<Control>().Any(control => control.Name == name);
}
