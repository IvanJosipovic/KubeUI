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

/// <summary>Aggregated row shown by the MR Diff Detection view.</summary>
public sealed class CrossplaneDiffRow
{
    private string _oldValue;
    private string _newValue;
    private bool _newComputed;
    private bool _newRemoved;
    private bool _requiresNew;
    private int _occurrences;
    private int _instanceCount;

    public CrossplaneDiffRow(CrossplaneDiffRecord record)
    {
        Uid = record.Uid;
        Name = record.Name;
        Namespace = record.Namespace;
        ApiVersion = record.ApiVersion;
        Kind = record.Kind;
        DiffField = record.DiffField;
        _oldValue = record.OldValue;
        _newValue = record.NewValue;
        _newComputed = record.NewComputed;
        _newRemoved = record.NewRemoved;
        _requiresNew = record.RequiresNew;
        Sensitive = record.Sensitive;
        _occurrences = 1;
        _instanceCount = 1;
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
    public int InstanceCount => _instanceCount;

    internal void Update(CrossplaneDiffRecord record)
    {
        _oldValue = record.OldValue;
        _newValue = record.NewValue;
        _newComputed = record.NewComputed;
        _newRemoved = record.NewRemoved;
        _requiresNew = record.RequiresNew;
        Sensitive = record.Sensitive;
        _occurrences++;
    }

    internal void SetInstanceCount(int value) => _instanceCount = value;

    internal CrossplaneDiffRow Snapshot()
    {
        CrossplaneDiffRow copy = new(new CrossplaneDiffRecord(
            Uid, Name, Namespace, ApiVersion, Kind, DiffField,
            OldValue, NewValue, NewComputed, NewRemoved, RequiresNew, Sensitive));
        copy._occurrences = Occurrences;
        copy._instanceCount = InstanceCount;
        return copy;
    }

}

/// <summary>Aggregates repeated diff events without retaining raw log text.</summary>
public sealed class CrossplaneDiffAggregator
{
    private readonly Dictionary<string, CrossplaneDiffRow> _rows = new(StringComparer.Ordinal);
    private readonly Dictionary<(string ApiVersion, string Kind, string DiffField), List<CrossplaneDiffRow>> _rowsByGroup = [];

    public IReadOnlyCollection<CrossplaneDiffRow> Rows => _rows.Values;

    public bool Add(CrossplaneDiffRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var key = CreateRowKey(record);
        if (_rows.TryGetValue(key, out var existing))
        {
            existing.Update(record);
            return false;
        }

        var row = new CrossplaneDiffRow(record);
        _rows.Add(key, row);
        var groupKey = CreateGroupKey(record);
        if (!_rowsByGroup.TryGetValue(groupKey, out var group))
        {
            group = [];
            _rowsByGroup.Add(groupKey, group);
        }

        group.Add(row);
        foreach (var groupRow in group)
        {
            groupRow.SetInstanceCount(group.Count);
        }

        return true;
    }

    public IReadOnlyList<CrossplaneDiffRow> AddAndGetAffectedRows(CrossplaneDiffRecord record)
    {
        var added = Add(record);
        var group = _rowsByGroup[CreateGroupKey(record)];
        if (!added)
        {
            return [_rows[CreateRowKey(record)].Snapshot()];
        }

        return group.Select(row => row.Snapshot()).ToArray();
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
    {
        return _rowsByGroup.TryGetValue((apiVersion, kind, diffField), out var rows) ? rows.Count : 0;
    }

    private static string CreateRowKey(CrossplaneDiffRecord record)
        => string.Join("\u0000", record.Uid, record.Name, record.Namespace, record.ApiVersion, record.Kind, record.DiffField);

    private static (string ApiVersion, string Kind, string DiffField) CreateGroupKey(CrossplaneDiffRecord record)
        => (record.ApiVersion, record.Kind, record.DiffField);
}
