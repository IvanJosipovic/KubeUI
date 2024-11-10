using System.Text;
using Avalonia.Controls.Notifications;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using k8s;
using k8s.Models;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public partial class ResourceYamlViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<ResourceYamlViewModel> _logger;
    private readonly INotificationManager _notificationManager;

    [ObservableProperty]
    private ICluster? _cluster;

    [ObservableProperty]
    private IKubernetesObject<V1ObjectMeta>? _object;

    [ObservableProperty]
    private TextDocument _yamlDocument = new();

    [ObservableProperty]
    private bool _editMode;

    [ObservableProperty]
    private bool _hideNoisyFields = true;

    [ObservableProperty]
    private bool _wordWrap;

    [ObservableProperty]
    private Vector _scrollOffset;

    [ObservableProperty]
    private IEnumerable<FoldingSection> _allFoldings;

    public ResourceYamlViewModel()
    {
        Title = Resources.ResourceYamlViewModel_Title;
        _logger = Application.Current.GetRequiredService<ILogger<ResourceYamlViewModel>>();
        _notificationManager = Application.Current.GetRequiredService<INotificationManager>();
    }

    public void Initialize(ICluster cluster, IKubernetesObject<V1ObjectMeta> @object)
    {
        Cluster = cluster;
        Object = @object;

        Id = $"{nameof(ResourceYamlViewModel)}-{Cluster.Name}-{Object.ApiVersion}/{Object.Kind}/{Object.Metadata.NamespaceProperty}/{Object.Metadata.Name}";
    }

    private void SetYamlDocument()
    {
        if (!EditMode && HideNoisyFields)
        {
            var ObjectClone = Utilities.CloneObject(Object);

            if (ObjectClone.Metadata != null)
            {
                ObjectClone.Metadata.ManagedFields = null;
            }

            ObjectClone?.Metadata?.Annotations?.Remove("kubectl.kubernetes.io/last-applied-configuration");

            YamlDocument.Text = Client.Serialization.KubernetesYaml.Serialize(ObjectClone);
        }
        else
        {
            YamlDocument.Text = Client.Serialization.KubernetesYaml.Serialize(Object);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Object) || e.PropertyName == nameof(EditMode) || e.PropertyName == nameof(HideNoisyFields))
        {
            SetYamlDocument();
        }

        if (e.PropertyName == nameof(Cluster))
        {
            Cluster.OnChange += Cluster_OnChange;
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
            await using MemoryStream stream = new MemoryStream(byteArray);
            await Cluster.ImportYaml(stream);
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

    [RelayCommand(CanExecute = nameof(CanSetHideNoisyFields))]
    private void SetHideNoisyFields()
    {
        HideNoisyFields = !HideNoisyFields;
    }

    private bool CanSetHideNoisyFields()
    {
        return true;
    }

    [RelayCommand(CanExecute = nameof(CanSetEditMode))]
    private void SetEditMode()
    {
        EditMode = !EditMode;
    }

    private bool CanSetEditMode()
    {
        return true;
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
