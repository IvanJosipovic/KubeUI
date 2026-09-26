using System.Resources;

namespace KubeUI.DynamicTableView;

/// <summary>Localized text resources for DynamicTableView controls.</summary>
public static class DynamicTableViewResources
{
    private static readonly ResourceManager s_resources = new(
        "KubeUI.DynamicTableView.DynamicTableViewResources",
        typeof(DynamicTableViewResources).Assembly);

    /// <summary>Gets or sets the resource culture.</summary>
    public static CultureInfo? Culture { get; set; }

    /// <summary>Gets the filter value prompt.</summary>
    public static string FilterValue => GetString(nameof(FilterValue));

    /// <summary>Gets the second filter value prompt.</summary>
    public static string FilterSecondValue => GetString(nameof(FilterSecondValue));

    /// <summary>Gets the apply filter label.</summary>
    public static string FilterApply => GetString(nameof(FilterApply));

    /// <summary>Gets the clear filter label.</summary>
    public static string FilterClear => GetString(nameof(FilterClear));

    /// <summary>Gets accessible text for filter options.</summary>
    public static string FilterAccessibleName => GetString(nameof(FilterAccessibleName));

    /// <summary>Gets accessible text for sort.</summary>
    public static string SortAccessibleName => GetString(nameof(SortAccessibleName));

    /// <summary>Gets the false value label.</summary>
    public static string FilterFalse => GetString(nameof(FilterFalse));

    /// <summary>Gets the true value label.</summary>
    public static string FilterTrue => GetString(nameof(FilterTrue));

    /// <summary>Gets a localized label for one filter operator.</summary>
    public static string GetFilterOperatorLabel(DynamicTableViewFilterOperator filterOperator)
        => GetString($"FilterOperator_{filterOperator}");

    private static string GetString(string key)
        => s_resources.GetString(key, Culture) ?? key;
}
