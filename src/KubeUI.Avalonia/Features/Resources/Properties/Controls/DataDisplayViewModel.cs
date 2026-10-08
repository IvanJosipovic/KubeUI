using System.Text.Json.Nodes;
using FluentAvalonia.UI.Controls;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

internal sealed partial class DataDisplayViewModel<TResource, TValue> : ViewModelBase
    where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly GroupApiVersionKind _kind;
    private readonly Func<TResource, IEnumerable<KeyValuePair<string, TValue>>?> _dataSelector;
    private readonly Func<TValue, string> _displayFormatter;
    private readonly Func<string, string> _wireFormatter;
    private readonly Func<TValue, string>? _originalValueWireFormatter;
    private readonly Dictionary<string, string> _baseline = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TValue> _baselineValues = new(StringComparer.Ordinal);
    private TResource _resource;
    private TResource _latestResource;
    private bool _rebuildingRows;

    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    [ObservableProperty]
    public partial bool EditMode { get; set; }

    [ObservableProperty]
    public partial bool CanEdit { get; private set; }

    [ObservableProperty]
    public partial bool IsSaving { get; private set; }

    [ObservableProperty]
    public partial string? ValidationMessage { get; private set; }

    [ObservableProperty]
    public partial string? ActionResultTitle { get; private set; }

    [ObservableProperty]
    public partial string? ActionResultMessage { get; private set; }

    [ObservableProperty]
    public partial bool ActionResultSuccess { get; private set; }

    public ObservableCollection<DataDisplayRowViewModel> Rows { get; } = [];

    public bool HasChanges => HasChangesCore();

    public bool CanShowToolbar => CanEdit || EditMode;

    public bool CanSave => EditMode
        && CanEdit
        && !IsSaving
        && ValidationMessage is null
        && HasChanges;

    public bool HasActionResult => !string.IsNullOrWhiteSpace(ActionResultMessage);

    public bool HasActionSuccessResult => HasActionResult && ActionResultSuccess;

    public bool HasActionFailureResult => HasActionResult && !ActionResultSuccess;

    public FAInfoBarSeverity ActionResultSeverity => ActionResultSuccess
        ? FAInfoBarSeverity.Success
        : FAInfoBarSeverity.Error;

    internal TResource Resource => _resource;

    internal DataDisplayViewModel(
        TResource resource,
        GroupApiVersionKind kind,
        Func<TResource, IEnumerable<KeyValuePair<string, TValue>>?> dataSelector,
        Func<TValue, string> displayFormatter,
        Func<string, string> wireFormatter,
        Func<TValue, string>? originalValueWireFormatter)
    {
        _resource = resource ?? throw new ArgumentNullException(nameof(resource));
        _latestResource = resource;
        _kind = kind;
        _dataSelector = dataSelector ?? throw new ArgumentNullException(nameof(dataSelector));
        _displayFormatter = displayFormatter ?? throw new ArgumentNullException(nameof(displayFormatter));
        _wireFormatter = wireFormatter ?? throw new ArgumentNullException(nameof(wireFormatter));
        _originalValueWireFormatter = originalValueWireFormatter;

        ReplaceRows(ReadData(resource));
    }

    internal void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
        UpdateCanEdit();
        NotifyCommandStateChanged();
    }

    internal void Refresh(TResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        if (!IsSameResource(_resource, resource))
        {
            _resource = resource;
            _latestResource = resource;
            ClearActionResult();
            EditMode = false;
            ReplaceRows(ReadData(resource));
            UpdateCanEdit();
            return;
        }

        _latestResource = resource;
        if (!EditMode)
        {
            _resource = resource;
            ReplaceRows(ReadData(resource));
        }

        UpdateCanEdit();
    }

    [RelayCommand(CanExecute = nameof(CanBeginEdit))]
    private void BeginEdit()
    {
        _resource = _latestResource;
        var data = ReadData(_resource);
        _baseline.Clear();
        _baselineValues.Clear();
        foreach (var entry in data)
        {
            _baseline.Add(entry.Key, entry.Value);
        }

        foreach (var entry in _dataSelector(_resource) ?? [])
        {
            _baselineValues[entry.Key] = entry.Value;
        }

        ReplaceRows(data);
        ClearActionResult();
        EditMode = true;
    }

    private bool CanBeginEdit() => CanEdit && !EditMode && !IsSaving;

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        Rows.Add(new DataDisplayRowViewModel(string.Empty, string.Empty));
        SubscribeToRow(Rows[^1]);
        ValidateRows();
        NotifyCommandStateChanged();
    }

    private bool CanAdd() => EditMode && !IsSaving;

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private void Remove(DataDisplayRowViewModel? row)
    {
        if (row is null || !Rows.Remove(row))
        {
            return;
        }

        UnsubscribeFromRow(row);
        ValidateRows();
        NotifyCommandStateChanged();
    }

    private bool CanRemove(DataDisplayRowViewModel? row)
        => EditMode && !IsSaving && row is not null && Rows.Contains(row);

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        ValidateRows();
        if (!CanSave)
        {
            return;
        }

        if (Cluster is null
            || Cluster.Runtime.Client is null
            || _resource.Metadata is null
            || string.IsNullOrWhiteSpace(_resource.Metadata.Name)
            || string.IsNullOrWhiteSpace(_resource.Metadata.NamespaceProperty))
        {
            SetActionResult(false, Assets.Resources.DataDisplay_SaveFailed, Assets.Resources.DataDisplay_PatchUnavailable);
            return;
        }

        if (!Cluster.Runtime.Permissions.CanI(_kind, Verb.Patch, _resource.Metadata.NamespaceProperty))
        {
            SetActionResult(false, Assets.Resources.DataDisplay_SaveFailed, Assets.Resources.DataDisplay_PatchPermissionDenied);
            UpdateCanEdit();
            return;
        }

        var patchData = BuildPatchData();
        if (patchData.Count == 0)
        {
            return;
        }

        IsSaving = true;
        try
        {
            var resourceBeingSaved = _resource;
            using var client = Cluster.Runtime.Client.GetGenericClient<TResource>();
            var patch = new JsonObject
            {
                ["data"] = patchData,
            };
            var saved = await client.PatchNamespacedAsync<TResource>(
                new V1Patch(patch.ToJsonString(), V1Patch.PatchType.MergePatch),
                resourceBeingSaved.Metadata.NamespaceProperty,
                resourceBeingSaved.Metadata.Name);

            if (!IsSameResource(_resource, resourceBeingSaved))
            {
                return;
            }

            _resource = saved;
            _latestResource = saved;
            var savedData = ReadData(saved);
            _baseline.Clear();
            foreach (var entry in savedData)
            {
                _baseline.Add(entry.Key, entry.Value);
            }

            ReplaceRows(savedData);
            EditMode = true;
            SetActionResult(true, Assets.Resources.DataDisplay_SaveSucceeded, Assets.Resources.DataDisplay_SaveSucceededMessage);
        }
        catch (Exception exception)
        {
            SetActionResult(false, Assets.Resources.DataDisplay_SaveFailed, Utilities.GetUserFacingErrorMessage(exception));
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        if (!EditMode)
        {
            return;
        }

        _resource = _latestResource;
        ReplaceRows(ReadData(_resource));
        ClearActionResult();
        EditMode = false;
    }

    private bool CanCancel() => EditMode && !IsSaving;

    [RelayCommand(CanExecute = nameof(CanDismissActionResult))]
    private void DismissActionResult()
    {
        ClearActionResult();
    }

    private bool CanDismissActionResult() => HasActionResult;

    private Dictionary<string, string> ReadData(TResource resource)
    {
        Dictionary<string, string> result = new(StringComparer.Ordinal);
        var data = _dataSelector(resource);
        if (data is null)
        {
            return result;
        }

        foreach (var entry in data)
        {
            result[entry.Key] = _displayFormatter(entry.Value);
        }

        return result;
    }

    private void ReplaceRows(IReadOnlyDictionary<string, string> data)
    {
        _rebuildingRows = true;
        try
        {
            foreach (var row in Rows)
            {
                UnsubscribeFromRow(row);
            }

            Rows.Clear();
            foreach (var entry in data)
            {
                var row = new DataDisplayRowViewModel(entry.Key, entry.Value, entry.Key);
                Rows.Add(row);
                SubscribeToRow(row);
            }
        }
        finally
        {
            _rebuildingRows = false;
        }

        ValidateRows();
        NotifyCommandStateChanged();
    }

    private void SubscribeToRow(DataDisplayRowViewModel row)
    {
        row.PropertyChanged += RowOnPropertyChanged;
    }

    private void UnsubscribeFromRow(DataDisplayRowViewModel row)
    {
        row.PropertyChanged -= RowOnPropertyChanged;
    }

    private void RowOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_rebuildingRows || e.PropertyName is not (nameof(DataDisplayRowViewModel.Key) or nameof(DataDisplayRowViewModel.Value)))
        {
            return;
        }

        ValidateRows();
        NotifyCommandStateChanged();
    }

    private void ValidateRows()
    {
        string? validationMessage = null;
        HashSet<string> keys = new(StringComparer.Ordinal);
        foreach (var row in Rows)
        {
            if (string.IsNullOrWhiteSpace(row.Key) || !keys.Add(row.Key))
            {
                validationMessage = Assets.Resources.DataDisplay_InvalidKey;
                break;
            }
        }

        ValidationMessage = validationMessage;
    }

    private bool HasChangesCore()
    {
        if (!EditMode || ValidationMessage is not null)
        {
            return false;
        }

        HashSet<string> currentKeys = new(StringComparer.Ordinal);
        foreach (var row in Rows)
        {
            currentKeys.Add(row.Key);
            if (!_baseline.TryGetValue(row.Key, out var originalValue)
                || !string.Equals(originalValue, row.Value, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return _baseline.Keys.Any(key => !currentKeys.Contains(key));
    }

    private JsonObject BuildPatchData()
    {
        Dictionary<string, string> current = new(StringComparer.Ordinal);
        foreach (var row in Rows)
        {
            current[row.Key] = row.Value;
        }

        JsonObject patchData = new();
        foreach (var original in _baseline)
        {
            if (!current.TryGetValue(original.Key, out var currentValue))
            {
                patchData[original.Key] = null;
            }
            else if (!string.Equals(original.Value, currentValue, StringComparison.Ordinal))
            {
                var row = Rows.First(item => string.Equals(item.Key, original.Key, StringComparison.Ordinal));
                patchData[original.Key] = GetWireValue(row, currentValue);
            }
        }

        foreach (var currentValue in current)
        {
            if (!_baseline.ContainsKey(currentValue.Key))
            {
                var row = Rows.First(item => string.Equals(item.Key, currentValue.Key, StringComparison.Ordinal));
                patchData[currentValue.Key] = GetWireValue(row, currentValue.Value);
            }
        }

        return patchData;
    }

    private string GetWireValue(DataDisplayRowViewModel row, string value)
    {
        if (row.OriginalKey is not null
            && _originalValueWireFormatter is not null
            && _baselineValues.TryGetValue(row.OriginalKey, out var originalValue)
            && _baseline.TryGetValue(row.OriginalKey, out var originalText)
            && string.Equals(value, originalText, StringComparison.Ordinal))
        {
            return _originalValueWireFormatter(originalValue);
        }

        return _wireFormatter(value);
    }

    private void UpdateCanEdit()
    {
        CanEdit = Cluster?.Runtime.Permissions.CanI(
            _kind,
            Verb.Patch,
            _resource.Metadata?.NamespaceProperty) == true;
    }

    private void SetActionResult(bool success, string title, string message)
    {
        ActionResultSuccess = success;
        ActionResultTitle = title;
        ActionResultMessage = message;
    }

    private void ClearActionResult()
    {
        ActionResultSuccess = false;
        ActionResultTitle = null;
        ActionResultMessage = null;
    }

    private static bool IsSameResource(TResource left, TResource right)
    {
        return string.Equals(left.ApiVersion, right.ApiVersion, StringComparison.Ordinal)
            && string.Equals(left.Kind, right.Kind, StringComparison.Ordinal)
            && string.Equals(left.Metadata?.Name, right.Metadata?.Name, StringComparison.Ordinal)
            && string.Equals(left.Metadata?.NamespaceProperty, right.Metadata?.NamespaceProperty, StringComparison.Ordinal);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(EditMode)
            or nameof(CanEdit)
            or nameof(IsSaving)
            or nameof(ValidationMessage)
            or nameof(HasChanges))
        {
            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(CanShowToolbar));
            BeginEditCommand.NotifyCanExecuteChanged();
            AddCommand.NotifyCanExecuteChanged();
            RemoveCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
        }

        if (e.PropertyName is nameof(ActionResultTitle)
            or nameof(ActionResultMessage)
            or nameof(ActionResultSuccess))
        {
            OnPropertyChanged(nameof(HasActionResult));
            OnPropertyChanged(nameof(HasActionSuccessResult));
            OnPropertyChanged(nameof(HasActionFailureResult));
            OnPropertyChanged(nameof(ActionResultSeverity));
            DismissActionResultCommand.NotifyCanExecuteChanged();
        }
    }

    private void NotifyCommandStateChanged()
    {
        OnPropertyChanged(nameof(HasChanges));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(CanShowToolbar));
        BeginEditCommand.NotifyCanExecuteChanged();
        AddCommand.NotifyCanExecuteChanged();
        RemoveCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }
}
