using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace KubeUI.DynamicTableView;

/// <summary>Read-only dynamic table view with DynamicData-backed query and row virtualization.</summary>
public sealed partial class DynamicTableView : TableView
{
    [GeneratedDirectProperty]
    public partial IDynamicTableViewSource? Source { get; set; }

    private readonly Dictionary<string, TableViewColumn> _nativeColumns = new(StringComparer.Ordinal);
    private readonly HashSet<string> _manuallySizedColumns = new(StringComparer.Ordinal);
    private IDynamicTableViewSource? _attachedSource;
    private bool _autoWidthPassQueued;
    private Vector? _pendingScrollOffset;
    private bool _applyingScrollRestore;
    private int _pointerDownColumn = -1;
    private Point _pointerDownPosition;
    private bool _headerDragStarted;

    /// <summary>Creates an empty dynamic table.</summary>
    public DynamicTableView()
    {
        Classes.Add("dynamic-table-view");
        SelectionMode = SelectionMode.Multiple;
        AddHandler(InputElement.PointerPressedEvent, OnGridPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(InputElement.PointerMovedEvent, OnGridPointerMoved, RoutingStrategies.Tunnel);
        AddHandler(InputElement.PointerReleasedEvent, OnGridPointerReleased, RoutingStrategies.Tunnel);
        AddHandler(ScrollViewer.ScrollChangedEvent, OnScrollChanged, RoutingStrategies.Bubble);
    }

    /// <summary>Gets or sets the command invoked when user double-clicks row or cell.</summary>
    public ICommand? ActivateCommand { get; set; }

    /// <summary>Gets or sets the command invoked by Delete key.</summary>
    public ICommand? DeleteCommand { get; set; }

    /// <summary>Gets or sets a provider for context menu items.</summary>
    public Func<IReadOnlyList<object>, IEnumerable?>? ContextMenuItemsFactory { get; set; }

    /// <summary>Raised when user right-clicks a realized row.</summary>
    public event EventHandler<DynamicTableViewContextMenuEventArgs>? ContextMenuRequested;

    protected override Type StyleKeyOverride => typeof(TableView);

    /// <summary>Captures keyed column order and widths, source sorts and filters, and scroll offset.</summary>
    public DynamicTableViewState CaptureState()
    {
        var source = Source ?? throw new InvalidOperationException("DynamicTableView has no source.");
        var columns = new List<DynamicTableViewColumnState>(source.Columns.Count);
        for (var i = 0; i < source.Columns.Count; i++)
        {
            var definition = source.Columns[i];
            var width = _nativeColumns.TryGetValue(definition.Key, out var native)
                ? native.ActualWidth
                : definition.Width;
            columns.Add(new(definition.Key, i, double.IsFinite(width) ? width : definition.MinWidth));
        }
        var scrollOffset = _pendingScrollOffset ?? FindScrollViewer()?.Offset ?? default;
        return new(columns, source.SortDescriptors.ToArray(), source.FilterDescriptors.ToArray(), scrollOffset);
    }

    /// <summary>Restores column order, pixel widths, sort descriptors, filters, and scroll offset by stable key.</summary>
    public void RestoreState(DynamicTableViewState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var source = Source ?? throw new InvalidOperationException("DynamicTableView has no source.");
        var ordered = state.Columns
            .Where(saved => source.Columns.Any(column => string.Equals(column.Key, saved.Key, StringComparison.Ordinal)))
            .OrderBy(static saved => saved.Order)
            .ToArray();

        for (var targetIndex = 0; targetIndex < ordered.Length; targetIndex++)
        {
            var currentIndex = IndexOfColumn(source.Columns, ordered[targetIndex].Key);
            if (currentIndex < 0 || currentIndex == targetIndex)
                continue;
            var column = source.Columns[currentIndex];
            source.Columns.RemoveAt(currentIndex);
            source.Columns.Insert(targetIndex, column);
        }

        foreach (var saved in state.Columns)
        {
            var definition = FindDefinition(saved.Key);
            if (definition is null)
                continue;
            definition.WidthMode = DynamicTableViewWidthMode.Pixel;
            definition.Width = Math.Max(definition.MinWidth, saved.Width);
            _manuallySizedColumns.Add(saved.Key);
        }

        source.SetSort(state.Sorts);
        source.ClearFilters();
        foreach (var filter in state.Filters)
            source.SetFilter(filter);
        _pendingScrollOffset = state.ScrollOffset;
        RebuildNativeColumns();
        QueueAutoWidthPass();
        QueueScrollRestore();
    }

    partial void OnPropertyChangedOverride(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == SourceProperty)
            AttachSource(change.GetNewValue<IDynamicTableViewSource?>());
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        LayoutUpdated += OnLayoutUpdated;
        QueueAutoWidthPass();
        QueueScrollRestore();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        LayoutUpdated -= OnLayoutUpdated;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnDoubleTapped(TappedEventArgs e)
    {
        base.OnDoubleTapped(e);
        if (ActivateCommand is null || !IsRowOrCellSource(e.Source))
            return;
        var parameter = Selection?.SelectedItems is { Count: > 0 } selected
            ? selected
            : FindRow(e.Source)?.DataContext;
        if (ActivateCommand.CanExecute(parameter))
            ActivateCommand.Execute(parameter);
    }

    private void AttachSource(IDynamicTableViewSource? source)
    {
        if (_attachedSource is not null)
        {
            _attachedSource.Changed -= SourceOnChanged;
            _attachedSource.Columns.CollectionChanged -= ColumnsOnCollectionChanged;
        }

        _attachedSource = source;
        if (source is null)
        {
            ItemsSource = null;
            Selection = null;
            Columns.Clear();
            _nativeColumns.Clear();
            return;
        }

        ItemsSource = source.Items;
        Selection = source.SelectionModel;
        source.Changed += SourceOnChanged;
        source.Columns.CollectionChanged += ColumnsOnCollectionChanged;
        RebuildNativeColumns();
    }

    private void SourceOnChanged(object? sender, EventArgs e)
    {
        UpdateHeaderStates();
        QueueAutoWidthPass();
    }

    private void ColumnsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildNativeColumns();
        QueueAutoWidthPass();
    }

    private void RebuildNativeColumns()
    {
        _nativeColumns.Clear();
        Columns.Clear();
        if (Source is null)
            return;

        foreach (var definition in Source.Columns)
        {
            var header = new DynamicTableViewHeader(this, definition);
            var native = new TableViewColumn
            {
                Header = header,
                Width = GetInitialWidth(definition),
                CellTemplate = definition.CellTemplate ?? new FuncDataTemplate<object>((_, _) =>
                    new DynamicTableViewCell(definition), supportsRecycling: true)
            };
            header.NativeColumn = native;
            _nativeColumns.Add(definition.Key, native);
            Columns.Add(native);
        }
        UpdateHeaderStates();
    }

    private static GridLength GetInitialWidth(DynamicTableViewColumn definition)
        => definition.WidthMode switch
        {
            DynamicTableViewWidthMode.Pixel => new GridLength(Math.Max(definition.MinWidth, definition.Width)),
            DynamicTableViewWidthMode.Star => new GridLength(Math.Max(0.1, definition.Width), GridUnitType.Star),
            _ => new GridLength(Math.Max(definition.MinWidth, 80))
        };

    private void UpdateHeaderStates()
    {
        if (Source is null)
            return;
        foreach (var definition in Source.Columns)
        {
            if (_nativeColumns.TryGetValue(definition.Key, out var native) && native.Header is DynamicTableViewHeader header)
                header.UpdateState(Source.SortDescriptors, Source.FilterDescriptors);
        }
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        TryRestorePendingScrollOffset();
        QueueAutoWidthPass();
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (!_applyingScrollRestore && e.OffsetDelta != default)
            _pendingScrollOffset = null;
    }

    private void QueueScrollRestore()
    {
        if (_pendingScrollOffset is null || VisualRoot is null)
            return;
        Dispatcher.UIThread.Post(TryRestorePendingScrollOffset, DispatcherPriority.Loaded);
    }

    private void TryRestorePendingScrollOffset()
    {
        if (_pendingScrollOffset is not { } requested || FindScrollViewer() is not { } scrollViewer)
            return;

        Vector maximum = new(
            Math.Max(0, scrollViewer.Extent.Width - scrollViewer.Viewport.Width),
            Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
        Vector restored = new(
            Math.Clamp(requested.X, 0, maximum.X),
            Math.Clamp(requested.Y, 0, maximum.Y));
        _applyingScrollRestore = true;
        try
        {
            scrollViewer.Offset = restored;
        }
        finally
        {
            _applyingScrollRestore = false;
        }

        if (restored == requested)
            _pendingScrollOffset = null;
    }

    private ScrollViewer? FindScrollViewer()
        => this.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();

    private void QueueAutoWidthPass()
    {
        if (_autoWidthPassQueued || Source is null || VisualRoot is null)
            return;
        _autoWidthPassQueued = true;
        Dispatcher.UIThread.Post(() =>
        {
            _autoWidthPassQueued = false;
            UpdateAutoWidths();
        }, DispatcherPriority.Background);
    }

    private void UpdateAutoWidths()
    {
        if (Source is null || VisualRoot is null)
            return;
        var cellSamples = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var cell in this.GetVisualDescendants().OfType<TableViewCell>())
        {
            if (cell.Column?.Header is not DynamicTableViewHeader header || cell.DataContext is not { } row ||
                FindDefinition(header.Column.Key) is not { } definition)
                continue;
            if (definition.WidthMode == DynamicTableViewWidthMode.Header || _manuallySizedColumns.Contains(definition.Key))
                continue;
            var measured = MeasureText(definition.GetDisplayValue(row)) + 24;
            if (!cellSamples.TryGetValue(definition.Key, out var current) || measured > current)
                cellSamples[definition.Key] = measured;
        }

        foreach (var definition in Source.Columns)
        {
            if (definition.WidthMode is DynamicTableViewWidthMode.Pixel or DynamicTableViewWidthMode.Star ||
                _manuallySizedColumns.Contains(definition.Key) || !_nativeColumns.TryGetValue(definition.Key, out var native))
                continue;

            var desired = definition.WidthMode == DynamicTableViewWidthMode.Cells ? definition.MinWidth : MeasureText(definition.Header.ToString()) + 24;
        if ((definition.WidthMode is DynamicTableViewWidthMode.Auto or DynamicTableViewWidthMode.Cells) && cellSamples.TryGetValue(definition.Key, out var cells))
                desired = Math.Max(desired, cells);
            desired = Math.Max(definition.MinWidth, desired);
            var currentWidth = native.ActualWidth;
            if (!double.IsFinite(currentWidth) || desired > currentWidth + 1)
                native.Width = new GridLength(desired);
        }
    }

    private static double MeasureText(string? text)
    {
        var presenter = new TextBlock { Text = text ?? string.Empty };
        presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return presenter.DesiredSize.Width;
    }

    private void OnGridPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
        {
            DataGridRightClick(e);
            return;
        }

        if (FindHeader(e.Source) is not { } header || FindDefinition(header.Column.Key) is not { } definition)
            return;
        var native = header.NativeColumn;
        if (native is null)
            return;
        var position = e.GetPosition(header);
        if (position.X >= header.Bounds.Width - 7)
        {
            _manuallySizedColumns.Add(definition.Key);
            definition.WidthMode = DynamicTableViewWidthMode.Pixel;
        }
        _pointerDownColumn = Columns.IndexOf(native);
        _pointerDownPosition = e.GetPosition(this);
        _headerDragStarted = false;
    }

    private void OnGridPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_pointerDownColumn < 0 || e.GetCurrentPoint(this).Properties.IsLeftButtonPressed is false)
            return;
        var current = e.GetPosition(this);
        if (!_headerDragStarted && Math.Abs(current.X - _pointerDownPosition.X) < 5)
            return;
        _headerDragStarted = true;
        if (FindHeader(e.Source) is not { } targetHeader || Source is null)
            return;
        var targetIndex = Columns.IndexOf(targetHeader.NativeColumn);
        if (targetIndex < 0 || targetIndex == _pointerDownColumn)
            return;
        var moving = Source.Columns[_pointerDownColumn];
        Source.Columns.RemoveAt(_pointerDownColumn);
        Source.Columns.Insert(targetIndex, moving);
        _pointerDownColumn = targetIndex;
        e.Handled = true;
    }

    private void OnGridPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _pointerDownColumn = -1;
        _headerDragStarted = false;
    }

    private void DataGridRightClick(PointerPressedEventArgs e)
    {
        if (FindRow(e.Source) is not { DataContext: { } clicked } row)
            return;
        IReadOnlyList<object> targets;
        var rowIsSelected = Selection?.SelectedItems.Any(item =>
            Source?.AreSameRows(clicked, item) == true) == true;
        if (rowIsSelected)
            targets = Selection.SelectedItems.Where(static item => item is not null).Cast<object>().ToArray();
        else
            targets = [clicked];
        ContextMenuRequested?.Invoke(this, new(clicked, targets));
        if (ContextMenu is ContextMenu contextMenu && ContextMenuItemsFactory is not null)
            contextMenu.ItemsSource = ContextMenuItemsFactory(targets);
    }

    internal void SortColumn(string key)
    {
        if (Source is null)
            return;
        var current = Source.SortDescriptors.FirstOrDefault(sort => string.Equals(sort.ColumnKey, key, StringComparison.Ordinal));
        var sorts = Source.SortDescriptors.ToList();
        if (current is null)
            sorts.Add(new(key, ListSortDirection.Ascending));
        else if (current.Direction == ListSortDirection.Ascending)
            sorts[sorts.IndexOf(current)] = current with { Direction = ListSortDirection.Descending };
        else
            sorts.Remove(current);
        Source.SetSort(sorts);
    }

    internal FlyoutBase MakeFilterFlyout(DynamicTableViewColumn definition)
    {
        if (Source is null)
            return new Flyout();
        var factory = definition.GetFilterFlyoutFactory();
        var content = factory is null
            ? new DynamicTableViewFilterFlyout(definition, Source)
            : factory(new DynamicTableViewFilterContext(definition, Source));
        return new Flyout { Content = content };
    }

    private DynamicTableViewColumn? FindDefinition(string key)
        => Source?.Columns.FirstOrDefault(column => string.Equals(column.Key, key, StringComparison.Ordinal));

    private static int IndexOfColumn(IList<DynamicTableViewColumn> columns, string key)
    {
        for (var i = 0; i < columns.Count; i++)
            if (string.Equals(columns[i].Key, key, StringComparison.Ordinal)) return i;
        return -1;
    }

    private static DynamicTableViewHeader? FindHeader(object? source)
        => source is Visual visual ? visual.GetSelfAndVisualAncestors().OfType<DynamicTableViewHeader>().FirstOrDefault() : null;

    private static TableViewRow? FindRow(object? source)
        => source is Visual visual ? visual.GetSelfAndVisualAncestors().OfType<TableViewRow>().FirstOrDefault() : null;

    private static bool IsRowOrCellSource(object? source)
        => source is Visual visual && visual.GetSelfAndVisualAncestors().Any(static item => item is TableViewRow or TableViewCell);
}
