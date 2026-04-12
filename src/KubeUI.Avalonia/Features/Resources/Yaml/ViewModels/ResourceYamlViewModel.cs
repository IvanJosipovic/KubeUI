using System.Text;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using FluentAvalonia.UI.Controls;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Features.Resources.Yaml.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Avalonia.Features.Resources.Yaml.ViewModels;

public partial class ResourceYamlViewModel : ViewModelBase, IDisposable
{
    public event EventHandler? CompletionRequested;

    private readonly ILogger<ResourceYamlViewModel> _logger;
    private readonly IKubernetesYamlSerializer _yamlSerializer;
    private readonly IYamlValidationService _yamlValidationService;
    private TextDocument? _validationDocument;
    private bool _actionResultFromValidation;
    private CancellationTokenSource? _validationDebounceCts;

    internal TimeSpan ValidationDebounceDelay { get; set; } = TimeSpan.FromSeconds(1);

    [ObservableProperty]
    public partial ClusterWorkspaceViewModel? Cluster { get; set; }

    private IKubernetesObject<V1ObjectMeta>? _object;

    public IKubernetesObject<V1ObjectMeta>? Object
    {
        get => _object;
        set
        {
            _object = value;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    public partial TextDocument YamlDocument { get; set; } = new();

    [ObservableProperty]
    public partial bool EditMode { get; set; }

    [ObservableProperty]
    public partial bool HideNoisyFields { get; set; } = true;

    [ObservableProperty]
    public partial bool WordWrap { get; set; }

    [ObservableProperty]
    public partial Vector ScrollOffset { get; set; }

    [ObservableProperty]
    public partial IEnumerable<NewFolding> AllFoldings { get; set; }

    [ObservableProperty]
    public partial ISettingsService Settings { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<YamlDiagnostic> ValidationDiagnostics { get; set; } = [];

    [ObservableProperty]
    public partial string? ActionResultTitle { get; set; }

    [ObservableProperty]
    public partial string? ActionResultMessage { get; set; }

    [ObservableProperty]
    public partial bool ActionResultSuccess { get; set; }

    public bool CanSaveAction => EditMode && ValidationDiagnostics.Count == 0;

    public bool CanDryRunAction => EditMode && ValidationDiagnostics.Count == 0;

    public bool HasActionResult => !string.IsNullOrWhiteSpace(ActionResultMessage);

    public bool HasActionSuccessResult => HasActionResult && ActionResultSuccess;

    public bool HasActionFailureResult => HasActionResult && !ActionResultSuccess;

    public InfoBarSeverity ActionResultSeverity => ActionResultSuccess ? InfoBarSeverity.Success : InfoBarSeverity.Error;

    public ResourceYamlViewModel(
        ILogger<ResourceYamlViewModel> logger,
        IKubernetesYamlSerializer yamlSerializer,
        IYamlValidationService yamlValidationService,
        ISettingsService settings)
    {
        Title = Assets.Resources.ResourceYamlViewModel_Title;
        _logger = logger;
        _yamlSerializer = yamlSerializer;
        _yamlValidationService = yamlValidationService;
        Settings = settings;
        AttachValidationDocument(YamlDocument);
    }

    public void Initialize(ClusterWorkspaceViewModel cluster, IKubernetesObject<V1ObjectMeta> @object)
    {
        Cluster = cluster;
        Cluster.OnChange += Cluster_OnChange;
        Object = @object;

        Id = $"{nameof(ResourceYamlViewModel)}-{Cluster.Name}-{Object.ApiVersion}/{Object.Kind}/{Object.Metadata.NamespaceProperty}/{Object.Metadata.Name}";
    }

    private void SetYamlDocument()
    {
        if (!EditMode && HideNoisyFields)
        {
            var objectClone = Utilities.CloneObject(Object);

            objectClone.Metadata?.ManagedFields = null;
            objectClone.Metadata?.Annotations?.Remove("kubectl.kubernetes.io/last-applied-configuration");

            YamlDocument.Text = _yamlSerializer.Serialize(objectClone);
        }
        else
        {
            YamlDocument.Text = _yamlSerializer.Serialize(Object!);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Object) || e.PropertyName == nameof(EditMode) || e.PropertyName == nameof(HideNoisyFields))
        {
            if (e.PropertyName == nameof(EditMode) && !EditMode)
            {
                ClearActionResult();
            }

            SetYamlDocument();
        }

        if (e.PropertyName == nameof(YamlDocument))
        {
            AttachValidationDocument(YamlDocument);
            ValidateYamlDocument();
        }

        if (e.PropertyName is nameof(EditMode) or nameof(ValidationDiagnostics))
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CanSaveAction)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CanDryRunAction)));
            SaveCommand.NotifyCanExecuteChanged();
            DryRunCommand.NotifyCanExecuteChanged();
        }

        if (e.PropertyName is nameof(ActionResultTitle) or nameof(ActionResultMessage) or nameof(ActionResultSuccess))
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasActionResult)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasActionSuccessResult)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasActionFailureResult)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActionResultSeverity)));
            DismissActionResultCommand.NotifyCanExecuteChanged();
        }
    }

    private void Cluster_OnChange(WatchEventType eventType, GroupApiVersionKind groupApiVersionKind, IKubernetesObject<V1ObjectMeta> resource)
    {
        if (Object != null
            && Object.Kind == resource.Kind
            && Object.ApiVersion == resource.ApiVersion
            && Object.Metadata.Name == resource.Metadata.Name
            && Object.Metadata.NamespaceProperty == resource.Metadata.NamespaceProperty)
        {
            Dispatcher.UIThread.Post(() => Object = resource);
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        ValidateYamlDocumentNow();

        if (ValidationDiagnostics.Count > 0)
        {
            var diagnostic = ValidationDiagnostics[0];
            var message = $"{diagnostic.Message} (line {diagnostic.StartLine}, column {diagnostic.StartColumn})";
            SetActionResult(false, Assets.Resources.ResourceYamlViewer_SaveFailed, message);
            return;
        }

        try
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(YamlDocument.Text);
            await using MemoryStream stream = new(byteArray);
            await Cluster!.ImportYaml(stream);
            SetActionResult(true, Assets.Resources.ResourceYamlViewer_SaveSucceeded, Assets.Resources.ResourceYamlViewer_SaveSucceededMessage);
        }
        catch (Exception ex)
        {
            SetActionResult(false, Assets.Resources.ResourceYamlViewer_SaveFailed, Utilities.GetUserFacingErrorMessage(ex));
            _logger.LogError(ex, "Error Saving Yaml");
        }
    }

    private bool CanSave()
    {
        return EditMode && ValidationDiagnostics.Count == 0;
    }

    [RelayCommand(CanExecute = nameof(CanDryRun))]
    private async Task DryRun()
    {
        ValidateYamlDocumentNow();

        if (ValidationDiagnostics.Count > 0)
        {
            var diagnostic = ValidationDiagnostics[0];
            var message = $"{diagnostic.Message} (line {diagnostic.StartLine}, column {diagnostic.StartColumn})";
            SetActionResult(false, Assets.Resources.ResourceYamlViewer_DryRunFailed, message);
            return;
        }

        try
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(YamlDocument.Text);
            await using MemoryStream stream = new(byteArray);
            await Cluster!.DryRunYaml(stream);
            SetActionResult(true, Assets.Resources.ResourceYamlViewer_DryRunSucceeded, Assets.Resources.ResourceYamlViewer_DryRunSucceededMessage);
        }
        catch (Exception ex)
        {
            SetActionResult(false, Assets.Resources.ResourceYamlViewer_DryRunFailed, Utilities.GetUserFacingErrorMessage(ex));
            _logger.LogError(ex, "Error Dry Running Yaml");
        }
    }

    private bool CanDryRun()
    {
        return EditMode && ValidationDiagnostics.Count == 0;
    }

    [RelayCommand(CanExecute = nameof(CanDismissActionResult))]
    private void DismissActionResult()
    {
        ClearActionResult();
    }

    private bool CanDismissActionResult()
    {
        return HasActionResult;
    }

    [RelayCommand]
    private void SetHideNoisyFields()
    {
        HideNoisyFields = !HideNoisyFields;
    }

    [RelayCommand(CanExecute = nameof(CanSetEditMode))]
    private void SetEditMode()
    {
        EditMode = !EditMode;
    }

    private bool CanSetEditMode()
    {
        return Cluster!.CanI(Object!.GetType(), Verb.Update, Object?.Metadata?.NamespaceProperty);
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        if (EditMode)
        {
            YamlDocument.UndoStack.Undo();
        }
    }

    [RelayCommand]
    private void RequestCompletion()
    {
        CompletionRequested?.Invoke(this, EventArgs.Empty);
    }

    private bool CanUndo()
    {
        return YamlDocument?.UndoStack.CanUndo == true;
    }

    public void Dispose()
    {
        CancelPendingValidation();
        DetachValidationDocument();

        if (Cluster != null)
        {
            Cluster.OnChange -= Cluster_OnChange;
        }
    }

    private void AttachValidationDocument(TextDocument? document)
    {
        if (ReferenceEquals(_validationDocument, document))
        {
            return;
        }

        DetachValidationDocument();
        _validationDocument = document;

        if (_validationDocument != null)
        {
            _validationDocument.Changed += YamlDocument_OnChanged;
        }
    }

    private void DetachValidationDocument()
    {
        CancelPendingValidation();

        if (_validationDocument != null)
        {
            _validationDocument.Changed -= YamlDocument_OnChanged;
            _validationDocument = null;
        }
    }

    private void YamlDocument_OnChanged(object? sender, DocumentChangeEventArgs e)
    {
        _ = DebounceValidationAsync();
    }

    private async Task DebounceValidationAsync()
    {
        CancelPendingValidation();

        var cts = new CancellationTokenSource();
        _validationDebounceCts = cts;

        try
        {
            if (ValidationDebounceDelay > TimeSpan.Zero)
            {
                await Task.Delay(ValidationDebounceDelay, cts.Token);
            }

            if (!cts.IsCancellationRequested)
            {
                await Dispatcher.UIThread.InvokeAsync(ValidateYamlDocument);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (ReferenceEquals(_validationDebounceCts, cts))
            {
                _validationDebounceCts = null;
            }

            cts.Dispose();
        }
    }

    private void ValidateYamlDocumentNow()
    {
        CancelPendingValidation();
        ValidateYamlDocument();
    }

    private void CancelPendingValidation()
    {
        if (_validationDebounceCts == null)
        {
            return;
        }

        _validationDebounceCts.Cancel();
        _validationDebounceCts.Dispose();
        _validationDebounceCts = null;
    }

    private void ValidateYamlDocument()
    {
        ValidationDiagnostics = _yamlValidationService.Validate(YamlDocument.Text, Cluster?.ModelCache);

        if (ValidationDiagnostics.Count > 0)
        {
            var diagnostic = ValidationDiagnostics[0];
            SetActionResult(
                false,
                Assets.Resources.ResourceYamlViewer_ValidationFailed,
                $"{diagnostic.Message} (line {diagnostic.StartLine}, column {diagnostic.StartColumn})",
                fromValidation: true);
            return;
        }

        if (_actionResultFromValidation)
        {
            ClearActionResult();
        }
    }

    private void SetActionResult(bool success, string title, string message, bool fromValidation = false)
    {
        _actionResultFromValidation = fromValidation;
        ActionResultSuccess = success;
        ActionResultTitle = title;
        ActionResultMessage = message;
    }

    private void ClearActionResult()
    {
        _actionResultFromValidation = false;
        ActionResultTitle = null;
        ActionResultMessage = null;
        ActionResultSuccess = false;
    }
}




