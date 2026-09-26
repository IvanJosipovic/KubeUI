using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSorting;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;
using DynamicData;
using DynamicData.Binding;
using k8s;
using k8s.Models;

namespace KubeUI.Avalonia.Infrastructure.DataGrid;

public class DynamicDataSortingAdapterFactory<T> : IDataGridSortingAdapterFactory where T : class
{
    private readonly IReadOnlyDictionary<string, IResourceListColumn> _resourceColumnsByKey;

    private static readonly IComparer<T> s_noopComparer = Comparer<T>.Create(static (_, _) => 0);
    private static readonly IComparable s_nullSortValue = new NullSortValue();

    public IComparer<T> SortComparer { get; private set; }

    public DynamicDataSortingAdapterFactory(IReadOnlyDictionary<string, IResourceListColumn> resourceColumnsByKey)
    {
        SortComparer = s_noopComparer;
        _resourceColumnsByKey = resourceColumnsByKey;
    }

    public DataGridSortingAdapter Create(global::Avalonia.Controls.DataGrid grid, ISortingModel model)
    {
        return new DynamicDataSortingAdapter(model, () => grid.Columns, UpdateComparer);
    }

    public void UpdateComparer(IReadOnlyList<SortingDescriptor> descriptors)
    {
        SortComparer = BuildComparer(descriptors);
    }

    private IComparer<T> BuildComparer(IReadOnlyList<SortingDescriptor> descriptors)
    {
        if (descriptors == null || descriptors.Count == 0)
        {
            return s_noopComparer;
        }

        SortExpressionComparer<T>? comparer = null;

        for (var i = 0; i < descriptors.Count; i++)
        {
            var descriptor = descriptors[i];
            if (descriptor == null)
            {
                continue;
            }

            var selector = CreateSelector(descriptor);

            if (selector == null)
            {
                continue;
            }

            IComparable SafeSelect(T item)
            {
                try
                {
                    return selector(item) ?? s_nullSortValue;
                }
                catch
                {
                    return s_nullSortValue;
                }
            }

            comparer = comparer == null
                ? descriptor.Direction == ListSortDirection.Ascending
                    ? SortExpressionComparer<T>.Ascending(SafeSelect)
                    : SortExpressionComparer<T>.Descending(SafeSelect)
                : descriptor.Direction == ListSortDirection.Ascending
                    ? comparer.ThenByAscending(SafeSelect)
                    : comparer.ThenByDescending(SafeSelect);
        }

        return comparer ?? s_noopComparer;
    }

    private Func<T, IComparable?>? CreateSelector(SortingDescriptor descriptor)
    {
        if (ResolveResourceColumn(descriptor.ColumnId) is not IResourceListColumn column)
        {
            return null;
        }

        return column.SortKey;
    }

    private IResourceListColumn? ResolveResourceColumn(object? columnId)
    {
        if (columnId is DataGridColumnDefinition definition)
        {
            if (definition.Tag is IResourceListColumn taggedColumn)
            {
                return taggedColumn;
            }

            if (TryResolveKey(definition.ColumnKey, out var key))
            {
                if (_resourceColumnsByKey.TryGetValue(key, out var resolvedColumn))
                {
                    return resolvedColumn;
                }
            }
        }

        if (TryResolveKey(columnId, out var directKey))
        {
            if (_resourceColumnsByKey.TryGetValue(directKey, out var directColumn))
            {
                return directColumn;
            }
        }

        return null;
    }

    private static bool TryResolveKey(object? columnId, out string key)
    {
        switch (columnId)
        {
            case null:
                key = string.Empty;
                return false;
            case string stringKey when !string.IsNullOrWhiteSpace(stringKey):
                key = stringKey;
                return true;
            case DataGridColumnDefinition definition when definition.ColumnKey is not null:
                key = definition.ColumnKey.ToString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(key);
            case DataGridColumn column when column.ColumnKey is not null:
                key = column.ColumnKey.ToString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(key);
            default:
                key = string.Empty;
                return false;
        }
    }

    private sealed class DynamicDataSortingAdapter : DataGridSortingAdapter
    {
        private readonly Action<IReadOnlyList<SortingDescriptor>> _update;

        public DynamicDataSortingAdapter(
            ISortingModel model,
            Func<IEnumerable<DataGridColumn>> columns,
            Action<IReadOnlyList<SortingDescriptor>> update)
            : base(model, columns)
        {
            _update = update;
        }

        protected override bool TryApplyModelToView(
            IReadOnlyList<SortingDescriptor> descriptors,
            IReadOnlyList<SortingDescriptor> previousDescriptors,
            out bool changed)
        {
            _update(descriptors);
            changed = true;
            return true;
        }
    }

    private sealed class NullSortValue : IComparable
    {
        public int CompareTo(object? obj)
        {
            return obj is NullSortValue ? 0 : -1;
        }
    }
}

public class DynamicDataFilteringAdapterFactory<T> : IDataGridFilteringAdapterFactory where T : class
{
    private static readonly Func<T, bool> s_alwaysTrue = static _ => true;

    private readonly IReadOnlyDictionary<string, IResourceListColumn> _resourceColumnsByKey;

    public DynamicDataFilteringAdapterFactory(IReadOnlyDictionary<string, IResourceListColumn> resourceColumnsByKey)
    {
        FilterPredicate = s_alwaysTrue;
        _resourceColumnsByKey = resourceColumnsByKey;
    }

    public DataGridFilteringAdapter Create(global::Avalonia.Controls.DataGrid grid, IFilteringModel model)
    {
        return new DynamicDataFilteringAdapter(model, () => grid.Columns, UpdateFilter);
    }

    public Func<T, bool> FilterPredicate { get; private set; }

    public void UpdateFilter(IReadOnlyList<FilteringDescriptor> descriptors)
    {
        FilterPredicate = BuildPredicate(descriptors);
    }

    private Func<T, bool> BuildPredicate(IReadOnlyList<FilteringDescriptor> descriptors)
    {
        if (descriptors == null || descriptors.Count == 0)
        {
            return s_alwaysTrue;
        }

        var compiled = new List<Func<T, bool>>(descriptors.Count);
        for (var i = 0; i < descriptors.Count; i++)
        {
            var descriptor = descriptors[i];
            var predicate = Compile(descriptor);
            if (predicate != null)
            {
                compiled.Add(predicate);
            }
        }

        if (compiled.Count == 0)
        {
            return s_alwaysTrue;
        }

        if (compiled.Count == 1)
        {
            return compiled[0];
        }

        return item =>
        {
            for (var i = 0; i < compiled.Count; i++)
            {
                if (!compiled[i](item))
                {
                    return false;
                }
            }

            return true;
        };
    }

    private Func<T, bool>? Compile(FilteringDescriptor descriptor)
    {
        if (descriptor == null)
        {
            return null;
        }

        if (descriptor.Predicate != null)
        {
            var predicate = descriptor.Predicate;
            return item =>
            {
                try
                {
                    return predicate(item);
                }
                catch
                {
                    return false;
                }
            };
        }

        var selector = CreateSelector(descriptor);
        if (selector == null)
        {
            return null;
        }

        object? SafeSelect(T item)
        {
            try
            {
                return selector(item);
            }
            catch
            {
                return null;
            }
        }

        var culture = descriptor.Culture ?? CultureInfo.InvariantCulture;
        var stringComparison = descriptor.StringComparisonMode ?? StringComparison.OrdinalIgnoreCase;
        var values = descriptor.Values;
        var value = descriptor.Value;

        return descriptor.Operator switch
        {
            FilteringOperator.Equals => item => Equals(SafeSelect(item), value),
            FilteringOperator.NotEquals => item => !Equals(SafeSelect(item), value),
            FilteringOperator.Contains => item => Contains(SafeSelect(item), value, stringComparison),
            FilteringOperator.StartsWith => item => StartsWith(SafeSelect(item), value, stringComparison),
            FilteringOperator.EndsWith => item => EndsWith(SafeSelect(item), value, stringComparison),
            FilteringOperator.GreaterThan => item => Compare(SafeSelect(item), value, culture) > 0,
            FilteringOperator.GreaterThanOrEqual => item => Compare(SafeSelect(item), value, culture) >= 0,
            FilteringOperator.LessThan => item => Compare(SafeSelect(item), value, culture) < 0,
            FilteringOperator.LessThanOrEqual => item => Compare(SafeSelect(item), value, culture) <= 0,
            FilteringOperator.Between => item => Between(SafeSelect(item), values, culture),
            FilteringOperator.In => item => In(SafeSelect(item), values),
            _ => s_alwaysTrue
        };
    }

    private Func<T, object?>? CreateSelector(FilteringDescriptor descriptor)
    {
        if (string.Equals(descriptor.PropertyPath, "namespace_scope", StringComparison.Ordinal))
        {
            return item =>
            {
                try
                {
                    return item is IKubernetesObject<V1ObjectMeta> resource ? resource.Namespace() : null;
                }
                catch
                {
                    return null;
                }
            };
        }

        if (ResolveResourceColumn(descriptor.ColumnId) is not IResourceListColumn column)
        {
            return null;
        }

        return item =>
        {
            try
            {
                return column.ValueAccessor.GetValue(item!);
            }
            catch
            {
                return null;
            }
        };
    }

    private IResourceListColumn? ResolveResourceColumn(object? columnId)
    {
        if (columnId is DataGridColumnDefinition definition)
        {
            if (definition.Tag is IResourceListColumn taggedColumn)
            {
                return taggedColumn;
            }

            if (TryResolveKey(definition.ColumnKey, out var key))
            {
                if (_resourceColumnsByKey.TryGetValue(key, out var resolvedColumn))
                {
                    return resolvedColumn;
                }
            }
        }

        if (TryResolveKey(columnId, out var directKey))
        {
            if (_resourceColumnsByKey.TryGetValue(directKey, out var directColumn))
            {
                return directColumn;
            }
        }

        return null;
    }

    private static bool TryResolveKey(object? columnId, out string key)
    {
        switch (columnId)
        {
            case null:
                key = string.Empty;
                return false;
            case string stringKey when !string.IsNullOrWhiteSpace(stringKey):
                key = stringKey;
                return true;
            case DataGridColumnDefinition definition when definition.ColumnKey is not null:
                key = definition.ColumnKey.ToString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(key);
            case DataGridColumn column when column.ColumnKey is not null:
                key = column.ColumnKey.ToString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(key);
            default:
                key = string.Empty;
                return false;
        }
    }

    private static bool Contains(object? source, object? target, StringComparison comparison)
    {
        if (source == null || target == null)
        {
            return false;
        }

        if (source is string s && target is string t)
        {
            return s.Contains(t, comparison);
        }

        if (source is IEnumerable<object> enumerable)
        {
            foreach (var item in enumerable)
            {
                if (Equals(item, target))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool StartsWith(object? source, object? target, StringComparison comparison)
    {
        return source is string s && target is string t && s.StartsWith(t, comparison);
    }

    private static bool EndsWith(object? source, object? target, StringComparison comparison)
    {
        return source is string s && target is string t && s.EndsWith(t, comparison);
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
            return comparable.CompareTo(ChangeType(right, left.GetType(), culture));
        }

        return Comparer<object>.Default.Compare(left, right);
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

    private static bool Between(object? source, IReadOnlyList<object?>? values, CultureInfo culture)
    {
        if (values == null || values.Count < 2)
        {
            return false;
        }

        return Compare(source, values[0], culture) >= 0 && Compare(source, values[1], culture) <= 0;
    }

    private static bool In(object? source, IReadOnlyList<object?>? values)
    {
        if (values == null || values.Count == 0)
        {
            return false;
        }

        foreach (var candidate in values)
        {
            if (Equals(source, candidate))
            {
                return true;
            }
        }

        return false;
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
        catch (Exception)
        {
            return value;
        }
    }

    private sealed class DynamicDataFilteringAdapter : DataGridFilteringAdapter
    {
        private readonly Action<IReadOnlyList<FilteringDescriptor>> _update;

        public DynamicDataFilteringAdapter(
            IFilteringModel model,
            Func<IEnumerable<DataGridColumn>> columns,
            Action<IReadOnlyList<FilteringDescriptor>> update)
            : base(model, columns)
        {
            _update = update;
        }

        protected override bool TryApplyModelToView(
            IReadOnlyList<FilteringDescriptor> descriptors,
            IReadOnlyList<FilteringDescriptor> previousDescriptors,
            out bool changed)
        {
            _update(descriptors);
            changed = true;
            return true;
        }
    }
}

/// <summary>
/// Adapter factory that translates SearchModel descriptors into a DynamicData search predicate.
/// It can push search criteria upstream while letting the grid compute match highlighting.
/// </summary>
public class DynamicDataSearchAdapterFactory<T> : IDataGridSearchAdapterFactory where T : class
{
    private readonly ColumnSelector[] _allColumns;
    private readonly Dictionary<string, ColumnSelector> _columnsByKey;

    public DynamicDataSearchAdapterFactory(IReadOnlyDictionary<string, IResourceListColumn> resourceColumnsByKey)
    {
        _allColumns = new ColumnSelector[resourceColumnsByKey.Count];
        _columnsByKey = new Dictionary<string, ColumnSelector>(resourceColumnsByKey.Count, StringComparer.OrdinalIgnoreCase);

        var index = 0;
        foreach (var pair in resourceColumnsByKey)
        {
            var selector = new ColumnSelector(pair.Key, pair.Value.DisplayValue);
            _allColumns[index++] = selector;
            _columnsByKey[pair.Key] = selector;
        }

        SearchPredicate = static _ => true;
    }

    public Func<T, bool> SearchPredicate { get; private set; }

    public DataGridSearchAdapter Create(global::Avalonia.Controls.DataGrid grid, ISearchModel model)
    {
        return new DynamicDataSearchAdapter(model, () => grid.ColumnDefinitions, UpdatePredicate);
    }

    public void UpdatePredicate(IReadOnlyList<SearchDescriptor> descriptors)
    {
        SearchPredicate = BuildPredicate(descriptors);
    }

    private Func<T, bool> BuildPredicate(IReadOnlyList<SearchDescriptor> descriptors)
    {
        if (descriptors == null || descriptors.Count == 0)
        {
            return static _ => true;
        }

        var compiled = new List<Func<T, bool>>();
        foreach (var descriptor in descriptors)
        {
            var predicate = Compile(descriptor);
            if (predicate != null)
            {
                compiled.Add(predicate);
            }
        }

        if (compiled.Count == 0)
        {
            return static _ => true;
        }

        return item =>
        {
            for (var i = 0; i < compiled.Count; i++)
            {
                if (compiled[i](item))
                {
                    return true;
                }
            }

            return false;
        };
    }

    private Func<T, bool>? Compile(SearchDescriptor descriptor)
    {
        if (descriptor == null)
        {
            return null;
        }

        var columns = SelectColumns(descriptor);
        if (columns.Length == 0)
        {
            return null;
        }

        return item =>
        {
            for (var i = 0; i < columns.Length; i++)
            {
                string? text;
                try
                {
                    text = columns[i].Getter(item);
                }
                catch
                {
                    continue;
                }

                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                if (TextMatcher.HasMatch(text, descriptor))
                {
                    return true;
                }
            }

            return false;
        };
    }

    private ColumnSelector[] SelectColumns(SearchDescriptor descriptor)
    {
        if (descriptor.Scope != SearchScope.ExplicitColumns)
        {
            return _allColumns;
        }

        if (descriptor.ColumnIds == null || descriptor.ColumnIds.Count == 0)
        {
            return [];
        }

        var selected = new List<ColumnSelector>(descriptor.ColumnIds.Count);
        for (var i = 0; i < descriptor.ColumnIds.Count; i++)
        {
            var id = descriptor.ColumnIds[i];
            if (!TryResolveColumnKey(id, out var key))
            {
                continue;
            }

            if (_columnsByKey.TryGetValue(key, out var selector))
            {
                selected.Add(selector);
            }
        }

        return selected.Count == 0 ? [] : selected.ToArray();
    }

    private static bool TryResolveColumnKey(object? columnId, out string key)
    {
        switch (columnId)
        {
            case null:
                key = string.Empty;
                return false;
            case string stringKey when !string.IsNullOrWhiteSpace(stringKey):
                key = stringKey;
                return true;
            case DataGridColumnDefinition definition when definition.ColumnKey is not null:
                key = definition.ColumnKey.ToString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(key);
            case DataGridColumn column when column.ColumnKey is not null:
                key = column.ColumnKey.ToString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(key);
            default:
                key = string.Empty;
                return false;
        }
    }

    private sealed class DynamicDataSearchAdapter : DataGridSearchAdapter
    {
        private readonly Action<IReadOnlyList<SearchDescriptor>> _update;

        public DynamicDataSearchAdapter(
            ISearchModel model,
            Func<IEnumerable<DataGridColumn>> columns,
            Action<IReadOnlyList<SearchDescriptor>> update
            )
            : base(model, columns)
        {
            _update = update;
        }

        protected override bool TryApplyModelToView(
            IReadOnlyList<SearchDescriptor> descriptors,
            IReadOnlyList<SearchDescriptor> previousDescriptors,
            out IReadOnlyList<SearchResult> results)
        {
            _update(descriptors);
            results = [];
            return false;
        }
    }

    private sealed class ColumnSelector
    {
        public ColumnSelector(string id, Func<T, string> getter)
        {
            Id = id;
            Getter = getter;
        }

        public string Id { get; }

        public Func<T, string> Getter { get; }
    }

    private static class TextMatcher
    {
        public static bool HasMatch(string text, SearchDescriptor descriptor)
        {
            if (descriptor == null || string.IsNullOrEmpty(text))
            {
                return false;
            }

            if (string.IsNullOrEmpty(descriptor.Query))
            {
                return descriptor.AllowEmpty && text.Length > 0;
            }

            var normalizedText = NormalizeText(text, descriptor.NormalizeWhitespace, descriptor.IgnoreDiacritics);
            var query = NormalizeQuery(descriptor.Query, descriptor.NormalizeWhitespace, descriptor.IgnoreDiacritics);

            if (descriptor.MatchMode == SearchMatchMode.Regex || descriptor.MatchMode == SearchMatchMode.Wildcard)
            {
                var pattern = descriptor.MatchMode == SearchMatchMode.Wildcard
                    ? WildcardToRegex(query)
                    : query;

                if (descriptor.WholeWord)
                {
                    pattern = $@"\b(?:{pattern})\b";
                }

                var options = RegexOptions.Compiled;
                if (IsIgnoreCase(descriptor.Comparison))
                {
                    options |= RegexOptions.IgnoreCase;
                }

                if (IsCultureInvariant(descriptor.Comparison))
                {
                    options |= RegexOptions.CultureInvariant;
                }

                try
                {
                    return Regex.IsMatch(normalizedText, pattern, options);
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }

            var terms = Tokenize(query);
            if (terms.Count == 0)
            {
                return false;
            }

            var comparison = descriptor.Comparison ?? StringComparison.OrdinalIgnoreCase;
            var any = descriptor.TermMode == SearchTermCombineMode.Any;

            foreach (var term in terms)
            {
                if (string.IsNullOrEmpty(term))
                {
                    continue;
                }

                var matched = FindTermMatch(normalizedText, term, descriptor.MatchMode, comparison, descriptor.WholeWord);
                if (matched && any)
                {
                    return true;
                }

                if (!matched && !any)
                {
                    return false;
                }
            }

            return !any;
        }

        private static bool FindTermMatch(
            string text,
            string term,
            SearchMatchMode mode,
            StringComparison comparison,
            bool wholeWord)
        {
            switch (mode)
            {
                case SearchMatchMode.StartsWith:
                    return text.StartsWith(term, comparison) && IsWholeWord(text, 0, term.Length, wholeWord);
                case SearchMatchMode.EndsWith:
                    if (!text.EndsWith(term, comparison))
                    {
                        return false;
                    }

                    var start = text.Length - term.Length;
                    return IsWholeWord(text, start, term.Length, wholeWord);
                case SearchMatchMode.Equals:
                    return string.Equals(text, term, comparison) && IsWholeWord(text, 0, term.Length, wholeWord);
                default:
                    return FindAllOccurrences(text, term, comparison, wholeWord);
            }
        }

        private static bool FindAllOccurrences(
            string text,
            string term,
            StringComparison comparison,
            bool wholeWord)
        {
            if (string.IsNullOrEmpty(term))
            {
                return false;
            }

            var startIndex = 0;
            while (startIndex < text.Length)
            {
                var index = text.IndexOf(term, startIndex, comparison);
                if (index < 0)
                {
                    break;
                }

                if (IsWholeWord(text, index, term.Length, wholeWord))
                {
                    return true;
                }

                startIndex = index + term.Length;
            }

            return false;
        }

        private static bool IsWholeWord(string text, int start, int length, bool wholeWord)
        {
            if (!wholeWord)
            {
                return true;
            }

            var startBoundary = start == 0 || !IsWordChar(text[start - 1]);
            var endIndex = start + length;
            var endBoundary = endIndex >= text.Length || !IsWordChar(text[endIndex]);

            return startBoundary && endBoundary;
        }

        private static bool IsWordChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private static List<string> Tokenize(string query)
        {
            var terms = new List<string>();
            if (string.IsNullOrWhiteSpace(query))
            {
                return terms;
            }

            var builder = new StringBuilder();
            var inQuote = false;

            foreach (var ch in query)
            {
                if (ch == '"')
                {
                    inQuote = !inQuote;
                    continue;
                }

                if (!inQuote && char.IsWhiteSpace(ch))
                {
                    Flush(builder, terms);
                    continue;
                }

                builder.Append(ch);
            }

            Flush(builder, terms);
            return terms;
        }

        private static void Flush(StringBuilder builder, List<string> terms)
        {
            if (builder.Length == 0)
            {
                return;
            }

            var term = builder.ToString().Trim();
            if (!string.IsNullOrEmpty(term))
            {
                terms.Add(term);
            }

            builder.Clear();
        }

        private static string NormalizeText(string text, bool normalizeWhitespace, bool ignoreDiacritics)
        {
            if (!normalizeWhitespace && !ignoreDiacritics)
            {
                return text;
            }

            var chars = new List<char>();
            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (ignoreDiacritics)
                {
                    foreach (var d in ch.ToString().Normalize(NormalizationForm.FormD))
                    {
                        if (IsDiacritic(d))
                        {
                            continue;
                        }

                        chars.Add(d);
                    }
                }
                else
                {
                    chars.Add(ch);
                }
            }

            if (!normalizeWhitespace)
            {
                return new string(chars.ToArray());
            }

            var builder = new StringBuilder();
            var wasWhitespace = false;

            for (var i = 0; i < chars.Count; i++)
            {
                var ch = chars[i];
                var isWhitespace = char.IsWhiteSpace(ch);
                if (isWhitespace)
                {
                    if (wasWhitespace)
                    {
                        continue;
                    }

                    builder.Append(' ');
                    wasWhitespace = true;
                }
                else
                {
                    builder.Append(ch);
                    wasWhitespace = false;
                }
            }

            return builder.ToString();
        }

        private static string NormalizeQuery(string query, bool normalizeWhitespace, bool ignoreDiacritics)
        {
            if (!normalizeWhitespace && !ignoreDiacritics)
            {
                return query;
            }

            var normalized = NormalizeText(query, normalizeWhitespace, ignoreDiacritics);
            return normalizeWhitespace ? normalized.Trim() : normalized;
        }

        private static bool IsDiacritic(char ch)
        {
            return CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark;
        }

        private static string WildcardToRegex(string pattern)
        {
            var builder = new StringBuilder();
            foreach (var ch in pattern)
            {
                switch (ch)
                {
                    case '*':
                        builder.Append(".*");
                        break;
                    case '?':
                        builder.Append(".");
                        break;
                    default:
                        builder.Append(Regex.Escape(ch.ToString()));
                        break;
                }
            }

            return builder.ToString();
        }

        private static bool IsIgnoreCase(StringComparison? comparison)
        {
            if (!comparison.HasValue)
            {
                return true;
            }

            switch (comparison.Value)
            {
                case StringComparison.CurrentCultureIgnoreCase:
                case StringComparison.InvariantCultureIgnoreCase:
                case StringComparison.OrdinalIgnoreCase:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsCultureInvariant(StringComparison? comparison)
        {
            if (!comparison.HasValue)
            {
                return true;
            }

            switch (comparison.Value)
            {
                case StringComparison.Ordinal:
                case StringComparison.OrdinalIgnoreCase:
                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    return true;
                default:
                    return false;
            }
        }
    }
}

