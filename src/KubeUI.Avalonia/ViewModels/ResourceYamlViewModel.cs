using System.Text;
using Avalonia.Controls.Notifications;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using k8s;
using k8s.Models;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;
using KubernetesClient.Informer.Client;

namespace KubeUI.Avalonia.ViewModels;

public partial class ResourceYamlViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<ResourceYamlViewModel> _logger;
    private readonly INotificationManager _notificationManager;
    private readonly IKubernetesYamlSerializer _yamlSerializer;

    [ObservableProperty]
    public partial ClusterWorkspaceViewModel? Cluster { get; set; }

    [ObservableProperty]
    public partial IKubernetesObject<V1ObjectMeta>? Object { get; set; }

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

    public ResourceYamlViewModel()
    {
        Title = Assets.Resources.ResourceYamlViewModel_Title;
        _logger = Application.Current.GetRequiredService<ILogger<ResourceYamlViewModel>>();
        _notificationManager = Application.Current.GetRequiredService<INotificationManager>();
        _yamlSerializer = Application.Current.GetRequiredService<IKubernetesYamlSerializer>();
        Settings = Application.Current.GetRequiredService<ISettingsService>();
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
            SetYamlDocument();
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
        try
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(YamlDocument.Text);
            await using MemoryStream stream = new(byteArray);
            await Cluster!.ImportYaml(stream);
        }
        catch (Exception ex)
        {
            Utilities.HandleException(_logger, _notificationManager, ex, "Error Saving Yaml", sendNotification: true);
        }
    }

    private bool CanSave()
    {
        return true;
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

    private bool CanUndo()
    {
        return YamlDocument?.UndoStack.CanUndo == true;
    }

    public void Dispose()
    {
        if (Cluster != null)
        {
            Cluster.OnChange -= Cluster_OnChange;
        }
    }
}




