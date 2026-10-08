namespace KubeUI.Kubernetes;

/// <summary>One parsed Crossplane managed-resource diff.</summary>
public sealed record CrossplaneDiffRecord(
    string Uid,
    string Name,
    string Namespace,
    string ApiVersion,
    string Kind,
    string DiffField,
    string OldValue,
    string NewValue,
    bool NewComputed,
    bool NewRemoved,
    bool RequiresNew,
    bool Sensitive = false);

internal sealed class CrossplaneDiffInstanceGroup
{
    public int Count { get; set; }
}

/// <summary>Aggregated row shown by the MR Diff Detection view.</summary>
public sealed class CrossplaneDiffRow
{
    private string _oldValue;
    private string _newValue;
    private bool _newComputed;
    private bool _newRemoved;
    private bool _requiresNew;
    private int _occurrences;
    private readonly CrossplaneDiffInstanceGroup _instanceGroup;

    public CrossplaneDiffRow(CrossplaneDiffRecord record)
        : this(record, new CrossplaneDiffInstanceGroup { Count = 1 })
    {
    }

    internal CrossplaneDiffRow(CrossplaneDiffRecord record, CrossplaneDiffInstanceGroup instanceGroup)
    {
        ArgumentNullException.ThrowIfNull(record);
        _instanceGroup = instanceGroup;
        Uid = record.Uid;
        Name = record.Name;
        Namespace = record.Namespace;
        ApiVersion = record.ApiVersion;
        Kind = record.Kind;
        DiffField = record.DiffField;
        _oldValue = record.Sensitive ? string.Empty : record.OldValue;
        _newValue = record.Sensitive ? string.Empty : record.NewValue;
        _newComputed = record.NewComputed;
        _newRemoved = record.NewRemoved;
        _requiresNew = record.RequiresNew;
        Sensitive = record.Sensitive;
        _occurrences = 1;
    }

    public string Uid { get; }
    public string Name { get; }
    public string Namespace { get; }
    public string ApiVersion { get; }
    public string Kind { get; }
    public string DiffField { get; }
    public string Key => string.Join("\u0000", Uid, Name, Namespace, ApiVersion, Kind, DiffField);
    public string OldValue => _oldValue;
    public string NewValue => _newValue;
    public bool NewComputed => _newComputed;
    public bool NewRemoved => _newRemoved;
    public bool RequiresNew => _requiresNew;
    public bool Sensitive { get; private set; }
    public int Occurrences => _occurrences;
    public int InstanceCount => _instanceGroup.Count;

    internal void Update(CrossplaneDiffRecord record)
    {
        Sensitive |= record.Sensitive;
        _oldValue = Sensitive ? string.Empty : record.OldValue;
        _newValue = Sensitive ? string.Empty : record.NewValue;
        _newComputed = record.NewComputed;
        _newRemoved = record.NewRemoved;
        _requiresNew = record.RequiresNew;
        _occurrences++;
    }

    internal CrossplaneDiffRow Snapshot()
    {
        CrossplaneDiffRow copy = new(new CrossplaneDiffRecord(
            Uid, Name, Namespace, ApiVersion, Kind, DiffField,
            OldValue, NewValue, NewComputed, NewRemoved, RequiresNew, Sensitive), _instanceGroup);
        copy._occurrences = Occurrences;
        return copy;
    }

}

/// <summary>Aggregates repeated diff events without retaining raw log text.</summary>
public sealed class CrossplaneDiffAggregator
{
    private sealed class GroupRows(CrossplaneDiffInstanceGroup instanceGroup)
    {
        public CrossplaneDiffInstanceGroup InstanceGroup { get; } = instanceGroup;
        public List<CrossplaneDiffRow> Rows { get; } = [];
    }

    private readonly Dictionary<string, CrossplaneDiffRow> _rows = new(StringComparer.Ordinal);
    private readonly Dictionary<(string ApiVersion, string Kind, string DiffField), GroupRows> _rowsByGroup = [];
    private readonly int _maximumRowCount;

    /// <summary>Creates a diff aggregator with an optional maximum number of retained rows.</summary>
    /// <param name="maximumRowCount">The maximum number of distinct resource-field rows to retain.</param>
    public CrossplaneDiffAggregator(int maximumRowCount = int.MaxValue)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maximumRowCount, 0);
        _maximumRowCount = maximumRowCount;
    }

    public IReadOnlyCollection<CrossplaneDiffRow> Rows => _rows.Values;

    /// <summary>Adds or updates a resource-field row, respecting the configured retention limit.</summary>
    /// <returns>
    /// <see langword="true"/> when a new row was retained; <see langword="false"/> when an existing row was
    /// updated or a new row could not be retained because the configured limit was reached.
    /// </returns>
    public bool Add(CrossplaneDiffRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var key = CreateRowKey(record);
        if (_rows.TryGetValue(key, out var existing))
        {
            existing.Update(record);
            return false;
        }

        if (_rows.Count >= _maximumRowCount)
        {
            return false;
        }

        var groupKey = CreateGroupKey(record);
        if (!_rowsByGroup.TryGetValue(groupKey, out var groupRows))
        {
            groupRows = new GroupRows(new CrossplaneDiffInstanceGroup());
            _rowsByGroup.Add(groupKey, groupRows);
        }

        var row = new CrossplaneDiffRow(record, groupRows.InstanceGroup);
        _rows.Add(key, row);
        groupRows.Rows.Add(row);
        groupRows.InstanceGroup.Count = groupRows.Rows.Count;

        return true;
    }

    /// <summary>Adds or updates a row and returns snapshots for the affected diff group.</summary>
    /// <returns>An empty collection when the retention limit prevents adding a new row.</returns>
    public IReadOnlyList<CrossplaneDiffRow> AddAndGetAffectedRows(CrossplaneDiffRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        var rowKey = CreateRowKey(record);
        if (!_rows.ContainsKey(rowKey) && _rows.Count >= _maximumRowCount)
        {
            return [];
        }

        var added = Add(record);
        var groupRows = _rowsByGroup[CreateGroupKey(record)].Rows;
        if (!added)
        {
            return [_rows[CreateRowKey(record)].Snapshot()];
        }

        return groupRows.Select(row => row.Snapshot()).ToArray();
    }

    /// <summary>Adds or updates a row and returns its detached snapshot when retained.</summary>
    /// <param name="record">The parsed diff record to aggregate.</param>
    /// <param name="added">Whether this record created a new distinct row.</param>
    /// <returns>The row snapshot, or <see langword="null"/> when the distinct-row limit is reached.</returns>
    public CrossplaneDiffRow? AddAndGetRowSnapshot(CrossplaneDiffRecord record, out bool added)
    {
        ArgumentNullException.ThrowIfNull(record);
        var key = CreateRowKey(record);
        if (!_rows.ContainsKey(key) && _rows.Count >= _maximumRowCount)
        {
            added = false;
            return null;
        }

        added = Add(record);
        return _rows[key].Snapshot();
    }

    public IReadOnlyList<CrossplaneDiffRow> GetGroupSnapshot(string apiVersion, string kind, string diffField)
    {
        return _rowsByGroup.TryGetValue((apiVersion, kind, diffField), out var groupRows)
            ? groupRows.Rows.Select(row => row.Snapshot()).ToArray()
            : [];
    }

    public void Clear()
    {
        _rows.Clear();
        _rowsByGroup.Clear();
    }

    public IReadOnlyList<CrossplaneDiffRow> GetSnapshot()
    {
        return _rows.Values.Select(row => row.Snapshot()).ToArray();
    }

    public int GetInstanceCount(string apiVersion, string kind, string diffField)
        => _rowsByGroup.TryGetValue((apiVersion, kind, diffField), out var groupRows)
            ? groupRows.InstanceGroup.Count
            : 0;

    private static string CreateRowKey(CrossplaneDiffRecord record)
        => string.Join("\u0000", record.Uid, record.Name, record.Namespace, record.ApiVersion, record.Kind, record.DiffField);

    private static (string ApiVersion, string Kind, string DiffField) CreateGroupKey(CrossplaneDiffRecord record)
        => (record.ApiVersion, record.Kind, record.DiffField);
}
