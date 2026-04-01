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
                apply: () => ApplyEnumFilter(filteringModel, column, context!.SelectedValue?.Value),
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
                apply: () => ApplyDateFilter(filteringModel, column, columnDefinition.ValueType, context!.SelectedOperator.Operator, context.Amount, context.SelectedUnit.Unit),
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
                apply: () => ApplyNumericFilter(filteringModel, column, context!.SelectedOperator.Operator, context.Value, context.SecondValue),
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
            apply: () => ApplyTextFilter(filteringModel, column, textContext!.SelectedOperator.Operator, textContext.Query),
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

    public void ApplyTextFilter(IFilteringModel filteringModel, DataGridColumnDefinition column, FilteringOperator filterOperator, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            ClearColumnFilter(filteringModel, column);
            return;
        }

        filteringModel.SetOrUpdate(new FilteringDescriptor(
            columnId: column,
            @operator: filterOperator,
            propertyPath: null,
            value: query.Trim(),
            stringComparison: StringComparison.OrdinalIgnoreCase));
    }

    public void ApplyEnumFilter(IFilteringModel filteringModel, DataGridColumnDefinition column, object? value)
    {
        if (value == null)
        {
            ClearColumnFilter(filteringModel, column);
            return;
        }

        filteringModel.SetOrUpdate(new FilteringDescriptor(
            columnId: column,
            @operator: FilteringOperator.Equals,
            propertyPath: null,
            value: value));
    }

    public void ApplyNumericFilter(IFilteringModel filteringModel, DataGridColumnDefinition column, FilteringOperator filterOperator, double? value, double? secondValue)
    {
        if (filterOperator == FilteringOperator.Between)
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
                filterOperator = FilteringOperator.GreaterThanOrEqual;
            }
            else
            {
                filterOperator = FilteringOperator.LessThanOrEqual;
                value = secondValue;
            }
        }

        if (value == null)
        {
            ClearColumnFilter(filteringModel, column);
            return;
        }

        filteringModel.SetOrUpdate(new FilteringDescriptor(
            columnId: column,
            @operator: filterOperator,
            propertyPath: null,
            value: value.Value));
    }

    public void ApplyDateFilter(
        IFilteringModel filteringModel,
        DataGridColumnDefinition column,
        Type valueType,
        FilteringOperator filterOperator,
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
        filteringModel.SetOrUpdate(new FilteringDescriptor(
            columnId: column,
            @operator: filterOperator,
            propertyPath: null,
            value: value));
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
