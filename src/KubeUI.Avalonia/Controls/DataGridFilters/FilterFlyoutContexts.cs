using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Avalonia.Controls.DataGridFiltering;

namespace KubeUI.Avalonia.Controls.DataGridFilters;

internal sealed record FilterOperatorChoice(FilteringOperator Operator, string Label, string? CustomKey = null)
{
    public bool Matches(FilteringDescriptor? descriptor)
    {
        if (descriptor == null)
        {
            return false;
        }

        if (CustomKey == null)
        {
            return descriptor.Operator == Operator;
        }

        return descriptor.Operator == FilteringOperator.Custom &&
               string.Equals(descriptor.PropertyPath, CustomKey, StringComparison.Ordinal);
    }
}

internal enum DateRelativeUnit
{
    Minutes,
    Hours,
    Days,
    Months
}

internal sealed record DateRelativeUnitChoice(DateRelativeUnit Unit, string Label);

internal sealed record EnumFilterChoice(string Label, object? Value);

internal abstract class ColumnFilterFlyoutContextBase : ObservableObject
{
    private FilterOperatorChoice _selectedOperator = null!;

    protected ColumnFilterFlyoutContextBase(string title, IReadOnlyList<FilterOperatorChoice> operators, Action apply, Action clear)
    {
        Title = title;
        Operators = new ObservableCollection<FilterOperatorChoice>(operators);
        _selectedOperator = Operators[0];
        ApplyCommand = new RelayCommand(apply);
        ClearCommand = new RelayCommand(clear);
    }

    public string Title { get; }

    public ObservableCollection<FilterOperatorChoice> Operators { get; }

    public ICommand ApplyCommand { get; }

    public ICommand ClearCommand { get; }

    public FilterOperatorChoice SelectedOperator
    {
        get => _selectedOperator;
        set
        {
            if (SetProperty(ref _selectedOperator, value))
            {
                OnPropertyChanged(nameof(IsRangeVisible));
            }
        }
    }

    public bool IsRangeVisible =>
        SelectedOperator.Operator == FilteringOperator.Between ||
        string.Equals(SelectedOperator.CustomKey, FilterOperatorKeys.NumericNotBetween, StringComparison.Ordinal);
}

internal sealed class EnumFilterFlyoutContext : ColumnFilterFlyoutContextBase
{
    private EnumFilterChoice? _selectedValue;

    public EnumFilterFlyoutContext(string title, Type enumType, Action apply, Action clear)
        : base(title, ResourceListFilterFlyoutOptions.EnumOperators, apply, clear)
    {
        Options = CreateOptions(enumType);
        _selectedValue = Options[0];
    }

    public ObservableCollection<EnumFilterChoice> Options { get; }

    public EnumFilterChoice? SelectedValue
    {
        get => _selectedValue;
        set => SetProperty(ref _selectedValue, value);
    }

    public void LoadFromDescriptor(FilteringDescriptor? descriptor)
    {
        if (descriptor == null)
        {
            ClearState();
            return;
        }

        SelectedOperator = Operators.FirstOrDefault(option => option.Matches(descriptor)) ?? Operators[0];
        var selected = Options.FirstOrDefault(option => Equals(option.Value, descriptor.Value));
        SelectedValue = selected ?? Options[0];
    }

    public void ClearState()
    {
        SelectedOperator = Operators[0];
        SelectedValue = Options[0];
    }

    private static ObservableCollection<EnumFilterChoice> CreateOptions(Type enumType)
    {
        var targetType = Nullable.GetUnderlyingType(enumType) ?? enumType;
        var items = new List<EnumFilterChoice> { new(Assets.Resources.DataGridFilterFlyout_Any, null) };

        if (targetType.IsEnum)
        {
            foreach (var value in Enum.GetValues(targetType))
            {
                items.Add(new EnumFilterChoice(value.ToString() ?? string.Empty, value));
            }
        }

        return new ObservableCollection<EnumFilterChoice>(items);
    }
}

internal sealed class TextFilterFlyoutContext : ColumnFilterFlyoutContextBase
{
    private string _query = string.Empty;

    public TextFilterFlyoutContext(string title, Action apply, Action clear)
        : base(title, ResourceListFilterFlyoutOptions.TextOperators, apply, clear)
    {
    }

    public string Query
    {
        get => _query;
        set => SetProperty(ref _query, value);
    }

    public void LoadFromDescriptor(FilteringDescriptor? descriptor)
    {
        if (descriptor == null)
        {
            ClearState();
            return;
        }

        SelectedOperator = Operators.FirstOrDefault(x => x.Matches(descriptor)) ?? Operators[0];
        Query = descriptor.Value?.ToString() ?? string.Empty;
    }

    public void ClearState()
    {
        SelectedOperator = Operators[0];
        Query = string.Empty;
    }
}

internal sealed class NumericFilterFlyoutContext : ColumnFilterFlyoutContextBase
{
    private double? _value;
    private double? _secondValue;

    public NumericFilterFlyoutContext(string title, Action apply, Action clear)
        : base(title, ResourceListFilterFlyoutOptions.NumericOperators, apply, clear)
    {
    }

    public double? Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public double? SecondValue
    {
        get => _secondValue;
        set => SetProperty(ref _secondValue, value);
    }

    public void LoadFromDescriptor(FilteringDescriptor? descriptor)
    {
        if (descriptor == null)
        {
            ClearState();
            return;
        }

        SelectedOperator = Operators.FirstOrDefault(x => x.Matches(descriptor)) ?? Operators[0];

        if ((descriptor.Operator == FilteringOperator.Between ||
             string.Equals(SelectedOperator.CustomKey, FilterOperatorKeys.NumericNotBetween, StringComparison.Ordinal)) &&
            descriptor.Values is { Count: >= 2 })
        {
            Value = ToDouble(descriptor.Values[0]);
            SecondValue = ToDouble(descriptor.Values[1]);
            return;
        }

        Value = ToDouble(descriptor.Value);
        SecondValue = null;
    }

    public void ClearState()
    {
        SelectedOperator = Operators[0];
        Value = null;
        SecondValue = null;
    }

    private static double? ToDouble(object? value)
    {
        return value switch
        {
            null => null,
            double d => d,
            float f => f,
            decimal m => (double)m,
            int i => i,
            long l => l,
            uint ui => ui,
            ulong ul => ul,
            short s => s,
            ushort us => us,
            byte b => b,
            sbyte sb => sb,
            _ => value is IConvertible convertible ? Convert.ToDouble(convertible, CultureInfo.InvariantCulture) : null
        };
    }
}

internal sealed class DateFilterFlyoutContext : ColumnFilterFlyoutContextBase
{
    private double? _amount = 1;
    private DateRelativeUnitChoice _selectedUnit = null!;

    public DateFilterFlyoutContext(string title, Action apply, Action clear)
        : base(title, ResourceListFilterFlyoutOptions.DateOperators, apply, clear)
    {
        Units = new ObservableCollection<DateRelativeUnitChoice>(ResourceListFilterFlyoutOptions.DateRelativeUnits);
        _selectedUnit = Units[0];
    }

    public ObservableCollection<DateRelativeUnitChoice> Units { get; }

    public double? Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }

    public DateRelativeUnitChoice SelectedUnit
    {
        get => _selectedUnit;
        set => SetProperty(ref _selectedUnit, value);
    }

    public void LoadFromDescriptor(FilteringDescriptor? descriptor)
    {
        if (descriptor == null)
        {
            ClearState();
            return;
        }

        var normalizedOperator = NormalizeDateOperator(descriptor.Operator);
        SelectedOperator = Operators.FirstOrDefault(x => x.Matches(descriptor) || x.Operator == normalizedOperator) ?? Operators[0];

        var threshold = ToDateTimeOffset(descriptor.Value);
        if (threshold == null)
        {
            ClearState();
            return;
        }

        var amountAndUnit = InferRelativeAmount(threshold.Value);
        Amount = amountAndUnit.Amount;
        SelectedUnit = amountAndUnit.Unit;
    }

    public void ClearState()
    {
        SelectedOperator = Operators[0];
        Amount = 1;
        SelectedUnit = Units[0];
    }

    private static FilteringOperator NormalizeDateOperator(FilteringOperator filterOperator)
    {
        return filterOperator switch
        {
            FilteringOperator.GreaterThanOrEqual => FilteringOperator.GreaterThan,
            FilteringOperator.LessThanOrEqual => FilteringOperator.LessThan,
            _ => filterOperator
        };
    }

    private static DateTimeOffset? ToDateTimeOffset(object? value)
    {
        return value switch
        {
            null => null,
            DateTimeOffset dto => dto,
            DateTime dt => new DateTimeOffset(dt),
            _ => null
        };
    }

    private static RelativeAmount InferRelativeAmount(DateTimeOffset threshold)
    {
        TimeSpan difference = DateTimeOffset.UtcNow - threshold;
        if (difference < TimeSpan.Zero)
        {
            difference = difference.Negate();
        }

        if (difference.TotalDays >= 30)
        {
            return new RelativeAmount(Math.Max(1, Math.Round(difference.TotalDays / 30d)), ResourceListFilterFlyoutOptions.DateRelativeUnits[3]);
        }

        if (difference.TotalDays >= 1)
        {
            return new RelativeAmount(Math.Max(1, Math.Round(difference.TotalDays)), ResourceListFilterFlyoutOptions.DateRelativeUnits[2]);
        }

        if (difference.TotalHours >= 1)
        {
            return new RelativeAmount(Math.Max(1, Math.Round(difference.TotalHours)), ResourceListFilterFlyoutOptions.DateRelativeUnits[1]);
        }

        return new RelativeAmount(Math.Max(1, Math.Round(difference.TotalMinutes)), ResourceListFilterFlyoutOptions.DateRelativeUnits[0]);
    }

    private readonly record struct RelativeAmount(double Amount, DateRelativeUnitChoice Unit);
}

internal static class ResourceListFilterFlyoutOptions
{
    public static IReadOnlyList<FilterOperatorChoice> TextOperators { get; } =
    [
        new(FilteringOperator.Contains, Assets.Resources.DataGridFilterFlyout_Contains),
        new(FilteringOperator.Custom, Assets.Resources.DataGridFilterFlyout_NotContains, FilterOperatorKeys.TextNotContains),
        new(FilteringOperator.Equals, Assets.Resources.DataGridFilterFlyout_Equals),
        new(FilteringOperator.NotEquals, Assets.Resources.DataGridFilterFlyout_NotEquals),
        new(FilteringOperator.StartsWith, Assets.Resources.DataGridFilterFlyout_StartsWith),
        new(FilteringOperator.Custom, Assets.Resources.DataGridFilterFlyout_NotStartsWith, FilterOperatorKeys.TextNotStartsWith),
        new(FilteringOperator.EndsWith, Assets.Resources.DataGridFilterFlyout_EndsWith),
        new(FilteringOperator.Custom, Assets.Resources.DataGridFilterFlyout_NotEndsWith, FilterOperatorKeys.TextNotEndsWith),
    ];

    public static IReadOnlyList<FilterOperatorChoice> NumericOperators { get; } =
    [
        new(FilteringOperator.Between, Assets.Resources.DataGridFilterFlyout_Between),
        new(FilteringOperator.Custom, Assets.Resources.DataGridFilterFlyout_NotBetween, FilterOperatorKeys.NumericNotBetween),
        new(FilteringOperator.Equals, Assets.Resources.DataGridFilterFlyout_Equals),
        new(FilteringOperator.NotEquals, Assets.Resources.DataGridFilterFlyout_NotEquals),
        new(FilteringOperator.GreaterThan, Assets.Resources.DataGridFilterFlyout_GreaterThan),
        new(FilteringOperator.Custom, Assets.Resources.DataGridFilterFlyout_NotGreaterThan, FilterOperatorKeys.NumericNotGreaterThan),
        new(FilteringOperator.GreaterThanOrEqual, Assets.Resources.DataGridFilterFlyout_GreaterThanOrEqual),
        new(FilteringOperator.Custom, Assets.Resources.DataGridFilterFlyout_NotGreaterThanOrEqual, FilterOperatorKeys.NumericNotGreaterThanOrEqual),
        new(FilteringOperator.LessThan, Assets.Resources.DataGridFilterFlyout_LessThan),
        new(FilteringOperator.Custom, Assets.Resources.DataGridFilterFlyout_NotLessThan, FilterOperatorKeys.NumericNotLessThan),
        new(FilteringOperator.LessThanOrEqual, Assets.Resources.DataGridFilterFlyout_LessThanOrEqual),
        new(FilteringOperator.Custom, Assets.Resources.DataGridFilterFlyout_NotLessThanOrEqual, FilterOperatorKeys.NumericNotLessThanOrEqual),
    ];

    public static IReadOnlyList<FilterOperatorChoice> DateOperators { get; } =
    [
        new(FilteringOperator.GreaterThan, Assets.Resources.DataGridFilterFlyout_NewerThan),
        new(FilteringOperator.Custom, Assets.Resources.DataGridFilterFlyout_NotNewerThan, FilterOperatorKeys.DateNotNewerThan),
        new(FilteringOperator.LessThan, Assets.Resources.DataGridFilterFlyout_OlderThan),
        new(FilteringOperator.Custom, Assets.Resources.DataGridFilterFlyout_NotOlderThan, FilterOperatorKeys.DateNotOlderThan),
    ];

    public static IReadOnlyList<FilterOperatorChoice> EnumOperators { get; } =
    [
        new(FilteringOperator.Equals, Assets.Resources.DataGridFilterFlyout_Equals),
        new(FilteringOperator.NotEquals, Assets.Resources.DataGridFilterFlyout_NotEquals),
    ];

    public static IReadOnlyList<DateRelativeUnitChoice> DateRelativeUnits { get; } =
    [
        new(DateRelativeUnit.Minutes, Assets.Resources.DataGridFilterFlyout_Minutes),
        new(DateRelativeUnit.Hours, Assets.Resources.DataGridFilterFlyout_Hours),
        new(DateRelativeUnit.Days, Assets.Resources.DataGridFilterFlyout_Days),
        new(DateRelativeUnit.Months, Assets.Resources.DataGridFilterFlyout_Months),
    ];
}

internal static class FilterOperatorKeys
{
    public const string TextNotContains = "__text_not_contains__";
    public const string TextNotStartsWith = "__text_not_starts_with__";
    public const string TextNotEndsWith = "__text_not_ends_with__";
    public const string NumericNotBetween = "__numeric_not_between__";
    public const string NumericNotGreaterThan = "__numeric_not_greater_than__";
    public const string NumericNotGreaterThanOrEqual = "__numeric_not_greater_than_or_equal__";
    public const string NumericNotLessThan = "__numeric_not_less_than__";
    public const string NumericNotLessThanOrEqual = "__numeric_not_less_than_or_equal__";
    public const string DateNotNewerThan = "__date_not_newer_than__";
    public const string DateNotOlderThan = "__date_not_older_than__";
}
