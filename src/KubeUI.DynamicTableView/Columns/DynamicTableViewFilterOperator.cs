namespace KubeUI.DynamicTableView;

/// <summary>Filter operators supported by built-in and custom filters.</summary>
public enum DynamicTableViewFilterOperator
{
    Contains, DoesNotContain, Equals, NotEquals, StartsWith, DoesNotStartWith, EndsWith, DoesNotEndWith,
    GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, Between, NotBetween, In, IsTrue, IsFalse, IsNull, IsNotNull
}
