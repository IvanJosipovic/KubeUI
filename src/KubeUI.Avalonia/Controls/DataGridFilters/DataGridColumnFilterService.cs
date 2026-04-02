using System.Globalization;
using Avalonia.Controls.DataGridFiltering;
using KubeUI.Avalonia.Resources;

namespace KubeUI.Avalonia.Controls.DataGridFilters;

internal enum DataGridColumnFilterKind
{
    Text,
    Enum,
    Numeric,
    Date,
}

internal sealed class DataGridColumnFilterDefinition
{
    private readonly Action _load;

    public DataGridColumnFilterDefinition(DataGridColumnFilterKind kind, object context, Action load)
    {
        Kind = kind;
        Context = context;
        _load = load;
    }

    public DataGridColumnFilterKind Kind { get; }

    public object Context { get; }

    public void Load()
    {
        _load();
    }
}

internal sealed class DataGridColumnFilterService
{
    public DataGridColumnFilterDefinition CreateDefinition(
        IResourceListColumn columnDefinition,
        DataGridColumnDefinition column,
        IFilteringModel filteringModel)
    {
        if (IsEnumType(columnDefinition.ValueType))
        {
            EnumFilterFlyoutContext? context = null;
            context = new EnumFilterFlyoutContext(
                columnDefinition.Name,
                columnDefinition.ValueType,
                apply: () => ApplyEnumFilter(filteringModel, column, context!.SelectedOperator, context!.SelectedValue?.Value),
                clear: () =>
                {
                    ClearColumnFilter(filteringModel, column);
                    context!.ClearState();
                });

            return new DataGridColumnFilterDefinition(
                DataGridColumnFilterKind.Enum,
                context,
                () => context.LoadFromDescriptor(GetCurrentFilterDescriptor(filteringModel, column)));
        }

        if (IsDateType(columnDefinition.ValueType))
        {
            DateFilterFlyoutContext? context = null;
            context = new DateFilterFlyoutContext(
                columnDefinition.Name,
                apply: () => ApplyDateFilter(filteringModel, column, columnDefinition.ValueType, context!.SelectedOperator, context.Amount, context.SelectedUnit.Unit),
                clear: () =>
                {
                    ClearColumnFilter(filteringModel, column);
                    context!.ClearState();
                });

            return new DataGridColumnFilterDefinition(
                DataGridColumnFilterKind.Date,
                context,
                () => context.LoadFromDescriptor(GetCurrentFilterDescriptor(filteringModel, column)));
        }

        if (IsNumericType(columnDefinition.ValueType))
        {
            NumericFilterFlyoutContext? context = null;
            context = new NumericFilterFlyoutContext(
                columnDefinition.Name,
                apply: () => ApplyNumericFilter(filteringModel, column, context!.SelectedOperator, context.Value, context.SecondValue),
                clear: () =>
                {
                    ClearColumnFilter(filteringModel, column);
                    context!.ClearState();
                });

            return new DataGridColumnFilterDefinition(
                DataGridColumnFilterKind.Numeric,
                context,
                () => context.LoadFromDescriptor(GetCurrentFilterDescriptor(filteringModel, column)));
        }

        TextFilterFlyoutContext? textContext = null;
        textContext = new TextFilterFlyoutContext(
            columnDefinition.Name,
            apply: () => ApplyTextFilter(filteringModel, column, textContext!.SelectedOperator, textContext.Query),
            clear: () =>
            {
                ClearColumnFilter(filteringModel, column);
                textContext!.ClearState();
            });

        return new DataGridColumnFilterDefinition(
            DataGridColumnFilterKind.Text,
            textContext,
            () => textContext.LoadFromDescriptor(GetCurrentFilterDescriptor(filteringModel, column)));
    }

    public FilteringDescriptor? GetCurrentFilterDescriptor(IFilteringModel filteringModel, DataGridColumnDefinition column)
    {
        return filteringModel.Descriptors.FirstOrDefault(descriptor =>
            ReferenceEquals(descriptor.ColumnId, column) || Equals(descriptor.ColumnId, column));
    }

    public void ClearColumnFilter(IFilteringModel filteringModel, DataGridColumnDefinition column)
    {
        filteringModel.Remove(column);
    }

    public void ApplyTextFilter(IFilteringModel filteringModel, DataGridColumnDefinition column, FilterOperatorChoice filterOperator, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            ClearColumnFilter(filteringModel, column);
            return;
        }

        var trimmed = query.Trim();
        if (filterOperator.CustomKey != null)
        {
            filteringModel.SetOrUpdate(CreateCustomDescriptor(
                column,
                filterOperator.CustomKey,
                CreateTextPredicate(column, filterOperator.CustomKey, trimmed, StringComparison.OrdinalIgnoreCase),
                value: trimmed,
                stringComparison: StringComparison.OrdinalIgnoreCase));
            return;
        }

        filteringModel.SetOrUpdate(new FilteringDescriptor(
            columnId: column,
            @operator: filterOperator.Operator,
            propertyPath: null,
            value: trimmed,
            stringComparison: StringComparison.OrdinalIgnoreCase));
    }

    public void ApplyEnumFilter(IFilteringModel filteringModel, DataGridColumnDefinition column, FilterOperatorChoice filterOperator, object? value)
    {
        if (value == null)
        {
            ClearColumnFilter(filteringModel, column);
            return;
        }

        filteringModel.SetOrUpdate(new FilteringDescriptor(
            columnId: column,
            @operator: filterOperator.Operator,
            propertyPath: null,
            value: value));
    }

    public void ApplyNumericFilter(IFilteringModel filteringModel, DataGridColumnDefinition column, FilterOperatorChoice filterOperator, double? value, double? secondValue)
    {
        FilteringOperator effectiveOperator = filterOperator.Operator;

        if (filterOperator.Operator == FilteringOperator.Between)
        {
            if (value == null && secondValue == null)
            {
                ClearColumnFilter(filteringModel, column);
                return;
            }

            if (value != null && secondValue != null)
            {
                filteringModel.SetOrUpdate(new FilteringDescriptor(
                    columnId: column,
                    @operator: FilteringOperator.Between,
                    propertyPath: null,
                    values: [value.Value, secondValue.Value]));
                return;
            }

            if (value != null)
            {
                effectiveOperator = FilteringOperator.GreaterThanOrEqual;
            }
            else
            {
                effectiveOperator = FilteringOperator.LessThanOrEqual;
                value = secondValue;
            }
        }

        if (value == null)
        {
            ClearColumnFilter(filteringModel, column);
            return;
        }

        if (filterOperator.CustomKey != null)
        {
            if (filterOperator.CustomKey == FilterOperatorKeys.NumericNotBetween && secondValue == null)
            {
                ClearColumnFilter(filteringModel, column);
                return;
            }

            filteringModel.SetOrUpdate(CreateCustomDescriptor(
                column,
                filterOperator.CustomKey,
                CreateNumericPredicate(column, filterOperator.CustomKey, value.Value, secondValue),
                value: value.Value,
                values: secondValue != null ? [value.Value, secondValue.Value] : null));
            return;
        }

        filteringModel.SetOrUpdate(new FilteringDescriptor(
            columnId: column,
            @operator: effectiveOperator,
            propertyPath: null,
            value: value.Value));
    }

    public void ApplyDateFilter(
        IFilteringModel filteringModel,
        DataGridColumnDefinition column,
        Type valueType,
        FilterOperatorChoice filterOperator,
        double? amount,
        DateRelativeUnit unit)
    {
        if (amount == null || amount <= 0)
        {
            ClearColumnFilter(filteringModel, column);
            return;
        }

        var threshold = ComputeRelativeDateThreshold(amount.Value, unit);
        object value = IsDateTimeOffsetType(valueType) ? threshold : threshold.UtcDateTime;

        if (filterOperator.CustomKey != null)
        {
            filteringModel.SetOrUpdate(CreateCustomDescriptor(
                column,
                filterOperator.CustomKey,
                CreateDatePredicate(column, filterOperator.CustomKey, value),
                value: value));
            return;
        }

        filteringModel.SetOrUpdate(new FilteringDescriptor(
            columnId: column,
            @operator: filterOperator.Operator,
            propertyPath: null,
            value: value));
    }

    private static FilteringDescriptor CreateCustomDescriptor(
        DataGridColumnDefinition column,
        string customKey,
        Func<object, bool> predicate,
        object? value = null,
        IReadOnlyList<object>? values = null,
        StringComparison? stringComparison = null)
    {
        return new FilteringDescriptor(
            columnId: column,
            @operator: FilteringOperator.Custom,
            propertyPath: customKey,
            value: value,
            values: values,
            predicate: predicate,
            stringComparison: stringComparison);
    }

    private static Func<object, bool> CreateTextPredicate(
        DataGridColumnDefinition column,
        string customKey,
        string query,
        StringComparison comparison)
    {
        return customKey switch
        {
            FilterOperatorKeys.TextNotContains => item => !Contains(GetColumnValue(column, item), query, comparison),
            FilterOperatorKeys.TextNotStartsWith => item => !StartsWith(GetColumnValue(column, item), query, comparison),
            FilterOperatorKeys.TextNotEndsWith => item => !EndsWith(GetColumnValue(column, item), query, comparison),
            _ => item => !Equals(GetColumnValue(column, item), query)
        };
    }

    private static Func<object, bool> CreateNumericPredicate(
        DataGridColumnDefinition column,
        string customKey,
        double value,
        double? secondValue)
    {
        CultureInfo culture = CultureInfo.InvariantCulture;
        return customKey switch
        {
            FilterOperatorKeys.NumericNotBetween => item => secondValue.HasValue && !Between(GetColumnValue(column, item), [value, secondValue.Value], culture),
            FilterOperatorKeys.NumericNotGreaterThan => item => Compare(GetColumnValue(column, item), value, culture) <= 0,
            FilterOperatorKeys.NumericNotGreaterThanOrEqual => item => Compare(GetColumnValue(column, item), value, culture) < 0,
            FilterOperatorKeys.NumericNotLessThan => item => Compare(GetColumnValue(column, item), value, culture) >= 0,
            FilterOperatorKeys.NumericNotLessThanOrEqual => item => Compare(GetColumnValue(column, item), value, culture) > 0,
            _ => item => !Equals(GetColumnValue(column, item), value)
        };
    }

    private static Func<object, bool> CreateDatePredicate(
        DataGridColumnDefinition column,
        string customKey,
        object value)
    {
        CultureInfo culture = CultureInfo.InvariantCulture;
        return customKey switch
        {
            FilterOperatorKeys.DateNotNewerThan => item => Compare(GetColumnValue(column, item), value, culture) <= 0,
            FilterOperatorKeys.DateNotOlderThan => item => Compare(GetColumnValue(column, item), value, culture) >= 0,
            _ => item => !Equals(GetColumnValue(column, item), value)
        };
    }

    private static object? GetColumnValue(DataGridColumnDefinition column, object item)
    {
        try
        {
            return column.ValueAccessor.GetValue(item);
        }
        catch
        {
            return null;
        }
    }

    private static int Compare(object? left, object? right, CultureInfo culture)
    {
        if (left == null && right == null)
        {
            return 0;
        }

        if (left == null)
        {
            return -1;
        }

        if (right == null)
        {
            return 1;
        }

        if (TryGetDateTimeOffset(left, out var leftDate) && TryGetDateTimeOffset(right, out var rightDate))
        {
            return leftDate.CompareTo(rightDate);
        }

        if (left is IComparable comparable)
        {
            try
            {
                return comparable.CompareTo(ChangeType(right, left.GetType(), culture));
            }
            catch
            {
            }
        }

        return Comparer<object>.Default.Compare(left, right);
    }

    private static bool Between(object? source, IReadOnlyList<object?>? values, CultureInfo culture)
    {
        if (values == null || values.Count < 2)
        {
            return false;
        }

        return Compare(source, values[0], culture) >= 0 && Compare(source, values[1], culture) <= 0;
    }

    private static bool Contains(object? source, object? target, StringComparison comparison)
    {
        return source is string s && target is string t && s.Contains(t, comparison);
    }

    private static bool StartsWith(object? source, object? target, StringComparison comparison)
    {
        return source is string s && target is string t && s.StartsWith(t, comparison);
    }

    private static bool EndsWith(object? source, object? target, StringComparison comparison)
    {
        return source is string s && target is string t && s.EndsWith(t, comparison);
    }

    private static bool TryGetDateTimeOffset(object? value, out DateTimeOffset dateTimeOffset)
    {
        switch (value)
        {
            case DateTimeOffset dto:
                dateTimeOffset = dto;
                return true;
            case DateTime dateTime:
                dateTimeOffset = dateTime.Kind == DateTimeKind.Unspecified
                    ? new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc))
                    : new DateTimeOffset(dateTime.ToUniversalTime());
                return true;
            default:
                dateTimeOffset = default;
                return false;
        }
    }

    private static object? ChangeType(object? value, Type targetType, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }

        if (targetType.IsInstanceOfType(value))
        {
            return value;
        }

        try
        {
            return Convert.ChangeType(value, targetType, culture);
        }
        catch
        {
            return value;
        }
    }

    public DateTimeOffset ComputeRelativeDateThreshold(double amount, DateRelativeUnit unit)
    {
        var roundedAmount = Math.Max(1, (int)Math.Round(amount, MidpointRounding.AwayFromZero));
        var now = DateTimeOffset.UtcNow;

        return unit switch
        {
            DateRelativeUnit.Minutes => now.AddMinutes(-roundedAmount),
            DateRelativeUnit.Hours => now.AddHours(-roundedAmount),
            DateRelativeUnit.Days => now.AddDays(-roundedAmount),
            DateRelativeUnit.Months => now.AddMonths(-roundedAmount),
            _ => now.AddDays(-roundedAmount)
        };
    }

    public bool IsNumericType(Type type)
    {
        var targetType = Nullable.GetUnderlyingType(type) ?? type;
        return targetType == typeof(byte) ||
               targetType == typeof(sbyte) ||
               targetType == typeof(short) ||
               targetType == typeof(ushort) ||
               targetType == typeof(int) ||
               targetType == typeof(uint) ||
               targetType == typeof(long) ||
               targetType == typeof(ulong) ||
               targetType == typeof(float) ||
               targetType == typeof(double) ||
               targetType == typeof(decimal);
    }

    public bool IsDateType(Type type)
    {
        var targetType = Nullable.GetUnderlyingType(type) ?? type;
        return targetType == typeof(DateTime) || targetType == typeof(DateTimeOffset);
    }

    public bool IsEnumType(Type type)
    {
        var targetType = Nullable.GetUnderlyingType(type) ?? type;
        return targetType.IsEnum;
    }

    private static bool IsDateTimeOffsetType(Type type)
    {
        var targetType = Nullable.GetUnderlyingType(type) ?? type;
        return targetType == typeof(DateTimeOffset);
    }
}
