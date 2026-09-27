using DynamicData;
using KubeUI.DynamicTableView.Tests.Fixtures;

namespace KubeUI.DynamicTableView.Tests.Unit;

public sealed class DynamicTableViewSourceFilteringTests
{
    [Theory]
    [InlineData(DynamicTableViewFilterOperator.Contains, "ph", "a")]
    [InlineData(DynamicTableViewFilterOperator.DoesNotContain, "pha", "b,c")]
    [InlineData(DynamicTableViewFilterOperator.StartsWith, "Al", "a")]
    [InlineData(DynamicTableViewFilterOperator.DoesNotStartWith, "Al", "b,c")]
    [InlineData(DynamicTableViewFilterOperator.EndsWith, "ta", "b")]
    [InlineData(DynamicTableViewFilterOperator.DoesNotEndWith, "ta", "a,c")]
    [InlineData(DynamicTableViewFilterOperator.Equals, "Beta", "b")]
    [InlineData(DynamicTableViewFilterOperator.NotEquals, "Beta", "a,c")]
    public void Text_filter_operators_match_expected_rows(DynamicTableViewFilterOperator operation, string query, string expectedId)
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());

        source.SetFilter(new("name", operation, query));

        Assert.Equal(expectedId.Split(','), source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
    }

    [Fact]
    public void String_equality_uses_descriptor_comparison()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());

        source.SetFilter(new("name", DynamicTableViewFilterOperator.Equals, "alpha", StringComparison: StringComparison.Ordinal));
        Assert.Empty(source.Items);

        source.SetFilter(new("name", DynamicTableViewFilterOperator.Equals, "alpha", StringComparison: StringComparison.OrdinalIgnoreCase));
        Assert.Equal(["a"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
    }

    [Fact]
    public void Ordinal_string_inequality_keeps_case_distinct_values()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());

        source.SetFilter(new("name", DynamicTableViewFilterOperator.NotEquals, "alpha", StringComparison: StringComparison.Ordinal));

        Assert.Equal(["a", "b", "c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
    }

    [Theory]
    [InlineData(DynamicTableViewFilterOperator.Equals, 20, "b")]
    [InlineData(DynamicTableViewFilterOperator.NotEquals, 20, "a,c")]
    [InlineData(DynamicTableViewFilterOperator.GreaterThan, 20, "c")]
    [InlineData(DynamicTableViewFilterOperator.GreaterThanOrEqual, 20, "b", "c")]
    [InlineData(DynamicTableViewFilterOperator.LessThan, 20, "a")]
    [InlineData(DynamicTableViewFilterOperator.LessThanOrEqual, 20, "a", "b")]
    [InlineData(DynamicTableViewFilterOperator.Between, 10, "a", "b")]
    [InlineData(DynamicTableViewFilterOperator.NotBetween, 10, "c")]
    public void Numeric_filter_operators_match_expected_rows(
        DynamicTableViewFilterOperator operation,
        int firstValue,
        string firstExpectedId,
        string? secondExpectedId = null)
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        object? secondValue = operation is DynamicTableViewFilterOperator.Between or DynamicTableViewFilterOperator.NotBetween ? 20 : null;

        source.SetFilter(new("age", operation, firstValue, secondValue));

        var expected = secondExpectedId is null ? firstExpectedId.Split(',') : new[] { firstExpectedId, secondExpectedId };
        Assert.Equal(expected, source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
    }

    [Fact]
    public void Date_boolean_enum_and_null_operators_are_supported()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());

        source.SetFilter(new("created", DynamicTableViewFilterOperator.GreaterThan, new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)));
        Assert.Equal(["b", "c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
        source.ClearFilters();
        source.SetFilter(new("enabled", DynamicTableViewFilterOperator.IsFalse));
        Assert.Equal(["b"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
        source.ClearFilters();
        source.SetFilter(new("enabled", DynamicTableViewFilterOperator.IsTrue));
        Assert.Equal(["a", "c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
        source.ClearFilters();
        source.SetFilter(new("state", DynamicTableViewFilterOperator.Equals, DynamicTableViewTestState.Failed));
        Assert.Equal(["c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
        source.SetFilter(new("state", DynamicTableViewFilterOperator.In, Values: [DynamicTableViewTestState.Ready, DynamicTableViewTestState.Failed]));
        Assert.Equal(["a", "c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
        source.ClearFilters();
        source.SetFilter(new("name", DynamicTableViewFilterOperator.IsNull));
        Assert.Empty(source.Items);
        source.SetFilter(new("name", DynamicTableViewFilterOperator.IsNotNull));
        Assert.Equal(3, source.Items.Cast<DynamicTableViewTestRow>().Count());
    }

    [Fact]
    public void Column_filter_scope_filter_and_search_compose_and_clear_independently()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        source.SetScopeFilter("scope", static item => ((DynamicTableViewTestRow)item).Age >= 20);
        source.SetFilter(new("enabled", DynamicTableViewFilterOperator.Equals, false));
        source.SearchText = "a";

        Assert.Equal(["b"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
        source.ClearFilters();

        Assert.Equal(["b", "c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
    }

    [Fact]
    public void Clearing_column_filters_keeps_scope_filter_and_search_combinations_independent()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        source.SetScopeFilter("namespace", static item => ((DynamicTableViewTestRow)item).Age >= 20);
        source.SetFilter(new("enabled", DynamicTableViewFilterOperator.IsFalse));
        source.SearchText = "a";

        Assert.Equal(["b"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));

        source.ClearFilters();

        Assert.Equal(["b", "c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
        source.SearchText = string.Empty;
        Assert.Equal(["b", "c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
    }

    [Fact]
    public void Custom_column_predicate_composes_with_typed_and_scope_filters()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        source.SetScopeFilter("visible", static item => ((DynamicTableViewTestRow)item).Enabled);
        source.SetFilter(new("age", DynamicTableViewFilterOperator.GreaterThanOrEqual, 10));
        source.SetCustomFilter("name", static item => ((DynamicTableViewTestRow)item).Name.EndsWith('a'));

        Assert.Equal(["a", "c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
    }

    [Fact]
    public void Enum_in_filter_matches_each_requested_choice()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());

        source.SetFilter(new("state", DynamicTableViewFilterOperator.In, Values: [DynamicTableViewTestState.Ready, DynamicTableViewTestState.Failed]));

        Assert.Equal(["a", "c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
    }

    [Fact]
    public void Replacing_filter_on_same_column_reapplies_predicate_to_excluded_rows()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());

        source.SetFilter(new("state", DynamicTableViewFilterOperator.Equals, DynamicTableViewTestState.Failed));
        Assert.Equal(["c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));

        source.SetFilter(new("state", DynamicTableViewFilterOperator.In, Values: [DynamicTableViewTestState.Ready, DynamicTableViewTestState.Failed]));

        Assert.Equal(["a", "c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
    }
}
