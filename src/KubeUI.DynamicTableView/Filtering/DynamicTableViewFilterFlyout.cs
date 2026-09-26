using Avalonia.Interactivity;

namespace KubeUI.DynamicTableView;

internal sealed partial class DynamicTableViewFilterFlyout : TemplatedControl
{
    private readonly DynamicTableViewColumn _column;
    private readonly IDynamicTableViewSource _source;
    private readonly bool _isBoolean;
    private readonly bool _isNumber;
    private readonly bool _isDate;
    private Button? _applyButton;
    private Button? _clearButton;
    private ListBox? _multipleChoiceBox;

    public DynamicTableViewFilterFlyout(DynamicTableViewColumn column, IDynamicTableViewSource source)
    {
        _column = column ?? throw new ArgumentNullException(nameof(column));
        _source = source ?? throw new ArgumentNullException(nameof(source));
        ColumnHeader = column.Header.ToString() ?? string.Empty;
        _isBoolean = column.ValueType == typeof(bool);
        _isNumber = IsNumericType(column.ValueType);
        _isDate = column.ValueType == typeof(DateTime) || column.ValueType == typeof(DateTimeOffset);
        FilterChoices = GetChoices(column, _isBoolean);
        OperatorChoices = GetOperators(_isBoolean, FilterChoices.Count > 0, _isNumber, _isDate)
            .Select(static filterOperator => new DynamicTableViewFilterChoice(
                DynamicTableViewResources.GetFilterOperatorLabel(filterOperator), filterOperator))
            .ToArray();
        SelectedOperatorChoice = OperatorChoices.FirstOrDefault();
        Classes.Add("dynamic-table-view-filter-flyout");
        UpdateInputVisibility();
    }

    [GeneratedDirectProperty]
    public partial string ColumnHeader { get; set; } = string.Empty;

    [GeneratedDirectProperty]
    public partial IReadOnlyList<DynamicTableViewFilterChoice> OperatorChoices { get; set; } = [];

    [GeneratedDirectProperty]
    public partial IReadOnlyList<DynamicTableViewFilterChoice> FilterChoices { get; set; } = [];

    [GeneratedDirectProperty]
    public partial DynamicTableViewFilterChoice? SelectedOperatorChoice { get; set; }

    [GeneratedDirectProperty]
    public partial DynamicTableViewFilterChoice? SelectedChoice { get; set; }

    [GeneratedDirectProperty]
    public partial string? FirstValueText { get; set; }

    [GeneratedDirectProperty]
    public partial string? SecondValueText { get; set; }

    [GeneratedDirectProperty]
    public partial bool IsValueInputVisible { get; set; }

    [GeneratedDirectProperty]
    public partial bool IsSecondValueInputVisible { get; set; }

    [GeneratedDirectProperty]
    public partial bool IsSingleChoiceInputVisible { get; set; }

    [GeneratedDirectProperty]
    public partial bool IsMultiChoiceInputVisible { get; set; }

    public event EventHandler? FilterValidationFailed;

    protected override Type StyleKeyOverride => typeof(DynamicTableViewFilterFlyout);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_applyButton is not null)
            _applyButton.Click -= ApplyButtonOnClick;
        if (_clearButton is not null)
            _clearButton.Click -= ClearButtonOnClick;
        if (_multipleChoiceBox is not null)
            _multipleChoiceBox.SelectionChanged -= MultipleChoiceBoxOnSelectionChanged;

        base.OnApplyTemplate(e);
        _applyButton = e.NameScope.Find<Button>("PART_ApplyButton");
        _clearButton = e.NameScope.Find<Button>("PART_ClearButton");
        _multipleChoiceBox = e.NameScope.Find<ListBox>("PART_MultipleChoiceBox");
        if (_applyButton is not null)
            _applyButton.Click += ApplyButtonOnClick;
        if (_clearButton is not null)
            _clearButton.Click += ClearButtonOnClick;
        if (_multipleChoiceBox is not null)
            _multipleChoiceBox.SelectionChanged += MultipleChoiceBoxOnSelectionChanged;
    }

    partial void OnSelectedOperatorChoicePropertyChanged(DynamicTableViewFilterChoice? newValue)
        => UpdateInputVisibility();

    private void UpdateInputVisibility()
    {
        DynamicTableViewFilterOperator? filterOperator = SelectedOperatorChoice?.Value is DynamicTableViewFilterOperator value ? value : null;
        var isRange = filterOperator is DynamicTableViewFilterOperator.Between or DynamicTableViewFilterOperator.NotBetween;
        var isIn = filterOperator == DynamicTableViewFilterOperator.In;
        var isValueFree = filterOperator is DynamicTableViewFilterOperator.IsTrue or DynamicTableViewFilterOperator.IsFalse or DynamicTableViewFilterOperator.IsNull or DynamicTableViewFilterOperator.IsNotNull;
        IsSingleChoiceInputVisible = FilterChoices.Count > 0 && !isIn;
        IsMultiChoiceInputVisible = FilterChoices.Count > 0 && isIn;
        IsValueInputVisible = !isValueFree && FilterChoices.Count == 0;
        IsSecondValueInputVisible = isRange && (_isNumber || _isDate);
    }

    private void ApplyButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (SelectedOperatorChoice?.Value is not DynamicTableViewFilterOperator filterOperator)
            return;

        object? value = null;
        object? secondValue = null;
        IReadOnlyList<object?>? values = null;
        if (filterOperator is DynamicTableViewFilterOperator.IsTrue or DynamicTableViewFilterOperator.IsFalse or DynamicTableViewFilterOperator.IsNull or DynamicTableViewFilterOperator.IsNotNull)
        {
            _source.SetFilter(new(_column.Key, filterOperator));
            return;
        }

        if (FilterChoices.Count > 0)
        {
            if (filterOperator == DynamicTableViewFilterOperator.In)
            {
                values = _multipleChoiceBox?.SelectedItems
                    .OfType<DynamicTableViewFilterChoice>()
                    .Select(static choice => choice.Value)
                    .ToArray() ?? [];
                if (values.Count == 0)
                {
                    FilterValidationFailed?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }
            else
            {
                value = SelectedChoice?.Value;
            }
        }
        else if (filterOperator == DynamicTableViewFilterOperator.In)
        {
            var tokens = (FirstValueText ?? string.Empty).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var parsed = new object?[tokens.Length];
            for (var i = 0; i < tokens.Length; i++)
            {
                if (!TryParseValue(tokens[i], out parsed[i]))
                {
                    FilterValidationFailed?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }
            values = parsed;
        }
        else if (!TryParseValue(FirstValueText ?? string.Empty, out value))
        {
            FilterValidationFailed?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (filterOperator is DynamicTableViewFilterOperator.Between or DynamicTableViewFilterOperator.NotBetween)
        {
            if (!TryParseValue(SecondValueText ?? string.Empty, out secondValue))
            {
                FilterValidationFailed?.Invoke(this, EventArgs.Empty);
                return;
            }
        }

        _source.SetFilter(new(_column.Key, filterOperator, value, secondValue, values));
    }

    private void ClearButtonOnClick(object? sender, RoutedEventArgs e)
        => _source.SetFilter(null, _column.Key);

    private void MultipleChoiceBoxOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SelectedOperatorChoice?.Value is DynamicTableViewFilterOperator selectedOperator && selectedOperator == DynamicTableViewFilterOperator.In)
            UpdateInputVisibility();
    }

    private bool TryParseValue(string text, out object? value)
    {
        if (_isNumber)
        {
            var parsed = decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var number);
            value = number;
            return parsed;
        }
        if (_isDate)
        {
            var parsed = DateTimeOffset.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var date);
            value = date;
            return parsed;
        }
        value = text;
        return true;
    }

    private static IReadOnlyList<DynamicTableViewFilterChoice> GetChoices(DynamicTableViewColumn column, bool isBoolean)
    {
        if (!isBoolean)
            return column.GetFilterChoices();
        return column.GetFilterChoices() is { Count: > 0 } choices
            ? choices
            : [new(DynamicTableViewResources.FilterFalse, false), new(DynamicTableViewResources.FilterTrue, true)];
    }

    private static IReadOnlyList<DynamicTableViewFilterOperator> GetOperators(bool boolean, bool choices, bool number, bool date)
    {
        if (boolean)
            return [DynamicTableViewFilterOperator.Equals, DynamicTableViewFilterOperator.NotEquals];
        if (choices)
            return [DynamicTableViewFilterOperator.Equals, DynamicTableViewFilterOperator.NotEquals, DynamicTableViewFilterOperator.In];
        if (date || number)
            return [DynamicTableViewFilterOperator.Equals, DynamicTableViewFilterOperator.NotEquals, DynamicTableViewFilterOperator.GreaterThan,
                DynamicTableViewFilterOperator.GreaterThanOrEqual, DynamicTableViewFilterOperator.LessThan,
                DynamicTableViewFilterOperator.LessThanOrEqual, DynamicTableViewFilterOperator.Between,
                DynamicTableViewFilterOperator.NotBetween, DynamicTableViewFilterOperator.In];
        return [DynamicTableViewFilterOperator.Contains, DynamicTableViewFilterOperator.DoesNotContain, DynamicTableViewFilterOperator.Equals,
            DynamicTableViewFilterOperator.NotEquals, DynamicTableViewFilterOperator.StartsWith,
            DynamicTableViewFilterOperator.DoesNotStartWith, DynamicTableViewFilterOperator.EndsWith,
            DynamicTableViewFilterOperator.DoesNotEndWith, DynamicTableViewFilterOperator.In,
            DynamicTableViewFilterOperator.IsNull, DynamicTableViewFilterOperator.IsNotNull];
    }

    private static bool IsNumericType(Type type)
        => type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) ||
           type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong) ||
           type == typeof(float) || type == typeof(double) || type == typeof(decimal);
}
