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
    bool Sensitive);

/// <summary>Aggregated row shown by the MR Diff Detection view.</summary>
public sealed class CrossplaneDiffRow
{
    public CrossplaneDiffRow(CrossplaneDiffRecord record)
    {
        Uid = record.Uid;
        Name = record.Name;
        Namespace = record.Namespace;
        ApiVersion = record.ApiVersion;
        Kind = record.Kind;
        DiffField = record.DiffField;
        OldValue = record.OldValue;
        NewValue = record.NewValue;
        NewComputed = record.NewComputed;
        NewRemoved = record.NewRemoved;
        RequiresNew = record.RequiresNew;
        Sensitive = record.Sensitive;
        Occurrences = 1;
        InstanceCount = 1;
    }

    public string Uid { get; }
    public string Name { get; }
    public string Namespace { get; }
    public string ApiVersion { get; }
    public string Kind { get; }
    public string DiffField { get; }
    public string OldValue { get; private set; }
    public string NewValue { get; private set; }
    public bool NewComputed { get; private set; }
    public bool NewRemoved { get; private set; }
    public bool RequiresNew { get; private set; }
    public bool Sensitive { get; private set; }
    public int Occurrences { get; private set; }
    public int InstanceCount { get; internal set; }

    internal void Update(CrossplaneDiffRecord record)
    {
        OldValue = record.OldValue;
        NewValue = record.NewValue;
        NewComputed = record.NewComputed;
        NewRemoved = record.NewRemoved;
        RequiresNew = record.RequiresNew;
        Sensitive = record.Sensitive;
        Occurrences++;
    }
}

/// <summary>Aggregates repeated diff events without retaining raw log text.</summary>
public sealed class CrossplaneDiffAggregator
{
    private readonly Dictionary<string, CrossplaneDiffRow> _rows = new(StringComparer.Ordinal);

    public IReadOnlyCollection<CrossplaneDiffRow> Rows => _rows.Values;

    public bool Add(CrossplaneDiffRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var key = string.Join("\u0000", record.Uid, record.Name, record.Namespace, record.ApiVersion, record.Kind, record.DiffField);
        if (_rows.TryGetValue(key, out var row))
        {
            row.Update(record);
            UpdateInstanceCounts();
            return false;
        }

        _rows.Add(key, new CrossplaneDiffRow(record));
        UpdateInstanceCounts();
        return true;
    }

    public void Clear() => _rows.Clear();

    private void UpdateInstanceCounts()
    {
        foreach (var row in _rows.Values)
        {
            row.InstanceCount = GetInstanceCount(row.ApiVersion, row.Kind, row.DiffField);
        }
    }

    public int GetInstanceCount(string apiVersion, string kind, string diffField)
    {
        return _rows.Values.Count(row =>
            string.Equals(row.ApiVersion, apiVersion, StringComparison.Ordinal)
            && string.Equals(row.Kind, kind, StringComparison.Ordinal)
            && string.Equals(row.DiffField, diffField, StringComparison.Ordinal));
    }
}
