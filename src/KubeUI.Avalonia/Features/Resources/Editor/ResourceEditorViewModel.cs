using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using FluentAvalonia.UI.Controls;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Avalonia.Features.Resources.Editor;

public sealed partial class ResourceEditorViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<ResourceEditorViewModel> _logger;
    private GroupApiVersionKind _kind;
    private CancellationTokenSource? _saveCancellation;

    [ObservableProperty] public partial ClusterWorkspace? Cluster { get; set; }
    [ObservableProperty] public partial IKubernetesObject<V1ObjectMeta>? Object { get; private set; }
    [ObservableProperty] public partial ResourceEditorSchemaNode? Schema { get; private set; }
    [ObservableProperty] public partial ResourceEditorDocument? Document { get; private set; }
    [ObservableProperty] public partial IReadOnlyList<ResourceEditorValidationError> ValidationErrors { get; private set; } = [];
    [ObservableProperty] public partial bool IsSaving { get; private set; }
    [ObservableProperty] public partial string? ErrorMessage { get; private set; }
    [ObservableProperty] public partial string? ValidationMessage { get; private set; }
    [ObservableProperty] public partial string? ActionResultTitle { get; private set; }
    [ObservableProperty] public partial string? ActionResultMessage { get; private set; }
    [ObservableProperty] public partial bool ActionResultSuccess { get; private set; }
    [ObservableProperty] public partial ResourceEditorNodeViewModel? EditorRoot { get; private set; }
    public bool IsCreateMode { get; private set; }
    public bool HasActionResult => !string.IsNullOrWhiteSpace(ActionResultMessage);
    public bool HasActionSuccessResult => HasActionResult && ActionResultSuccess;
    public bool HasActionFailureResult => HasActionResult && !ActionResultSuccess;
    public FAInfoBarSeverity ActionResultSeverity => ActionResultSuccess
        ? FAInfoBarSeverity.Success
        : FAInfoBarSeverity.Error;

    public ResourceEditorViewModel(
        IKubernetesYamlSerializer serializer,
        ILogger<ResourceEditorViewModel> logger)
    {
        _ = serializer;
        _logger = logger;
        Title = "Resource Editor";
        Id = nameof(ResourceEditorViewModel);
    }

    public bool IsDirty => Document?.IsDirty == true;
    public bool CanSave => !IsSaving && IsDirty && ValidationErrors.Count == 0 && (IsCreateMode ? CanCreate : CanUpdate);
    public bool CanUpdate => Cluster is not null && Object is not null
        && Cluster.Runtime.Permissions.CanI(_kind, Verb.Update, Object.Metadata?.NamespaceProperty);

    public bool CanCreate => Cluster is not null && Object is not null
        && Cluster.Runtime.Permissions.CanI(_kind, Verb.Create, Object.Metadata?.NamespaceProperty);

    public void Initialize(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource)
        => InitializeCore(cluster, resource, false);

    public void InitializeNew(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource)
        => InitializeCore(cluster, resource, true);

    private void InitializeCore(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource, bool isNew)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(resource);
        Cluster?.Runtime.OnChange -= OnClusterChange;
        Cluster = cluster;
        Object = resource;
        IsCreateMode = isNew;
        if (!cluster.Runtime.ModelCatalog.TryGetResourceKind(resource, out _kind))
            throw new InvalidOperationException($"Unknown Kubernetes resource {resource.ApiVersion}/{resource.Kind}.");
        Schema = ResourceEditorSchemaNode.CreateRoot(_kind, cluster.Runtime.ModelCatalog.OpenApiSchemas);
        Document = ResourceEditorDocument.Parse(KubernetesJson.Serialize(resource));
        Id = isNew
            ? $"{nameof(ResourceEditorViewModel)}-new-{cluster.Runtime.Name}-{_kind}/{Guid.NewGuid():N}"
            : $"{nameof(ResourceEditorViewModel)}-{cluster.Runtime.Name}-{_kind}/{resource.Namespace()}/{resource.Name()}";
        Title = isNew ? $"Create {resource.Kind}" : $"Edit {resource.Kind}/{resource.Name()}";
        Cluster.Runtime.OnChange += OnClusterChange;
        Validate();
        RebuildEditorTree();
        NotifyStateChanged();
    }

    public void SetValue(string path, JsonNode? value)
    {
        if (Document is null)
            throw new InvalidOperationException("Resource editor has not been initialized.");
        SetPath(Document.Root, path, value);
        Validate();
        NotifyStateChanged();
    }

    [RelayCommand]
    public void Reset()
    {
        Document?.Reset();
        RebuildEditorTree();
        Validate();
        NotifyStateChanged();
    }

    [RelayCommand]
    public async Task ValidateNow()
    {
        ErrorMessage = null;
        Validate();
        if (ValidationErrors.Count > 0)
        {
            ErrorMessage = string.Join(Environment.NewLine, ValidationErrors.Select(error => $"{error.Path}: {error.Message}"));
            SetActionResult(false, Assets.Resources.ResourceEditorView_ValidationFailed, ErrorMessage);
            NotifyStateChanged();
            return;
        }

        if (Cluster is null || Document is null)
        {
            ErrorMessage = "Resource editor has not been initialized.";
            SetActionResult(false, Assets.Resources.ResourceEditorView_ValidationFailed, ErrorMessage);
            NotifyStateChanged();
            return;
        }

        try
        {
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Document.Root.ToJsonString()));
            await Cluster.Runtime.DryRunYaml(stream);
            ValidationMessage = Assets.Resources.ResourceEditorView_ValidationSucceeded;
            SetActionResult(
                true,
                Assets.Resources.ResourceEditorView_ValidationSucceeded,
                Assets.Resources.ResourceEditorView_ValidationSucceededMessage);
        }
        catch (Exception ex)
        {
            var message = Utilities.GetUserFacingErrorMessage(ex);
            ErrorMessage = message;
            var serverErrors = Utilities.GetKubernetesStatusCauses(ex)
                .Select(cause => new ResourceEditorValidationError(cause.Path, cause.Message))
                .ToArray();
            if (serverErrors.Length == 0 && EditorRoot is not null)
            {
                var matchingNode = EnumerateNodes(EditorRoot)
                    .Where(node => !string.IsNullOrEmpty(node.Path)
                        && message.Contains(node.Path, StringComparison.Ordinal))
                    .OrderByDescending(node => node.Path.Length)
                    .FirstOrDefault();
                if (matchingNode is not null)
                    serverErrors = [new(matchingNode.Path, message)];
            }
            var allErrors = new List<ResourceEditorValidationError>();
            EditorRoot?.AppendLocalValidationErrors(allErrors);
            allErrors.AddRange(serverErrors);
            ValidationErrors = allErrors;
            EditorRoot?.SetDocumentValidationErrors(serverErrors);
            var summary = allErrors.Count == 0 ? message : FormatValidationErrors(allErrors);
            ErrorMessage = summary;
            SetActionResult(false, Assets.Resources.ResourceEditorView_ValidationFailed, summary);
            _logger.LogError(ex, "Error validating resource editor document");
        }
        NotifyStateChanged();
    }

    private static string FormatValidationErrors(IEnumerable<ResourceEditorValidationError> errors)
        => string.Join(
            Environment.NewLine,
            errors.Select(error => $"{error.Path}: {error.Message}"));

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        if (Cluster is null || Document is null || Object is null)
            return;

        Validate();
        if (!CanSave)
            return;

        IsSaving = true;
        ErrorMessage = null;
        _saveCancellation?.Dispose();
        _saveCancellation = new CancellationTokenSource();
        try
        {
            var json = Document.Root.ToJsonString();
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            await Cluster.Runtime.ImportYaml(stream).ConfigureAwait(false);
            Document = ResourceEditorDocument.Parse(json);
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error saving resource editor document");
        }
        finally
        {
            IsSaving = false;
            NotifyStateChanged();
        }
    }

    private void OnClusterChange(WatchEventType eventType, GroupApiVersionKind kind, IKubernetesObject<V1ObjectMeta> resource)
    {
        if (Object is null || kind != _kind || resource.Name() != Object.Name() || resource.Namespace() != Object.Namespace())
            return;
        if (!IsDirty)
            Dispatcher.UIThread.Post(() => Object = resource);
    }

    private void Validate()
    {
        ValidationMessage = null;
        var errors = new List<ResourceEditorValidationError>();
        if (Document is not null && Schema is not null)
            ValidateNode(Document.Root, Schema, string.Empty, errors);
        EditorRoot?.AppendLocalValidationErrors(errors);
        ValidationErrors = errors;
        EditorRoot?.SetDocumentValidationErrors(errors);
        UpdateImmediateValidationResult();
    }

    [RelayCommand(CanExecute = nameof(CanDismissActionResult))]
    private void DismissActionResult()
    {
        ActionResultTitle = null;
        ActionResultMessage = null;
        ActionResultSuccess = false;
    }

    private bool CanDismissActionResult() => HasActionResult;

    private void SetActionResult(bool success, string title, string message)
    {
        ActionResultSuccess = success;
        ActionResultTitle = title;
        ActionResultMessage = message;
    }

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is nameof(ActionResultTitle) or nameof(ActionResultMessage) or nameof(ActionResultSuccess))
        {
            OnPropertyChanged(nameof(HasActionResult));
            OnPropertyChanged(nameof(HasActionSuccessResult));
            OnPropertyChanged(nameof(HasActionFailureResult));
            OnPropertyChanged(nameof(ActionResultSeverity));
            DismissActionResultCommand.NotifyCanExecuteChanged();
        }
    }

    private void RebuildEditorTree()
    {
        if (Document is null || Schema is null)
        {
            EditorRoot = null;
            return;
        }
        EditorRoot = ResourceEditorNodeViewModel.Create(Schema, Document.Root, () =>
        {
            Validate();
            UpdateImmediateValidationResult();
            NotifyStateChanged();
        });
        EditorRoot.SetDocumentValidationErrors(ValidationErrors);
    }

    private void UpdateImmediateValidationResult()
    {
        if (ValidationErrors.Count > 0)
        {
            var message = string.Join(
                Environment.NewLine,
                ValidationErrors.Select(error => $"{error.Path}: {error.Message}"));
            SetActionResult(false, Assets.Resources.ResourceEditorView_ValidationFailed, message);
            return;
        }

        if (HasActionFailureResult)
            DismissActionResult();
        ErrorMessage = null;
    }

    private static void ValidateNode(JsonNode? value, ResourceEditorSchemaNode schema, string path, List<ResourceEditorValidationError> errors)
    {
        var currentPath = string.IsNullOrEmpty(path) ? schema.Name : path;
        if (value is null)
        {
            if (schema.IsRequired)
                errors.Add(new(currentPath, "Value is required."));
            return;
        }
        if (schema.ValueKind == ResourceEditorValueKind.Object && value is JsonObject obj)
        {
            foreach (var child in schema.Properties.Values)
                if (obj.TryGetPropertyValue(child.Name, out var childValue))
                    ValidateNode(childValue, child, string.IsNullOrEmpty(path) ? child.Name : $"{path}.{child.Name}", errors);
            foreach (var required in schema.Required)
                if (!obj.ContainsKey(required) || obj[required] is null)
                    errors.Add(new($"{currentPath}.{required}", "Value is required."));
        }
        else if (schema.ValueKind == ResourceEditorValueKind.Object)
            errors.Add(new(currentPath, "Value must be an object."));
        else if (schema.ValueKind == ResourceEditorValueKind.Array && value is not JsonArray)
            errors.Add(new(currentPath, "Value must be an array."));
        else if (schema.ValueKind == ResourceEditorValueKind.Map && value is not JsonObject)
            errors.Add(new(currentPath, "Value must be an object."));
        else if (schema.ValueKind == ResourceEditorValueKind.Enum && value is JsonValue enumValue
            && enumValue.TryGetValue<string>(out var text) && !schema.EnumValues.Contains(text, StringComparer.Ordinal))
            errors.Add(new(currentPath, "Value is not valid for this enum."));

        if (schema.Format == "date-time"
            && value is JsonValue dateValue
            && dateValue.TryGetValue<string>(out var dateText)
            && !string.IsNullOrEmpty(dateText)
            && !DateTimeOffset.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
            errors.Add(new(currentPath, "Value must be a valid date-time."));
    }

    private static IEnumerable<ResourceEditorNodeViewModel> EnumerateNodes(ResourceEditorNodeViewModel node)
    {
        yield return node;
        foreach (var child in node.Children)
        foreach (var descendant in EnumerateNodes(child))
            yield return descendant;
    }

    private static void SetPath(JsonObject root, string path, JsonNode? value)
    {
        var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
            throw new ArgumentException("Path cannot be empty.", nameof(path));
        var current = root;
        for (var i = 0; i < segments.Length - 1; i++)
            current = current[segments[i]] as JsonObject ?? throw new KeyNotFoundException(path);
        current[segments[^1]] = value;
    }

    private void NotifyStateChanged()
    {
        OnPropertyChanged(nameof(IsDirty));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(CanUpdate));
        OnPropertyChanged(nameof(CanCreate));
        SaveCommand.NotifyCanExecuteChanged();
    }

    public void Dispose()
    {
        if (Cluster is not null)
            Cluster.Runtime.OnChange -= OnClusterChange;
        _saveCancellation?.Dispose();
    }
}
