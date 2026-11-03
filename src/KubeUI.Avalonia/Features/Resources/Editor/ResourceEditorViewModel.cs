using KubeUI.Kubernetes.Serialization;
using System.Globalization;
using System.Net;
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
using KubeUI.Kubernetes.Resources.Editor;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Avalonia.Features.Resources.Editor;

public sealed partial class ResourceEditorViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<ResourceEditorViewModel> _logger;
    private readonly IKubernetesYamlSerializer _yamlSerializer;
    private GroupApiVersionKind _kind;

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

    public ResourceEditorViewModel(ILogger<ResourceEditorViewModel> logger, IKubernetesYamlSerializer yamlSerializer)
    {
        _logger = logger;
        _yamlSerializer = yamlSerializer;
        Title = "Resource Editor";
        Id = nameof(ResourceEditorViewModel);
    }

    public bool IsDirty => Document?.IsDirty == true;
    public bool CanSave => !IsSaving && (IsCreateMode || IsDirty) && ValidationErrors.Count == 0 && (IsCreateMode ? CanCreate : CanUpdate);
    public bool CanUpdate => Cluster is not null && Object is not null
        && Cluster.Runtime.Permissions.CanI(_kind, Verb.Update, Object.Metadata?.NamespaceProperty);

    public bool CanCreate => Cluster is not null && Object is not null
        && Cluster.Runtime.Permissions.CanI(_kind, Verb.Create, Object.Metadata?.NamespaceProperty);

    /// <summary>Initializes the editor for an existing resource.</summary>
    public void Initialize(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource)
        => InitializeCore(cluster, resource, false);

    /// <summary>Initializes the editor to create a resource of the same kind.</summary>
    public void InitializeForCreate(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource)
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
            LogValidationFailed(ex);
        }
        NotifyStateChanged();
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Error validating resource editor document.")]
    private partial void LogValidationFailed(Exception exception);

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
        try
        {
            var json = Document.Root.ToJsonString();

            // The resource is applied as a single object so the server-assigned UID and
            // resource version come back and are written into the document. Without them a
            // second save would try to create the resource again instead of replacing it.
            var applied = await Cluster.Runtime.ApplyResource(ToResource(json)).ConfigureAwait(false);

            // The saved JSON becomes the new baseline. The editor tree holds closures over the
            // current JsonObject instances, so the tree must be rebuilt against the new baseline
            // or subsequent edits would keep mutating the discarded document.
            Document = ResourceEditorDocument.Parse(KubernetesJson.Serialize(applied));
            Object = applied;
            if (IsCreateMode)
            {
                IsCreateMode = false;
                OnPropertyChanged(nameof(IsCreateMode));
            }

            RebuildEditorTree();
            Validate();
            SetActionResult(
                true,
                Assets.Resources.ResourceEditorView_SaveSucceeded,
                Assets.Resources.ResourceEditorView_SaveSucceededMessage);
        }
        catch (Exception ex)
        {
            var message = Utilities.GetUserFacingErrorMessage(ex);
            ErrorMessage = message;
            LogSaveFailed(ex);

            var serverErrors = Utilities.GetKubernetesStatusCauses(ex)
                .Select(cause => new ResourceEditorValidationError(cause.Path, cause.Message))
                .ToArray();
            if (serverErrors.Length > 0)
            {
                var allErrors = new List<ResourceEditorValidationError>();
                EditorRoot?.AppendLocalValidationErrors(allErrors);
                allErrors.AddRange(serverErrors);
                ValidationErrors = allErrors;
                EditorRoot?.SetDocumentValidationErrors(serverErrors);
            }

            SetActionResult(
                false,
                IsConflict(ex)
                    ? Assets.Resources.ResourceEditorView_SaveConflict
                    : Assets.Resources.ResourceEditorView_SaveFailed,
                message);
        }
        finally
        {
            IsSaving = false;
            NotifyStateChanged();
        }
    }

    private static bool IsConflict(Exception ex)
        => Utilities.GetKubernetesStatusCode(ex) == (int)HttpStatusCode.Conflict;

    /// <summary>
    /// Converts the edited document back into the resource type that is being edited.
    /// </summary>
    /// <param name="json">The edited document JSON.</param>
    /// <returns>The resource represented by the document.</returns>
    private GenericKubernetesObject ToResource(string json)
        => KubernetesJson.Deserialize<GenericKubernetesObject>(json);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Error saving resource editor document.")]
    private partial void LogSaveFailed(Exception exception);

    private void OnClusterChange(WatchEventType eventType, GroupApiVersionKind kind, IKubernetesObject<V1ObjectMeta> resource)
    {
        if (eventType is not (WatchEventType.Added or WatchEventType.Modified))
            return;

        Dispatcher.UIThread.Post(() =>
        {
            var currentObject = Object;
            if (currentObject is null
                || IsCreateMode
                || _kind != kind
                || currentObject.Name() != resource.Name()
                || currentObject.Namespace() != resource.Namespace()
                || IsDirty)
                return;

            Object = resource;
            Document = ResourceEditorDocument.Parse(KubernetesJson.Serialize(resource));
            RebuildEditorTree();
            Validate();
            NotifyStateChanged();
        });
    }

    private void Validate()
    {
        ValidationMessage = null;
        var errors = new List<ResourceEditorValidationError>();
        if (Document is not null && Schema is not null)
            ValidateNode(Document.Root, Schema, string.Empty, isRequired: false, errors);
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

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
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
        EditorRoot = ResourceEditorNodeViewModel.Create(_yamlSerializer, Schema, Document.Root, () =>
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

    private static void ValidateNode(
        JsonNode? value,
        ResourceEditorSchemaNode schema,
        string path,
        bool isRequired,
        List<ResourceEditorValidationError> errors)
    {
        if (value is null)
        {
            if (isRequired)
                errors.Add(new(path, "Value is required."));
            return;
        }

        if (schema.ValueKind == ResourceEditorValueKind.ObjectKind && value is JsonObject obj)
        {
            foreach (var child in schema.Properties.Values)
            {
                obj.TryGetPropertyValue(child.Name, out var childValue);
                ValidateNode(
                    childValue,
                    child,
                    JoinPath(path, child.Name),
                    schema.Required.Contains(child.Name),
                    errors);
            }

            foreach (var required in schema.Required)
            {
                if (!obj.ContainsKey(required) || obj[required] is null)
                    errors.Add(new(JoinPath(path, required), "Value is required."));
            }
        }
        else if (schema.ValueKind == ResourceEditorValueKind.ObjectKind)
        {
            errors.Add(new(path, "Value must be an object."));
        }
        else if (schema.ValueKind == ResourceEditorValueKind.ArrayKind)
        {
            if (value is not JsonArray array)
            {
                errors.Add(new(path, "Value must be an array."));
            }
            else if (schema.Items is not null)
            {
                for (var index = 0; index < array.Count; index++)
                    ValidateNode(array[index], schema.Items, $"{path}[{index}]", isRequired: false, errors);
            }
        }
        else if (schema.ValueKind == ResourceEditorValueKind.MapKind)
        {
            if (value is not JsonObject map)
            {
                errors.Add(new(path, "Value must be an object."));
            }
            else if (schema.Items is not null)
            {
                foreach (var entry in map)
                    ValidateNode(entry.Value, schema.Items, JoinPath(path, entry.Key), isRequired: false, errors);
            }
        }
        else if (schema.ValueKind == ResourceEditorValueKind.EnumKind
            && value is JsonValue enumValue
            && enumValue.TryGetValue<string>(out var text)
            && !schema.EnumValues.Contains(text, StringComparer.Ordinal))
        {
            errors.Add(new(path, "Value is not valid for this enum."));
        }

        if (schema.Format == "date-time"
            && value is JsonValue dateValue
            && dateValue.TryGetValue<string>(out var dateText)
            && !string.IsNullOrEmpty(dateText)
            && !DateTimeOffset.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
            errors.Add(new(path, "Value must be a valid date-time."));
    }

    private static string JoinPath(string path, string name)
        => string.IsNullOrEmpty(path) ? name : $"{path}.{name}";

    private static IEnumerable<ResourceEditorNodeViewModel> EnumerateNodes(ResourceEditorNodeViewModel node)
    {
        yield return node;
        foreach (var child in node.Children)
            foreach (var descendant in EnumerateNodes(child))
                yield return descendant;
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
    }
}
