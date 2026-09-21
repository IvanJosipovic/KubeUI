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
    bool RequiresNew);

/// <summary>Aggregated row shown by the MR Diff Detection view.</summary>
public sealed class CrossplaneDiffRow : System.ComponentModel.INotifyPropertyChanged
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
    public int Occurrences => _occurrences;
    public int InstanceCount => _instanceCount;

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    internal void Update(CrossplaneDiffRecord record)
    {
        _oldValue = record.OldValue;
        _newValue = record.NewValue;
        _newComputed = record.NewComputed;
        _newRemoved = record.NewRemoved;
        _requiresNew = record.RequiresNew;
        _occurrences++;
    }

    internal void SetInstanceCount(int value) => _instanceCount = value;

    internal CrossplaneDiffRow Snapshot()
    {
        CrossplaneDiffRow copy = new(new CrossplaneDiffRecord(
            Uid, Name, Namespace, ApiVersion, Kind, DiffField,
            OldValue, NewValue, NewComputed, NewRemoved, RequiresNew));
        copy._occurrences = Occurrences;
        copy._instanceCount = InstanceCount;
        return copy;
    }

    public void Apply(CrossplaneDiffRow source)
    {
        Set(ref _oldValue, source.OldValue, nameof(OldValue));
        Set(ref _newValue, source.NewValue, nameof(NewValue));
        Set(ref _newComputed, source.NewComputed, nameof(NewComputed));
        Set(ref _newRemoved, source.NewRemoved, nameof(NewRemoved));
        Set(ref _requiresNew, source.RequiresNew, nameof(RequiresNew));
        Set(ref _occurrences, source.Occurrences, nameof(Occurrences));
        Set(ref _instanceCount, source.InstanceCount, nameof(InstanceCount));
    }

    private void Set<T>(ref T field, T value, string propertyName)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
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
                UpdateInstanceCounts(record.ApiVersion, record.Kind, record.DiffField);
                return false;
            }

            _rows.Add(key, new CrossplaneDiffRow(record));
            UpdateInstanceCounts(record.ApiVersion, record.Kind, record.DiffField);
        return true;
    }

    public IReadOnlyList<CrossplaneDiffRow> AddAndGetAffectedRows(CrossplaneDiffRecord record)
    {
        Add(record);
        return _rows.Values
            .Where(row => string.Equals(row.ApiVersion, record.ApiVersion, StringComparison.Ordinal)
                && string.Equals(row.Kind, record.Kind, StringComparison.Ordinal)
                && string.Equals(row.DiffField, record.DiffField, StringComparison.Ordinal))
            .Select(row => row.Snapshot())
            .ToArray();
    }

    public void Clear() => _rows.Clear();

    public IReadOnlyList<CrossplaneDiffRow> GetSnapshot()
    {
        return _rows.Values.Select(row => row.Snapshot()).ToArray();
    }

    private void UpdateInstanceCounts(string apiVersion, string kind, string diffField)
    {
        foreach (var row in _rows.Values)
        {
            if (string.Equals(row.ApiVersion, apiVersion, StringComparison.Ordinal)
                && string.Equals(row.Kind, kind, StringComparison.Ordinal)
                && string.Equals(row.DiffField, diffField, StringComparison.Ordinal))
            {
                row.SetInstanceCount(GetInstanceCount(apiVersion, kind, diffField));
            }
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
